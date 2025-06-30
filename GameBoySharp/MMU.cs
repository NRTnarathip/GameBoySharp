
using System.Diagnostics;

namespace GameBoySharp;

public sealed class MMU
{
    byte[] VRAM = new byte[0x2000];
    byte[] WRAM0 = new byte[0x1000];
    byte[] WRAM1 = new byte[0x1000];
    byte[] OAM = new byte[0xA0];
    byte[] IO = new byte[0x80];
    byte[] HRAM = new byte[0x80];

    MBCBase mbc;
    public MMU()
    {
        //FF4D - KEY1 - CGB Mode Only - Prepare Speed Switch
        //HardCoded to FF to identify DMG as 00 is GBC
        IO[0x4D] = 0xFF;

        IO[0x10] = 0x80;
        IO[0x11] = 0xBF;
        IO[0x12] = 0xF3;
        IO[0x14] = 0xBF;
        IO[0x16] = 0x3F;
        IO[0x19] = 0xBF;
        IO[0x1A] = 0x7F;
        IO[0x1B] = 0xFF;
        IO[0x1C] = 0x9F;
        IO[0x1E] = 0xBF;
        IO[0x20] = 0xFF;
        IO[0x23] = 0xBF;
        IO[0x24] = 0x77;
        IO[0x25] = 0xF3;
        IO[0x26] = 0xF1;
        IO[0x40] = 0x91;
        IO[0x47] = 0xFC;
        IO[0x48] = 0xFF;
        IO[0x49] = 0xFF;
    }
    public void Init(GamePak game)
    {
        this.mbc = game.mbc!;
    }

    public byte ReadByte(ushort addr)
    {
        var hex = addr.ToHex();
        if (addr is 0xC000)
        {
            Console.WriteLine();
        }
        switch (addr)
        {
            // Low, Hi Rom
            case <= 0x7FFF:
                return mbc.ReadROM(addr);
            case <= 0x9FFF:
                // 8000-9FFF 8KB Video RAM(VRAM)(switchable bank 0-1 in CGB Mode)
                return VRAM[addr & 0x1FFF];
            case <= 0xBFFF:
                // A000-BFFF 8KB External RAM(in cartridge, switchable bank, if any)
                return mbc.ReadExternalRAM(addr);
            case <= 0xCFFF:
                // C000-CFFF 4KB Work RAM Bank 0(WRAM) <br/>
                return WRAM0[addr & 0xFFF];
            case <= 0xDFFF:
                // D000-DFFF 4KB Work RAM Bank 1(WRAM)(switchable bank 1-7 in CGB Mode) <br/>
                return WRAM1[addr & 0xFFF];
            case <= 0xEFFF:
                // E000-FDFF Same as 0xC000-DDFF(ECHO)  
                return WRAM0[addr & 0xFFF];
            case <= 0xFDFF:
                // E000-FDFF Same as 0xC000-DDFF(ECHO)
                return WRAM1[addr & 0xFFF];
            case <= 0xFE9F:
                // FE00-FE9F Sprite Attribute Table(OAM)
                return OAM[addr - 0xFE00];
            case <= 0xFEFF:
                // FEA0-FEFF Not Usable 0
                return 0x00;
            case <= 0xFF7F:
                // FF00-FF7F IO Ports
                return IO[addr & 0x7F];
            case <= 0xFFFF:
                // FF80-FFFE High RAM(HRAM)
                return HRAM[addr & 0x7F];
        }
    }
    public byte ReadByte(int addr) => ReadByte((ushort)addr);
    public ushort ReadWord(ushort addr)
    {
        return (ushort)(ReadByte(addr + 1) << 8 | ReadByte(addr));
    }

    public void WriteByte(int addr, byte b)
    {
        switch (addr)
        {                            // General Memory Map 64KB
            case <= 0x7FFF:     //0000-3FFF 16KB ROM Bank 00 (in cartridge, private at bank 00) 4000-7FFF 16KB ROM Bank 01..NN(in cartridge, switchable bank number)
                mbc.WriteROM(addr, b);
                break;
            case <= 0x9FFF:    // 8000-9FFF 8KB Video RAM(VRAM)(switchable bank 0-1 in CGB Mode)
                VRAM[addr & 0x1FFF] = b;
                break;
            case <= 0xBFFF:    // A000-BFFF 8KB External RAM(in cartridge, switchable bank, if any)
                mbc.WriteExternalRAM(addr, b);
                break;
            case <= 0xCFFF:    // C000-CFFF 4KB Work RAM Bank 0(WRAM) <br/>
                var hex = addr.ToHex();
                var pc = CPU.PC;
                WRAM0[addr & 0xFFF] = b;
                break;
            case <= 0xDFFF:    // D000-DFFF 4KB Work RAM Bank 1(WRAM)(switchable bank 1-7 in CGB Mode)
                WRAM1[addr & 0xFFF] = b;
                break;
            case <= 0xEFFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)  
                WRAM0[addr & 0xFFF] = b;
                break;
            case <= 0xFDFF:    // E000-FDFF Same as 0xC000-DDFF(ECHO)
                WRAM1[addr & 0xFFF] = b;
                break;
            case <= 0xFE9F:    // FE00-FE9F Sprite Attribute Table(OAM)
                OAM[addr & 0x9F] = b;
                break;
            case <= 0xFEFF:    // FEA0-FEFF Not Usable
                break;
            case <= 0xFF7F:    // FF00-FF7F IO Ports
                switch (addr)
                {
                    //case 0xFF00: b |= 0xC0; break;
                    case 0xFF0F: b |= 0xE0; break; // IF returns 1 on first 3 unused bits
                    case 0xFF04:                //DIV on write = 0
                    case 0xFF44: b = 0; break;  //LY on write = 0
                    case 0xFF46: DMA(b); break;
                }
                //if (addr == 0xFF02 && b == 0x81) { //Temp Serial Link output for debug
                //Console.Write(Convert.ToChar(readByte(0xFF01)));
                //Console.ReadLine();
                //}
                IO[addr & 0x7F] = b;
                break;
            case <= 0xFFFF:    // FF80-FFFE High RAM(HRAM)
                HRAM[addr & 0x7F] = b;
                break;
        }
    }
    public void WriteWord(ushort addr, ushort w)
    {
        var stack = new StackTrace();
        var farmes = stack.GetFrames().ToArray();
        WriteByte(addr + 1, (byte)(w >> 8));
        WriteByte(addr, (byte)w);
    }
    public void WriteWord(int addr, ushort w) => WriteWord((ushort)addr, w);

    void DMA(byte b)
    {
        ushort addr = (ushort)(b << 8);
        for (byte i = 0; i < OAM.Length; i++)
        {
            OAM[i] = ReadByte((ushort)(addr + i));
        }
    }
}
