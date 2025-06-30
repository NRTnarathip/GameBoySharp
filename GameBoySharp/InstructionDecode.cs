using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp
{
    public sealed class InstructionDecode
    {
        public readonly InstructionMeta meta;
        public readonly byte[] raw;
        public readonly string bytesHex = "";

        public InstructionDecode(InstructionMeta meta, byte[] instructionBytes)
        {
            this.meta = meta;
            this.raw = instructionBytes;
            foreach (var b in raw)
            {
                bytesHex += $" 0x{b.ToHex()}";
            }
        }
    }
}
