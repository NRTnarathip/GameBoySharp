
namespace GameBoySharp;

public class Register8Bit
{
    public readonly string name;
    byte _value;

    public byte value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public Register8Bit(string name)
    {
        this.name = name;
    }

    public virtual void SetValue(byte newValue)
    {
        this._value = newValue;
    }

    public virtual byte GetValue()
    {
        return _value;
    }

    public static implicit operator byte(Register8Bit v)
    {
        return v._value;
    }
}
