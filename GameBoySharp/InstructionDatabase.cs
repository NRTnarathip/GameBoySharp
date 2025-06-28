using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp
{
    public sealed class InstructionDatabase
    {
        public static readonly InstructionDatabase Singleton = new();

        readonly Dictionary<byte, InstructionDBInfo> normalInstructions = new();
        readonly Dictionary<byte, InstructionDBInfo> cbprefixedInstructions = new();

        public InstructionDatabase()
        {
            if (Singleton is not null)
                throw new Exception("already instance");

            Init();
        }

        public void Init()
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

        public InstructionDBInfo? GetInstruction(byte opcode)
        {
            InstructionDBInfo? inst;
            if (opcode == 0xCB)
            {
                cbprefixedInstructions.TryGetValue(opcode, out inst);
                return inst;
            }

            normalInstructions.TryGetValue(opcode, out inst);
            return inst;
        }
    }
}
