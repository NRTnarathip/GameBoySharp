
namespace GameBoySharp
{
    public abstract class MBCBase
    {
        public abstract byte Read(int address);
        public abstract ushort Read16(ushort address);

        public abstract void Write(int address, byte val);
    }
}
