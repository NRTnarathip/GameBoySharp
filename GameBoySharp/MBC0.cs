using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp
{
    // Rom Only!!
    public sealed class MBC0 : MBCBase
    {
        //Rom Size 32KiB
        public const int RomSize = 0x8000;
        public readonly byte[] ROM = new byte[RomSize];

        public MBC0(byte[] bytes) : base(bytes)
        {
        }

        public override byte ReadExternalRAM(ushort addr)
        {
            return 0xFF;
        }

        public override byte ReadROM(ushort addr)
        {
            return ROM[addr];
        }

        public override void WriteExternalRAM(ushort addr, byte val)
        {
            // ignore
        }

        public override void WriteROM(ushort address, byte val)
        {
            // ignore
        }
    }
}
