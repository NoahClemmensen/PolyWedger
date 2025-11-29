namespace RGEInterfacerTest;

public class RGEInterfacer(RawInputHidSender hid)
{
    private readonly Random _rng = new Random();

    void RobloxClick(int x, int y, int jitterMoves = 3)
    {
        int moves = Math.Max(0, jitterMoves - 1);

        for (int i = 0; i < moves; i++)
        {
            int jitterX = x + _rng.Next(-2, 3);
            int jitterY = y + _rng.Next(-2, 3);

            hid.MoveTo(jitterX, jitterY);
            Thread.Sleep(10);
        }

        hid.MoveTo(x, y);
        Thread.Sleep(100);
        hid.LeftClickAt(x, y);
        Thread.Sleep(100);
    }

    void CopyUIDFromField(int x, int y)
    {
        RobloxClick(x, y, 1);
        RobloxClick(x, y, 1);
        Thread.Sleep(1000);
        hid.PressCombo(0x11, 0x43);
        Thread.Sleep(1000);
    }

    void PasteUID()
    {
        hid.PressCombo(0x11, 0x56);
    }

    public void RunTriangleWorkflow()
    {
        int spawnX = 840, spawnY = 65;
        int centerX = 996, centerY = 475;
        int cmdX = 914, cmdY = 1033;
        int uidX = 1827, uidY = 227;

        RobloxClick(spawnX, spawnY);

        Thread.Sleep(200);

        RobloxClick(centerX, centerY);

        Thread.Sleep(200);

        RobloxClick(uidX, uidY);
        CopyUIDFromField(uidX, uidY);

        Thread.Sleep(300);

        RobloxClick(cmdX, cmdY);

        Thread.Sleep(150);

        PasteUID();
        hid.PressKey(0x0D);
    }
}