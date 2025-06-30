using System.Diagnostics;

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
        var fpsTimer = Stopwatch.StartNew();
        var gameTimer = Stopwatch.StartNew();
        while (true)
        {
            Thread.Sleep(0);

            cpu.Step();

            if (fpsTimer.Elapsed.TotalSeconds >= 1)
            {
                fpsTimer.Restart();

                Console.WriteLine("run timer: " + gameTimer.Elapsed.TotalSeconds);
                Console.WriteLine("  last instruction: " + cpu.lastFetch);
            }
        }
    }

    public void LoadGame(string path) => LoadGame(File.ReadAllBytes(path));
}
