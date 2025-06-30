using Newtonsoft.Json;

namespace GameBoySharp;

[Flags]
public enum OperandType : int
{
    None = 0,
    Address = 1, // a16
    Register = 2, // H or HL
    RegisterPostIncrement = 4, // HL+
    RegisterPostDecrement = 8, // HL-
    Imm8 = 16, // 0xF9
    Imm16 = 32, // 0x0200
    Signed8 = 64, // e8
    Indirect = 128, // (0x0002) or (HL)
}

public sealed class InstructionMeta
{
    public class OperandMeta
    {
        // Serializer Fields
        [JsonProperty]
        public string name { get; private set; }
        [JsonProperty]
        public int bytes { get; private set; }
        [JsonProperty]
        public bool immediate { get; private set; }
        [JsonProperty]
        public bool increment { get; private set; }
        [JsonProperty]
        public bool decrement { get; private set; }

        // Helper
        public OperandType types { get; private set; } = OperandType.None;
        public bool is8Bit { get; private set; }
        public bool is16Bit { get; private set; }
        public bool isRegister { get; private set; }
        // for read operand value at address with PC + byteOffset
        public int byteOffset { get; private set; }
        public int index { get; private set; }


        public void Init(int operandIndex, int byteOffset)
        {
            this.index = operandIndex;
            this.byteOffset = byteOffset;

            is8Bit = bytes == 1;
            is16Bit = !is8Bit;

            if (RegisterUtils.IsRegisterByName(name))
            {
                isRegister = true;
                types = OperandType.Register;
                if (increment)
                    types |= OperandType.RegisterPostIncrement;
                else if (decrement)
                    types |= OperandType.RegisterPostDecrement;
            }
            // a8, a16, address 8-bit, 16-bit
            else if (name.StartsWith("a"))
            {
                types = OperandType.Address;
            }
            // n8, n16 | immediate 8-bit, 16-bit
            else if (name.StartsWith("n"))
            {
                types = is8Bit ? OperandType.Imm8 : OperandType.Imm16;
            }
            // e8 | signed 8bit all use immediate
            else if (name == "e8")
            {
                types = OperandType.Signed8;
            }

            if (!immediate)
                types |= OperandType.Indirect;
        }
    }

    // serialize fields
    [JsonProperty]
    public string mnemonic { get; private set; }
    [JsonProperty]
    public ushort bytes { get; private set; }
    [JsonProperty]
    public int[]? cycles { get; private set; }
    [JsonProperty]
    public bool immediate { get; private set; }
    [JsonProperty]
    public Dictionary<string, string> flags { get; private set; }
    [JsonProperty]
    public OperandMeta[] operands { get; private set; } = [];


    // helper
    // need to init this
    public byte opcode { get; private set; }
    public string? opcodeHex { get; private set; }

    public string flagZ { get; private set; }
    public string flagN { get; private set; }
    public string flagH { get; private set; }
    public string flagC { get; private set; }
    public bool anyFlagAffect { get; private set; }
    public int cycle { get; private set; }
    public OperandMeta? operand1 { get; private set; }
    public OperandMeta? operand2 { get; private set; }
    public OperandMeta? operand3 { get; private set; }
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
        cycle = cycles != null ? cycles[0] : 0;
        flagZ = flags["Z"];
        flagN = flags["N"];
        flagH = flags["H"];
        flagC = flags["C"];

        foreach ((var flagName, var flagAffect) in flags)
        {
            if (flagAffect != "-")
            {
                anyFlagAffect = true;
            }
        }

        if (operands.Length > 0)
        {
            operand1 = operands[0];
            operand1.Init(0, operand1.bytes);

            operand2 = operands.Length >= 2 ? operands[1] : null;
            if (operand2 != null)
                operand2.Init(1, operand1.bytes + operand2.bytes);

            operand3 = operands.Length >= 3 ? operands[2] : null;
            if (operand3 != null)
                operand3?.Init(2, operand1.bytes + operand2.bytes + operand3.bytes);
        }

        isPrefix = mnemonic == "PREFIX";
    }

    public override string? ToString()
    {
        return $"{mnemonic} 0x{opcodeHex}";
    }
}
