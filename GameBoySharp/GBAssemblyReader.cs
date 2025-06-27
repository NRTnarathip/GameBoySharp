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
    public GBAssemblyReader(byte[] rom)
    {
        InitInstructionDatabase();

        this.rom = rom;
        romSizeBytes = rom.Length;

        int PC = 0x150;
        HashSet<InstructionDBInfo> uniqGeneralOpcodes = new();
        HashSet<InstructionDBInfo> uniqPrefixOpcodes = new();
        while (true)
        {
            Thread.Sleep(0);

            try
            {
                //Console.WriteLine($"PC: {PC}");
                var opcode = ReadOpcode(PC);
                PC += opcode.bytes;
                if (opcode is null)
                {
                    Console.WriteLine("not found opcode at address: " + PC);
                    break;
                }

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


                // check out of memory
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

    //string ByteToHex(this byte b) => $"0x{b:X}";

    InstructionDBInfo? ReadOpcode(byte opcode)
    {
        normalInstructions.TryGetValue(opcode, out var inst);
        return inst;
    }

    InstructionDBInfo? ReadOpcode(int i)
    {
        return ReadOpcode(ReadByte(i));
    }

    readonly Dictionary<byte, InstructionDBInfo> normalInstructions = new();
    readonly Dictionary<byte, InstructionDBInfo> cbprefixedInstructions = new();
    void InitInstructionDatabase()
    {
        var json = JObject.Parse(File.ReadAllText("instructions.json"));
        var unprefixItems = json["unprefixed"].ToObject<Dictionary<string, InstructionDBInfo>>();
        var cbprefixItems = json["cbprefixed"].ToObject<Dictionary<string, InstructionDBInfo>>();
        LoadInstructionToDictionary(unprefixItems, normalInstructions);
        LoadInstructionToDictionary(cbprefixItems, cbprefixedInstructions);
    }
    void LoadInstructionToDictionary(
        Dictionary<string, InstructionDBInfo> srcItems,
        Dictionary<byte, InstructionDBInfo> destDictionary)
    {
        foreach ((var opcodeHex, var inst) in srcItems)
        {
            inst.Init(opcodeHex);
            destDictionary[inst.opcode] = inst;
        }
    }

}
