using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PolyWedger
{
    public class RGEInterfacer
    {
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk, wScan;
            public uint dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        const uint INPUT_MOUSE = 0;
        const uint INPUT_KEYBOARD = 1;
        const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        const uint MOUSEEVENTF_LEFTUP = 0x04;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")] static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll")] static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")] static extern bool OpenClipboard(IntPtr hwnd);
        [DllImport("user32.dll")] static extern bool CloseClipboard();
        [DllImport("user32.dll")] static extern bool EmptyClipboard();
        [DllImport("user32.dll")] static extern bool SetClipboardData(uint uFormat, IntPtr data);
        const uint CF_UNICODETEXT = 13;

        void PressKey(ushort vk)
        {
            INPUT[] inp = new INPUT[2];

            inp[0].type = INPUT_KEYBOARD;
            inp[0].U.ki.wVk = vk;

            inp[1].type = INPUT_KEYBOARD;
            inp[1].U.ki.wVk = vk;
            inp[1].U.ki.dwFlags = KEYEVENTF_KEYUP;

            SendInput(2, inp, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(12);
        }

        void PressCombo(ushort mod, ushort key)
        {
            PressKey(mod);
            PressKey(key);

            INPUT up = new INPUT { type = INPUT_KEYBOARD };
            up.U.ki.wVk = mod;
            up.U.ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput(1, new INPUT[] { up }, Marshal.SizeOf(typeof(INPUT)));
        }

        void RobloxClick(int x, int y, int cycles = 2)
        {
            SetCursorPos(x, y);
            Thread.Sleep(5);

            int[,] offsets = new int[,]
            {
                { 0,0 }, { 5,0 }, { 5,5 }, { 0,5 }, { -5,5 },
                { -5,0 }, { -5,-5 }, { 0,-5 }, { 5,-5 }
            };

            for (int c = 0; c < cycles; c++)
            {
                for (int i = 0; i < offsets.GetLength(0); i++)
                {
                    int ox = x + offsets[i, 0];
                    int oy = y + offsets[i, 1];

                    SetCursorPos(ox, oy);
                    Thread.Sleep(4);

                    INPUT[] click = new INPUT[2];

                    click[0].type = INPUT_MOUSE;
                    click[0].U.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;

                    click[1].type = INPUT_MOUSE;
                    click[1].U.mi.dwFlags = MOUSEEVENTF_LEFTUP;

                    SendInput(2, click, Marshal.SizeOf(typeof(INPUT)));
                }
            }
        }

        void CopyUID()
        {
            PressCombo(0x11, 0x43);
        }

        void PasteUID()
        {
            PressCombo(0x11, 0x56);
        }

        public void RunTriangleWorkflow()
        {
            int spawnX = 100, spawnY = 200;
            int centerX = 960, centerY = 540;
            int uidX = 300, uidY = 300;
            int cmdX = 200, cmdY = 1000;

            RobloxClick(spawnX, spawnY);

            Thread.Sleep(200);

            RobloxClick(centerX, centerY);

            Thread.Sleep(200);

            RobloxClick(uidX, uidY);

            CopyUID();

            Thread.Sleep(150);

            RobloxClick(cmdX, cmdY);

            Thread.Sleep(150);

            PasteUID();
            PressKey(0x0D);
        }
    }
}
