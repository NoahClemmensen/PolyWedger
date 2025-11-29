using System.Runtime.InteropServices;
using Windows.UI.Input.Preview.Injection;

namespace RGEInterfacerTest;

public sealed class RawInputHidSender
{
    private const int AbsoluteRange = 65535;

        private readonly InputInjector _injector;
        private readonly int _originX;
        private readonly int _originY;
        private readonly int _width;
        private readonly int _height;

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;

        public RawInputHidSender()
        {
            _injector = InputInjector.TryCreate() ?? throw new InvalidOperationException("InputInjector is unavailable. Enable HID input injection (Win10+) or install the HID injection driver.");

            _originX = GetSystemMetrics(SM_XVIRTUALSCREEN);
            _originY = GetSystemMetrics(SM_YVIRTUALSCREEN);
            _width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            _height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

            if (_width <= 0 || _height <= 0)
            {
                _width = GetSystemMetrics(0);
                _height = GetSystemMetrics(1);
            }
        }

        public void MoveTo(int x, int y)
        {
            var mouseMove = new InjectedInputMouseInfo
            {
                MouseOptions = InjectedInputMouseOptions.Move | InjectedInputMouseOptions.Absolute | InjectedInputMouseOptions.VirtualDesk,
                DeltaX = NormalizeToAbsolute(x, _originX, _width),
                DeltaY = NormalizeToAbsolute(y, _originY, _height)
            };

            _injector.InjectMouseInput(new[] { mouseMove });
        }

        public void LeftClickAt(int x, int y)
        {
            int nx = NormalizeToAbsolute(x, _originX, _width);
            int ny = NormalizeToAbsolute(y, _originY, _height);

            _injector.InjectMouseInput(new[]
            {
                new InjectedInputMouseInfo
                {
                    MouseOptions = InjectedInputMouseOptions.Move | InjectedInputMouseOptions.Absolute | InjectedInputMouseOptions.VirtualDesk,
                    DeltaX = nx,
                    DeltaY = ny
                },
                new InjectedInputMouseInfo
                {
                    MouseOptions = InjectedInputMouseOptions.LeftDown | InjectedInputMouseOptions.Absolute | InjectedInputMouseOptions.VirtualDesk,
                    DeltaX = nx,
                    DeltaY = ny
                },
                new InjectedInputMouseInfo
                {
                    MouseOptions = InjectedInputMouseOptions.LeftUp | InjectedInputMouseOptions.Absolute | InjectedInputMouseOptions.VirtualDesk,
                    DeltaX = nx,
                    DeltaY = ny
                }
            });
        }

        public void PressKey(ushort vk)
        {
            _injector.InjectKeyboardInput(new[]
            {
                new InjectedInputKeyboardInfo { VirtualKey = vk, KeyOptions = InjectedInputKeyOptions.None },
                new InjectedInputKeyboardInfo { VirtualKey = vk, KeyOptions = InjectedInputKeyOptions.KeyUp }
            });
            Thread.Sleep(12);
        }

        public void PressCombo(ushort mod, ushort key)
        {
            _injector.InjectKeyboardInput(new[]
            {
                new InjectedInputKeyboardInfo { VirtualKey = mod, KeyOptions = InjectedInputKeyOptions.None },
                new InjectedInputKeyboardInfo { VirtualKey = key, KeyOptions = InjectedInputKeyOptions.None },
                new InjectedInputKeyboardInfo { VirtualKey = key, KeyOptions = InjectedInputKeyOptions.KeyUp },
                new InjectedInputKeyboardInfo { VirtualKey = mod, KeyOptions = InjectedInputKeyOptions.KeyUp }
            });
            Thread.Sleep(12);
        }

        private static int NormalizeToAbsolute(int coordinate, int origin, int span)
        {
            if (span <= 1)
            {
                return 0;
            }

            int clamped = Math.Clamp(coordinate, origin, origin + span - 1);
            double relative = (clamped - origin) * (double)AbsoluteRange / (span - 1);
            return (int)Math.Round(relative);
        }
        
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
}