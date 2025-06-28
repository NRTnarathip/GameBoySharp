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
    public readonly Register16BitHiLo HL, AF, BC, DE;

    public bool FlagZ { get => F.Z; set => F.Z = value; }
    public bool FlagN { get => F.N; set => F.N = value; }
    public bool FlagH { get => F.H; set => F.H = value; }
    public bool FlagC { get => F.C; set => F.C = value; }

    InstructionDatabase instructionDB;

    public CPU()
    {
        AF = new("AF", A, F);
        BC = new("BC", B, C);
        DE = new("DE", D, E);
        HL = new("HL", H, L);

        instructionDB = InstructionDatabase.Singleton;
    }

    public void Step()
    {
        var opcode = gamePak.mbc.Read(PC);
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
            case "XOR": XOR(inst); break;
            case "LD": LD(inst); break;
            default:
                Console.WriteLine("not support mnemonic: " + instName);
                break;
        }
    }

     void LD(InstructionDBInfo inst)
    {
    }

    byte ReadByte(ushort addr)
    {
        return gamePak.mbc.Read(addr);
    }

    void Jump(InstructionDBInfo inst)
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
            PC = gamePak.mbc.Read16(PC);
        }
        else
        {
            PC += 2;
        }

        Console.WriteLine("jumped to: 0x" + PC.ToHex());
    }

    void XOR(InstructionDBInfo inst)
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

    }
}
