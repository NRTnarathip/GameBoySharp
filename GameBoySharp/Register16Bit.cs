


namespace GameBoySharp;

public sealed class Register16Bit : RegisterBase<ushort>
{
    public readonly Register8Bit lo, hi;

    public Register16Bit(string name, Register8Bit hi, Register8Bit lo) : base(name)
    {
        this.hi = hi;
        this.lo = lo;
        hi.OnSetterValue = OnHiLowSetterValue;
        lo.OnSetterValue = OnHiLowSetterValue;
    }

    void OnHiLowSetterValue()
    {
        _value = (ushort)(hi.value << 8 | lo.value);
    }

    public override void Decrement()
    {
        value = (ushort)(_value - 1);
    }
    public override void Increment()
    {
        value = (ushort)(_value + 1);
    }

    protected override void SetterValue(ushort newValue)
    {
        if (_value == newValue)
            return;

        _value = newValue;
        hi.value = (byte)(newValue >> 8);
        lo.value = (byte)newValue;
    }

    public static implicit operator ushort(Register16Bit register)
    {
        return register.value;
    }
}
