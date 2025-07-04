﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp
{
    public sealed class MBC1 : MBCBase
    {
        private byte[] ERAM = new byte[0x8000]; //MBC1 MAX ERAM on 4 banks
        private bool ERAM_ENABLED;
        private int ROM_BANK = 1; //default as 0 is 0x0000 - 0x3FFF fixed
        private int RAM_BANK;
        private int BANKING_MODE; // 0 ROM / 1 RAM
        private const int ROM_OFFSET = 0x4000;
        private const int ERAM_OFFSET = 0x2000;

        public MBC1(byte[] bytes) : base(bytes)
        {
        }

        public override byte ReadExternalRAM(ushort addr)
        {
            if (ERAM_ENABLED)
                return ERAM[(ERAM_OFFSET * RAM_BANK) + (addr & 0x1FFF)];
            else
                return 0xFF;
        }

        public override void WriteExternalRAM(ushort addr, byte value)
        {
            if (ERAM_ENABLED)
                ERAM[(ERAM_OFFSET * RAM_BANK) + addr & 0x1FFF] = value;
        }


        byte ReadLoROM(ushort addr)
        {
            return ROM[addr];
        }

        byte ReadHiROM(ushort addr)
        {
            return ROM[(ROM_OFFSET * ROM_BANK) + (addr & 0x3FFF)];
        }

        public override byte ReadROM(ushort address)
        {
            if (address <= 0x3FFF)
                return ReadLoROM(address);

            return ReadHiROM(address);
        }

        public override void WriteROM(ushort addr, byte value)
        {
            switch (addr)
            {
                case ushort _ when addr < 0x2000:
                    ERAM_ENABLED = value == 0x0A ? true : false;
                    break;
                case ushort _ when addr < 0x4000:
                    ROM_BANK = value & 0x1F; //only last 5bits are used
                    if (ROM_BANK == 0x00 || ROM_BANK == 0x20 || ROM_BANK == 0x40 || ROM_BANK == 0x60)
                        ROM_BANK++;
                    break;
                case ushort _ when addr < 0x6000:
                    if (BANKING_MODE == 0)
                    {
                        ROM_BANK |= value & 0x3;
                        if (ROM_BANK == 0x00 || ROM_BANK == 0x20 || ROM_BANK == 0x40 || ROM_BANK == 0x60)
                            ROM_BANK++;
                    }
                    else
                    {
                        RAM_BANK = value & 0x3;
                    }
                    break;
                case ushort _ when addr <= 0x8000:
                    // 00h = ROM Banking Mode (up to 8KByte RAM, 2MByte ROM) (default)
                    // 01h = RAM Banking Mode(up to 32KByte RAM, 512KByte ROM)
                    BANKING_MODE = value & 0x1;
                    break;
            }
        }

    }
}
