namespace RGEInterfacerTest;

internal abstract class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("RGEInterfacer HID Test Runner");
        Console.WriteLine("You have 3 seconds to focus Roblox...");
        Thread.Sleep(3000);

        RawInputHidSender hid = new RawInputHidSender();
        RGEInterfacer rge = new RGEInterfacer(hid);

        Console.WriteLine("Running triangle workflow via HID injection...");
        rge.RunTriangleWorkflow();

        Console.WriteLine("Done! Press any key to exit.");
        Console.ReadKey();
    }
}

