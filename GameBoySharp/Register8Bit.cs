

namespace GameBoySharp;

public class Register8Bit : RegisterBase<byte>
{
    public Register8Bit(string name) : base(name)
    {
    }

    public override void Decrement()
    {
        value--;
    }

    public override void Increment()
    {
        value++;
    }

    public static implicit operator byte(Register8Bit v)
    {
        return v._value;
    }
}
