

namespace GameBoySharp;

public sealed class Register16BitHiLo
{
    public readonly string name;
    public readonly Register8Bit lo, hi;
    public ushort value { get => GetValue(); set => SetValue(value); }


    public Register16BitHiLo(string name, Register8Bit hi, Register8Bit lo)
    {
        this.name = name;
        this.hi = hi;
        this.lo = lo;
    }

    ushort GetValue()
    {
        return (ushort)(hi.value << 8 | lo.value);
    }

    void SetValue(ushort value)
    {
        byte loByte = (byte)(value & 0xF);
        byte hiByte = (byte)(value >> 8 & 0xF);

        lo.value = loByte;
        hi.value = hiByte;
    }

    public static implicit operator ushort(Register16BitHiLo register)
    {
        return register.value;
    }
}
