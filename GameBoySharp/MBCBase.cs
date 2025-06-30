

namespace GameBoySharp
{
    public abstract class MBCBase
    {
        public abstract byte ReadROM(ushort address);
        public abstract byte ReadExternalRAM(ushort addr);

        public abstract void WriteROM(ushort address, byte b);
        public void WriteROM(int addr, byte b) => WriteROM((ushort)addr, b);

        public abstract void WriteExternalRAM(ushort addr, byte val);
        public void WriteExternalRAM(int addr, byte b) => WriteExternalRAM((ushort)addr, b);
    }
}
