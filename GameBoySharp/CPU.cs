using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public sealed class CPU
{
    // Registers
    public static ushort PC { get; private set; }
    public static ushort SP { get; private set; }

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

        // setup default state

        AF.value = 0x01B0;
        BC.value = 0x0013;
        DE.value = 0x00D8;
        HL.value = 0x014d;
        SP = 0xFFFE;
        PC = 0x100;
    }

    public InstructionMeta? lastFetch;

    InstructionMeta Fetch()
    {
        var opcode = mmu.ReadByte(PC++);

        var inst = instructionDB.GetInstruction(opcode);
        lastFetch = inst;

        if (inst is null)
        {
            // freeze PC
            PC--;

            throw new Exception("instruction is null");
        }

        return inst;
    }

    void ReadInstructionBytes(int address, byte[] bytes, ref string bytesHex)
    {
        var opcode = ReadByte(address);
        var instruction = instructionDB.GetInstruction(opcode);
        bytes[0] = opcode;
        bytesHex = "0x" + opcode.ToHex();

        if (instruction.bytes >= 2)
        {
            bytes[1] = ReadByte(address + 1);
            bytesHex += " 0x" + bytes[1].ToHex();
            if (instruction.bytes >= 3)
            {
                bytes[2] = ReadByte(address + 2);
                bytesHex += " 0x" + bytes[2].ToHex();
            }
        }
    }

    InstructionDecode? lastDecode;
    public void Step()
    {
        var inst = Fetch();

        byte[] instructionBytes = new byte[inst.bytes];
        string instructionBytesHex = "";
        ReadInstructionBytes(PC - 1, instructionBytes, ref instructionBytesHex);
        lastDecode = new(inst, instructionBytes);

        //Logger.Log("");
        //Logger.Log("");
        //Logger.Log("PC", (PC - 1).ToHex());
        //Logger.Log(inst);

        if (inst.isPrefix)
        {
            throw new NotSupportedException("not support prefix opcode!!");
        }

        var instName = inst.mnemonic;
        switch (instName)
        {
            case "NOP": break;
            case "ADD": ADD(inst); break;
            case "SUB": SUB(inst); break;
            case "SBC": SBC(inst); break;
            case "INC": INC(inst); break;
            case "DEC": DEC(inst); break;
            case "CP": CP(inst); break;
            case "CPL": CPL(inst); break;
            case "JP": Jump(inst); break;
            case "JR": JumpRelative(inst); break;
            case "CALL": CALL(inst); break;
            case "RET":
            case "RETI":
                RETOrRETI(inst); break;
            case "POP": POP(inst); break;
            case "PUSH": PUSH(inst); break;
            case "RST": RST(inst); break;

            case "AND": AND(inst); break;
            case "OR": OR(inst); break;
            case "XOR": XOR(inst); break;

            case "LD": LD(inst); break;
            case "LDH": LDH(inst); break;
            //case "ILLEGAL_FC": break;
            case "DI": DI(inst); break;
            case "STOP": STOP(inst); break;
            case "CCF": CCF(inst); break;

            // Prefix Extends
            //case "SET": SET(inst); break;

            default: throw new NotImplementedException();
        }
    }

    void CCF(InstructionMeta inst)
    {
        FlagC = !FlagC;
        FlagN = false;
        FlagH = false;
    }

    void STOP(InstructionMeta inst)
    {
        throw new NotImplementedException();
    }

    void POP(InstructionMeta inst)
    {
        var reg = GetRegister16Bit(inst.operand1);
        reg.value = POP();
    }

    void RST(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            case 0xC7: RST(0x00); break;
            case 0xD7: RST(0x10); break;
            case 0xE7: RST(0x20); break;
            case 0xF7: RST(0x30); break;

            case 0xCF: RST(0x08); break;
            case 0xDF: RST(0x18); break;
            case 0xEF: RST(0x28); break;
            case 0xFF: RST(0x38); break;

            default: throw new NotImplementedException();
        }
    }

    void RST(byte b)
    {
        PUSH(PC);
        PC = b;
    }

    void PUSH(InstructionMeta inst)
    {
        PUSH(GetRegister16Bit(inst.operand1).value);
    }

    void ADD(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            // ADD HL, ??
            case <= 0x39:
                DAD(GetRegister16Bit(inst.operand2).value);
                break;
            case 0x86:
                ADD(ReadByte(HL));
                break;
            case <= 0x87:
                ADD((byte)GetRegister(inst.operand2).GetValue());
                break;
            case 0xC6:
                ADD(ReadBytePC());
                break;

            default: throw new NotImplementedException();
        }
    }

    void ADD(byte b)
    {
        int result = A + b;
        SetFlagZ(result);
        FlagN = false;
        SetFlagH(A, b);
        SetFlagC(result);
        A.value = (byte)result;
    }
    void DAD(ushort w)
    {
        int result = HL + w;
        FlagN = false;
        SetFlagH(HL.value, w); //Special Flag H with word
        FlagC = result >> 16 != 0; //Special FlagC as short value involved
        HL.value = (ushort)result;
    }

    void CALL(InstructionMeta inst)
    {
        // CALL a16
        if (inst.opcode is 0xCD)
        {
            CALL(true);
            return;
        }

        CALL(GetOperandCondValue(inst.operand1));
    }


    // instruction use 3 byte
    void CALL(bool flag)
    {
        if (flag)
        {
            PUSH((ushort)(PC + 2));
            PC = ReadWordPC();
        }
        else
        {
            PC += 2;
        }
    }

    void CPL(InstructionMeta inst)
    {
        A.value = (byte)~A.value;
        FlagN = true;
        FlagH = true;
    }

    void SBC(InstructionMeta inst)
    {
        // SBC A, d8
        if (inst.opcode is 0xDE)
        {
            SBC(A.value);
            return;
        }
        // SBC (HL)
        else if (inst.opcode is 0x9E)
        {
            SBC(ReadByte(HL));
            return;
        }

        var reg = (Register8Bit)GetRegister(inst.operand1);
        reg.value = SUB(reg.value);
    }

    void SBC(byte b)
    {
        int carry = FlagC ? 1 : 0;
        int result = A - b - carry;
        SetFlagZ(result);
        FlagN = true;
        if (FlagC)
            SetFlagHSubCarry(A, b);
        else
            SetFlagHSub(A, b);
        SetFlagC(result);
        A.value = (byte)result;
    }
    void SetFlagHSubCarry(byte b1, byte b2)
    {
        int carry = FlagC ? 1 : 0;
        FlagH = (b1 & 0xF) < ((b2 & 0xF) + carry);
    }

    byte SUB(byte b)
    {
        int result = A - b;
        SetFlagZ(result);
        FlagN = true;
        SetFlagHSub(A, b);
        SetFlagC(result);
        A.value = (byte)result;
        return (byte)result;
    }
    void SUB(InstructionMeta inst)
    {
        // SUB (HL)
        if (inst.opcode is 0x96)
        {
            SUB(ReadByte(HL.value));
            return;
        }
        // SUB d8
        else if (inst.opcode is 0xD6)
        {
            SUB(ReadBytePC());
            PC++;
            return;
        }

        var register = (Register8Bit)GetRegister(inst.operand1);
        register.value = SUB(register.value);
    }

    bool GetOperandCondValue(InstructionMeta.OperandMeta operand)
    {
        switch (operand.name)
        {
            case "Z": return FlagZ;
            case "NZ": return NotFlagZ;
            case "C": return FlagC;
            case "NC": return NotFlagC;
            default: throw new NotImplementedException();
        }
    }
    void RETOrRETI(InstructionMeta inst)
    {
        if (inst.mnemonic is "RETI")
        {
            RET(true);
            IME = true;
            return;
        }
        if (inst.operand1 is not null)
            RET(GetOperandCondValue(inst.operand1));
        else
            RET(true);
    }

    void RET(bool flag)
    {
        if (flag)
            PC = POP();
    }

    void PUSH(ushort addr)
    {
        SP -= 2;
        WriteWord(SP, addr);
    }
    ushort POP()
    {
        ushort addr = ReadWord(SP);
        SP += 2;
        return addr;
    }

    void OR(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            case <= 0xB7:
                var register = GetRegister(inst.operand1);
                var registerValue = register.GetValue();
                if (inst.opcode is 0xB6)
                    OR((byte)registerValue);
                else
                    OR(ReadByte(registerValue));
                break;

            // OR d8
            case 0xF6:
                OR(ReadByte(PC));
                PC++;
                break;

            default: throw new NotImplementedException();
        }

    }

    void OR(byte b)
    {
        byte result = (byte)(A | b);
        SetFlagZ(result);
        FlagN = false;
        FlagH = false;
        FlagC = false;
        A.value = result;
    }

    void SET(InstructionMeta inst)
    {
        var operand1 = inst.operand1;
        var operand2 = inst.operand2;
        var srcValue = ReadByte(PC);

        var register = GetRegister(operand2);
        if (operand2.immediate)
            register.SetValue(register.GetValue() | srcValue);
        else
            register.SetValue(ReadByte(register.GetValue()) | srcValue);
    }

    void AND(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            case <= 0xA7:
                var register = GetRegister(inst.operand1);
                var regValue = register.GetValue();
                // AND (HL)
                if (inst.opcode is 0xA6)
                {
                    AND(ReadByte(regValue));
                    return;
                }

                AND((byte)regValue);
                break;
            case 0xE6:
                AND(ReadByte(PC));
                break;

            default: throw new NotImplementedException();
        }

        PC += (ushort)(inst.bytes - 1);
    }

    void AND(byte b)
    {
        byte result = (byte)(A & b);
        SetFlagZ(result);
        FlagN = false;
        FlagH = true;
        FlagC = false;
        A.value = result;
    }

    void CP(InstructionMeta inst)
    {
        var operand = inst.operand1;
        switch (inst.opcode)
        {
            case <= 0xBF:
                var register = GetRegister(operand);
                // CP d8
                if (inst.opcode is 0xBE)
                    CP(ReadByte(register.GetValue()));
                else
                    CP((byte)register.GetValue());
                break;
            case 0xFE:
                CP(ReadByte(PC));
                break;

            default: throw new NotImplementedException();
        }

        PC += (ushort)(inst.bytes - 1);
    }
    void CP(byte b)
    {
        int result = A - b;
        SetFlagZ(result);
        FlagN = true;
        SetFlagHSub(A, b);
        SetFlagC(result);
    }

    void JumpRelative(InstructionMeta inst)
    {
        switch (inst.opcode)
        {
            case 0x18: JumpRelative(true); break;
            case 0x20: JumpRelative(NotFlagZ); break;
            case 0x28: JumpRelative(FlagZ); break;
            case 0x30: JumpRelative(NotFlagC); break;
            case 0x38: JumpRelative(FlagC); break;
            default: throw new NotImplementedException();
        }
    }
    void JumpRelative(bool jump)
    {
        if (jump)
        {
            var offset = (sbyte)lastDecode.raw[1];
            PC = (ushort)(PC + offset + 1);
        }

        // move next instruction
        else
        {
            PC++;
        }
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

    void INC(InstructionMeta inst)
    {
        var opcode = inst.opcode;
        // 0x33 INC SP
        if (opcode is 0x33)
        {
            SP++;
            return;
        }
        // 0x34 INC (HL)
        else if (opcode is 0x34)
        {
            var newValue = INC(ReadByte(HL.value));
            WriteWord(HL.value, newValue);
            return;
        }

        // INC Reg?
        var register = GetRegister(inst.operand1);
        if (inst.anyFlagAffect)
        {
            // support only 8bit
            byte newValue = INC((byte)register.GetValue());
            if (register is Register16Bit reg16bit)
                throw new NotSupportedException();
            else if (register is Register8Bit)
                register.SetValue(newValue);
        }
        else
        {
            var newValue = register.GetValue() + 1;
            if (register is Register16Bit regis16Bit)
                regis16Bit.SetValue(newValue);
            else
                throw new NotImplementedException();
        }
    }

    byte INC(byte b)
    {
        int result = b + 1;
        SetFlagZ(result);
        FlagN = false;
        SetFlagH(b, 1);
        return (byte)result;
    }

    void DEC(InstructionMeta inst)
    {
        var operand = inst.operand1;
        var register = GetRegister(operand);

        // 0x3B DEC SP
        if (inst.opcode is 0x3B)
        {
            SP--;
            return;
        }
        // 0x35 DEC (HL)
        else if (inst.opcode == 0x35)
        {
            var newValue = DEC(ReadByte(HL.value));
            WriteByte(HL.value, newValue);
            PC += (ushort)(inst.bytes - 1);
            return;
        }

        // Decrement Register 8bit | 16bit
        if (inst.anyFlagAffect)
        {
            byte prevValue = (byte)register.GetValue();
            //byte newValue = DEC(prevValue);
            //register.SetValue(newValue);
            register.SetValue(DEC(prevValue));
        }
        // 0x?8
        else
            register.SetValue(register.GetValue() - 1);

        PC += (ushort)(inst.bytes - 1);
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
        FlagZ = (byte)result == 0;
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
    Register16Bit? GetRegister16Bit(InstructionMeta.OperandMeta operand)
        => GetRegisterByName(operand.name) as Register16Bit;

    // auto value 8bit, 16bit
    void LoadValueToRegister(IRegisterBase register, ushort value)
    {
        if (register is Register8Bit reg8Bit)
            reg8Bit.SetValue((byte)value);

        else if (register is Register16Bit reg16Bit)
            reg16Bit.SetValue(value);
    }

    ushort ReadOperandValue(InstructionMeta inst, InstructionMeta.OperandMeta operand)
    {
        if (operand.types.HasFlag(OperandType.Register))
        {
            var register = GetRegisterByName(operand.name);
            var regisValue = register.GetValue();
            if (operand.immediate)
                return regisValue;

            if (register is Register16Bit reg16Bit)
                return ReadWord(regisValue);

            throw new NotImplementedException();
        }

        if (operand.immediate)
        {
            if (operand.name is "n16")
                return ReadWord(PC - 1 + operand.byteOffset);
            else if (operand.name is "n8")
                return ReadByte(PC - 1 + operand.byteOffset);
            else if (operand.name is "SP")
                return SP;
            else if (operand.name is "a16")
                return ReadWord(PC - 1 + operand.byteOffset);
        }

        throw new NotImplementedException();
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
                        ushort dstAddress = dstRegister.GetValue();
                        if (inst.opcode is 0x12)
                        {
                            Console.WriteLine();
                        }

                        if (srcOperand.is8Bit)
                            WriteByte(dstAddress, (byte)srcValue);
                        else
                            WriteWord(dstAddress, srcValue);
                    }
                    else
                    {
                        ushort dstAddress = ReadOperandValue(inst, dstOperand);
                        if (dstOperand.is8Bit)
                            WriteByte(dstAddress, (byte)srcValue);
                        else
                            WriteWord(dstAddress, srcValue);
                    }
                }
                // load value to register
                else
                {
                    LoadValueToRegister(dstRegister, srcValue);
                }

                // done & post
                if (dstOperand.increment || dstOperand.decrement)
                    OperandPostIncOrDec(dstOperand);
                else if (srcOperand.increment || srcOperand.decrement)
                    OperandPostIncOrDec(srcOperand);

                break;
            case 0xF8:
                HL.value = DADe8(SP);
                PC += (ushort)(inst.bytes - 1);
                break;
            case 0xFA:
                A.value = ReadByte(ReadWordPC());
                break;
            case 0xEA:
                WriteByte(ReadWordPC(), A.value);
                break;
            case 0xF9:
                SP = HL;
                break;
            default:
                throw new NotImplementedException();
        }

        PC += (ushort)(inst.bytes - 1);
    }

    void OperandPostIncOrDec(InstructionMeta.OperandMeta operand)
    {
        if (!operand.increment && !operand.decrement)
            return;

        var reg = GetRegister(operand);
        if (operand.increment)
            reg.Increment();
        else
            reg.Decrement();

        //Logger.Log("OperandPostIncOrDec", operand.name, "flag:", operand.types);
    }

    void SetFlagH(byte a, byte b)
    {
        FlagH = ((a & 0xF) + (b & 0xF)) > 0xF;
    }
    void SetFlagH(ushort w1, ushort w2)
    {
        FlagH = ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF;
    }
    void SetFlagC(int c)
    {
        FlagC = (c >> 8) != 0;
    }
    private ushort DADe8(ushort w)
    {
        byte b = ReadByte(PC);
        FlagZ = false;
        FlagN = false;
        SetFlagH((byte)w, b);
        SetFlagC((byte)w + b);
        return (ushort)(w + (sbyte)b);
    }

    #region Memory Mapping
    byte ReadByte(ushort addr) => mmu.ReadByte(addr);
    byte ReadByte(int addr) => ReadByte((ushort)addr);
    byte ReadBytePC() => mmu.ReadByte(PC);
    byte ReadByte(Register16Bit r) => mmu.ReadByte(r.value);
    ushort ReadWord(ushort addr) => mmu.ReadWord(addr);
    ushort ReadWord(int addr) => mmu.ReadWord((ushort)addr);
    ushort ReadWordPC() => mmu.ReadWord(PC);

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
            case 0xC2: jump = NotFlagZ; break;
            case 0xC3: jump = true; break;
            case 0xCA: jump = FlagZ; break;
            case 0xD2: jump = NotFlagC; break;
            case 0xDA: jump = FlagC; break;

            default: throw new NotImplementedException();
        }


        if (jump)
        {
            PC = ReadWordPC();
            Console.WriteLine("jumped to : " + PC.ToHex());
        }
        else
        {
            PC += 2;
            Console.WriteLine("skip jump");
        }
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
