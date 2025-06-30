

namespace GameBoySharp;

public sealed class Register16Bit : RegisterBase<ushort>
{
    public readonly Register8Bit lo, hi;

    public Register16Bit(string name, Register8Bit hi, Register8Bit lo) : base(name)
    {
        this.hi = hi;
        this.lo = lo;
    }

    public override void Decrement()
    {
        value = (ushort)(value - 1);
    }
    public override void Increment()
    {
        value = (ushort)(value + 1);
    }

    protected override ushort GetValueProperty()
    {
        return (ushort)(hi.value << 8 | lo.value);
    }

    protected override void SetValueProperty(ushort value)
    {
        byte loByte = (byte)(value & 0xF);
        byte hiByte = (byte)(value >> 8 & 0xF);

        lo.value = loByte;
        hi.value = hiByte;
    }

    public static implicit operator ushort(Register16Bit register)
    {
        return register.value;
    }
}
