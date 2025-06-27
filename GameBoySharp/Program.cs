using GameBoySharp;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        var tetrisRomBytes = LoadRom("Tetris.gb");
        var asmReader = new GBAssemblyReader(tetrisRomBytes);

        //var gameBoy = new GameBoy();
        //gameBoy.LoadRom(tetrisRomBytes);
        Console.ReadKey();
    }

    public static byte[] LoadRom(string romName)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var romsDir = Path.GetFullPath(Path.Combine(currentDir, "..\\..\\..\\..\\..", "Roms"));
        return File.ReadAllBytes(Path.Combine(romsDir, romName));
    }
}