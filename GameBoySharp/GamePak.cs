using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public sealed class GamePak
{
    public readonly MBCBase? mbc;

    public readonly byte mbcType;

    public GamePak(byte[] bytes)
    {
        // read MBC type
        mbcType = bytes[0x147];

        switch (mbcType)
        {
            case 0x0: mbc = new MBC0(bytes); break;
            case 0x1: mbc = new MBC1(bytes); break;

            default: throw new NotImplementedException();
        }

    }
}
