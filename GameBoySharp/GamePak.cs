using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public sealed class GamePak
{
    public MBCBase? mbc;

    public readonly byte mbcType;
    public GamePak(byte[] rom)
    {
        // read MBC type
        mbcType = rom[0x147];

        switch (mbcType)
        {
            case 0x0:
                mbc = new MBC0(rom);
                break;
            default:
                throw new NotImplementedException();
        }

    }
}
