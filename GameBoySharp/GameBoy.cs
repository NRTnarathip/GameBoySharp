namespace GameBoySharp;

public sealed class GameBoy
{
    public readonly MMU mmu = new();
    public readonly CPU cpu;

    public GameBoy()
    {
        cpu = new(mmu);
    }


    GamePak? gamePak;
    public void LoadGame(byte[] rom)
    {
        // setup
        gamePak = new(rom);
        cpu.InitGamePak(gamePak);

        // run now!!
        RunGame();
    }

    void RunGame()
    {
        while (true)
        {
            Thread.Sleep(0);

            cpu.Step();
        }
    }

    public void LoadGame(string path) => LoadGame(File.ReadAllBytes(path));
}
