using GameBoySharp;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var tetrisRomBytes = LoadRom("Tetris.gb");
        //var asmReader = new GBAssemblyReader(tetrisRomBytes);

        var gameBoy = new GameBoy();
        //gameBoy.LoadGame(LoadRom("cpu_instrs/cpu_instrs.gb"));
        //gameBoy.LoadGame(LoadRom("cpu_instrs/individual/01-special.gb"));
        //gameBoy.LoadGame(LoadRom("cpu_instrs/individual/02-interrupts.gb"));
        gameBoy.LoadGame(LoadRom("cpu_instrs/individual/06-ld r,r.gb"));
        Console.ReadKey();
    }

    public static byte[] LoadRom(string romName)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var romsDir = Path.GetFullPath(Path.Combine(currentDir, "..\\..\\..\\..\\..", "Roms"));
        return File.ReadAllBytes(Path.Combine(romsDir, romName));
    }
}