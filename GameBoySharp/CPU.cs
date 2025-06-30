using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public sealed class CPU
{
    // Registers
    public ushort PC;
    public ushort SP;

    public readonly Register8Bit A = new("A");
    public readonly Register8Bit B = new("B");
    public readonly Register8Bit C = new("C");
    public readonly Register8Bit D = new("D");
    public readonly Register8Bit E = new("E");
    public readonly RegisterFlag F = new();
    public readonly Register8Bit H = new("H");
    public readonly Register8Bit L = new("L");
    public readonly Register16Bit HL, AF, BC, DE;

    public bool FlagZ { get => F.Z; set => F.Z = value; }
    public bool NotFlagZ => !F.Z;
    public bool FlagN { get => F.N; set => F.N = value; }
    public bool FlagH { get => F.H; set => F.H = value; }
    public bool FlagC { get => F.C; set => F.C = value; }
    public bool NotFlagC => !F.C;

    InstructionDatabase instructionDB;

    readonly MMU mmu;

    public CPU(MMU mmu)
    {
        this.mmu = mmu;

        AF = new("AF", A, F);
        BC = new("BC", B, C);
        DE = new("DE", D, E);
        HL = new("HL", H, L);

        instructionDB = InstructionDatabase.Singleton;
    }

    public void Step()
    {
        var opcode = gamePak.mbc.ReadROM(PC);
        PC++;

        var inst = instructionDB.GetInstruction(opcode);
        if (inst is null)
        {
            throw new Exception("opcode is null");
        }

        Console.WriteLine("fetched inst: " + inst);


        if (inst.isPrefix)
        {
            throw new Exception("not support prefix opcode!!");
        }

        var instName = inst.mnemonic;
        switch (instName)
        {
            case "NOP": break;
            case "JP": Jump(inst); break;
            case "JR": JumpOffset(inst); break;
            case "XOR": XOR(inst); break;
            case "LD": LD(inst); break;
            case "LDH": LDH(inst); break;
            case "DEC": DEC(inst); break;
            case "ILLEGAL_FC": break;
            case "DI": DI(inst); break;
            default: throw new NotImplementedException();
        }
    }

    void JumpOffset(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            case 0x18: JumpOffset(true); break;
            case 0x20: JumpOffset(NotFlagZ); break;
            case 0x28: JumpOffset(FlagZ); break;
            case 0x30: JumpOffset(NotFlagC); break;
            case 0x38: JumpOffset(FlagC); break;
            default: throw new NotImplementedException();
        }
    }
    void JumpOffset(bool jump)
    {
        if (jump)
        {
            sbyte sb = (sbyte)ReadByte(PC);
            PC = (ushort)(PC + sb);
        }
        else
        {

        }

        PC += 1;
    }

    void LDH(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            case 0xE0:
                WriteByte(0xFF00 + ReadByte(PC), A.value);
                PC++;
                break;
            case 0xF0:
                A.value = ReadByte(0xFF00 + ReadByte(PC));
                PC++;
                break;
        }
    }

    public bool IME { get; private set; }
    void DI(InstructionMeta inst)
    {
        IME = false;
    }

    void DEC(InstructionMeta inst)
    {
        var operand = inst.operand1;
        // dec at register | register indirect

        // 0x3B DEC SP
        if (inst.opcode is 0x3B)
        {
            SP--;
            return;
        }

        var register = GetRegister(operand);
        if (operand.immediate)
        {
            if (inst.anyFlagAffect)
            {
                register.SetValue(DEC((byte)register.GetValue()));
            }
            // 0x?8
            else
            {
                register.SetValue(register.GetValue() - 1);
            }
        }
        // DEC (HL)
        else if (inst.opcode == 0x35)
        {
            WriteByte(register.GetValue(), DEC(ReadByte(register.GetValue())));
        }
        else
        {
            throw new NotImplementedException();
        }

        // update flag
        if (inst.anyFlagAffect)
        {

        }
    }

    byte DEC(byte b)
    {
        int result = b - 1;
        SetFlagZ(result);
        FlagN = true;
        SetFlagHSub(b, 1);
        return (byte)result;
    }

    void SetFlagZ(int result)
    {
        FlagZ = result == 0;
    }

    void SetFlagHSub(byte b1, byte b2)
    {
        FlagH = (b1 & 0xF) < (b2 & 0xF);
    }

    IRegisterBase? GetRegisterByName(string name)
    {
        switch (name)
        {
            case "A": return A;
            case "B": return B;
            case "C": return C;
            case "D": return D;
            case "E": return E;
            case "F": return F;
            case "H": return H;
            case "L": return L;
            case "AF": return AF;
            case "BC": return BC;
            case "DE": return DE;
            case "HL": return HL;
            default: return null;
        }
    }
    IRegisterBase? GetRegister(InstructionMeta.OperandMeta operand)
        => GetRegisterByName(operand.name);

    // support srcValue 8bit, 16bit
    void LoadValueToAddress(ushort dstAddress, object srcValue)
    {
        if (srcValue is byte val8)
            WriteByte(dstAddress, val8);
        else if (srcValue is ushort val16)
            WriteWord(dstAddress, val16);
        else
            throw new NotSupportedException();
    }

    // auto value 8bit, 16bit
    void LoadValueToRegister(IRegisterBase register, ushort value)
    {
        register.SetValue(value);
    }

    ushort ReadOperandValue(InstructionMeta inst, InstructionMeta.OperandMeta operand)
    {
        if (operand.types.HasFlag(OperandType.Register))
        {
            var register = GetRegisterByName(operand.name);
            var regisValue = (ushort)register.GetValue();
            if (operand.immediate)
                return regisValue;
            return ReadWord(regisValue);
        }

        if (operand.is8Bit)
            return ReadByte(operand.byteOffset);
        return ReadWord(operand.byteOffset);
    }

    void LD(InstructionMeta inst)
    {
        // simple case
        switch (inst.opcode)
        {
            case <= 0x7F:
                var dstOperand = inst.operand1;
                var srcOperand = inst.operand2;
                ushort srcValue = ReadOperandValue(inst, srcOperand);

                var dstRegister = GetRegisterByName(dstOperand.name);
                // load value to address
                if (dstOperand.immediate is false)
                {
                    if (dstRegister != null)
                    {
                        ushort dstAddress = (ushort)dstRegister.GetValue();
                        LoadValueToAddress(dstAddress, srcValue);
                    }
                    else
                    {
                        var dstValue = ReadOperandValue(inst, dstOperand);
                        LoadValueToAddress(dstValue, srcValue);
                    }
                }
                // load value to register
                else
                {
                    LoadValueToRegister(dstRegister, srcValue);
                }

                PC += (ushort)(inst.bytes - 1);
                break;
            case 0xF8:
                HL.value = DADe8(SP); break;
            case 0xF9:
                SP = HL; break;
            default:
                throw new NotImplementedException();
        }
    }
    void SetFlagH(byte a, byte b)
    {
        FlagH = ((a & 0xF) + (b & 0xF)) > 0xF;
    }
    void SetFlagC(int c)
    {
        FlagC = (c >> 8) != 0;
    }
    private ushort DADe8(ushort w)
    {
        byte b = ReadByte(PC++);
        FlagZ = false;
        FlagN = false;
        SetFlagH((byte)w, b);
        SetFlagC((byte)w + b);
        return (ushort)(w + (sbyte)b);
    }

    #region Memory Mapping
    byte ReadByte(ushort addr) => mmu.ReadByte(addr);
    byte ReadByte(int addr) => ReadByte((ushort)addr);
    ushort ReadWord(ushort addr) => mmu.ReadWord(addr);
    ushort ReadWord(int addr) => mmu.ReadWord((ushort)addr);

    void WriteByte(ushort addr, byte b) => mmu.WriteByte(addr, b);
    void WriteByte(int addr, byte b) => mmu.WriteByte(addr, b);

    void WriteWord(ushort addr, ushort w) => mmu.WriteWord(addr, w);
    void WriteWord(int addr, ushort w) => mmu.WriteWord(addr, w);

    #endregion

    void Jump(InstructionMeta inst)
    {
        bool jump = false;
        switch (inst.opcode)
        {
            case 0xC2:
                jump = !FlagZ;
                break;
            case 0xC3:
                jump = true;
                break;
            case 0xCA:
                jump = FlagZ;
                break;
            case 0xDA:
                jump = FlagC;
                break;
            case 0xD2:
                jump = !FlagC;
                break;

            default: throw new NotImplementedException();
        }

        if (jump)
        {
            PC = ReadWord(PC);
        }
        else
        {
            PC += 2;
        }

        Console.WriteLine("jumped to: 0x" + PC.ToHex());
    }

    void XOR(InstructionMeta inst)
    {
        byte val = 0;

        switch (inst.opcode)
        {
            case 0xA8: val = B; break;
            case 0xA9: val = C; break;
            case 0xAA: val = D; break;
            case 0xAB: val = E; break;
            case 0xAC: val = H; break;
            case 0xAD: val = L; break;
            case 0xAE: val = ReadByte(HL); break;
            case 0xAF: val = A; break;
        }

        byte result = (byte)(A.value ^ val);

        A.value = result;
        FlagZ = result == 1;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    GamePak? gamePak;
    internal void InitGamePak(GamePak gamePak)
    {
        this.gamePak = gamePak;
        mmu.Init(gamePak);
    }
}
