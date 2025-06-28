namespace GameBoySharp;

public sealed class InstructionDBInfo
{
    public class Operand
    {
        // Serializer Fields
        public required string name { get; set; }
        public int bytes { get; set; }
        public bool immediate { get; set; }
    }

    // serialize fields
    public required string mnemonic { get; set; }
    public ushort bytes { get; set; }
    public required int[]? cycles { get; set; }
    public bool immediate { get; set; }
    public required Dictionary<string, string> flags { get; set; }
    public Operand[] operands { get; set; } = [];


    // helper
    // need to init this
    public byte opcode { get; private set; }
    public string? opcodeHex { get; private set; }

    public string flagZ { get; private set; }
    public string flagN { get; private set; }
    public string flagH { get; private set; }
    public string flagC { get; private set; }
    public int cycle { get; private set; }
    public Operand? operand0 { get; private set; }
    public Operand? operand1 { get; private set; }
    public bool isPrefix { get; private set; }

    public string GetFlag(string name)
    {
        switch (name.ToUpper())
        {
            case "Z": return flagZ;
            case "N": return flagN;
            case "H": return flagH;
            case "C": return flagC;
            default: return "-";
        }
    }

    public bool HasFlag(string name)
    {
        return GetFlag(name) != "-";
    }

    public void Init(string hex)
    {
        // assert
        hex = hex.Replace("0x", "").ToUpper();

        this.opcodeHex = hex;
        this.opcode = Convert.ToByte(hex, 16);
        cycle = cycles[0];
        flagZ = flags["Z"];
        flagN = flags["N"];
        flagH = flags["H"];
        flagC = flags["C"];

        if (operands.Length > 0)
        {
            operand0 = operands[0];
            operand1 = operands.Length == 2 ? operands[1] : null;
        }

        isPrefix = mnemonic == "PREFIX";
    }

    public override string? ToString()
    {
        return $"{mnemonic} 0x{opcodeHex}";
    }
}
