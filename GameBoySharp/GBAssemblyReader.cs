using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public sealed class GBAssemblyReader
{
    readonly byte[] rom;
    readonly int romSizeBytes;
    readonly InstructionDatabase instructionDatabase = new();

    public GBAssemblyReader(byte[] rom)
    {
        instructionDatabase.Init();

        this.rom = rom;
        romSizeBytes = rom.Length;

        int PC = 0x150;
        HashSet<InstructionMeta> uniqGeneralOpcodes = new();
        HashSet<InstructionMeta> uniqPrefixOpcodes = new();
        while (true)
        {
            Thread.Sleep(0);

            try
            {
                //Console.WriteLine($"PC: {PC}");
                var opcode = ReadOpcode(PC);
                if (opcode is null)
                {
                    Console.WriteLine("not found opcode at address: " + PC);
                    break;
                }

                PC += opcode.bytes;

                // assert prefix opcode
                if (opcode.isPrefix)
                {
                    opcode = ReadOpcode(PC);
                    PC += opcode.bytes;
                    Console.WriteLine("found prefix opcode: 0x" + opcode.opcodeHex);
                    if (opcode is null)
                    {
                        Console.WriteLine("not found prefix opcode at address: " + PC);
                        break;
                    }
                    uniqPrefixOpcodes.Add(opcode);
                }
                else
                {
                    uniqGeneralOpcodes.Add(opcode);
                }

                // check out of rom
                if (PC >= rom.Length)
                    break;

                //Console.WriteLine($" opcode: 0x{opcode.opcodeHex}, len: {opcode.bytes}");
                //if (opcode.operands.Length > 0)
                //{
                //    var op0 = opcode.operand0;
                //    var op1 = opcode.operand1;
                //    Console.WriteLine($"  operand 0: {op0?.name}");
                //    if (op1 != null)
                //        Console.WriteLine($"  operand 1: {op1?.name}");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        Console.WriteLine("successfully");
        Console.WriteLine("total general opcodes: " + uniqGeneralOpcodes.Count);
        Console.WriteLine("total prefix opcodes: " + uniqPrefixOpcodes.Count);
    }

    byte ReadByte(int i)
    {
        return rom[i];
    }

    ushort ReadUShort(int i)
    {
        // little indian
        return (ushort)(rom[i + 1] << 8 | rom[i]);
    }

    InstructionMeta? ReadOpcode(byte opcode)
    {
        return instructionDatabase.GetInstruction(opcode);
    }

    InstructionMeta? ReadOpcode(int i)
    {
        return ReadOpcode(ReadByte(i));
    }
}
