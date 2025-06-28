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

        public MBC0(byte[] rom)
        {
            this.ROM = rom;
        }

        public override byte Read(int address)
        {
            return ROM[address];
        }

        public override ushort Read16(ushort addr)
        {
            // little-endian
            return (ushort)(Read(addr + 1) << 8 | Read(addr));
        }

        public override void Write(int address, byte val)
        {
            ROM[address] = val;
        }
    }
}
