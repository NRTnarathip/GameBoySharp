namespace GameBoySharp;

public sealed class RegisterFlag : Register8Bit
{
    // ZNHC_0000 

    bool _Z;
    public bool Z
    {
        get => _Z;
        set
        {
            _Z = value;
            _value = _value.SetBit(7, value);
        }
    }
    bool _N;
    public bool N
    {
        get => _N;
        set
        {
            _N = value;
            _value = _value.SetBit(6, value);
        }
    }
    bool _H;
    public bool H
    {
        get => _H;
        set
        {
            _H = value;
            _value = _value.SetBit(5, value);
        }
    }
    bool _C;
    public bool C
    {
        get => _C;
        set
        {
            _C = value;
            _value = _value.SetBit(5, value);
        }
    }

    public RegisterFlag() : base("F")
    {

    }

    protected override void SetterValue(byte newValue)
    {
        // update flags
        Z = (newValue & 0x80) == 0x1;
        N = (newValue & 0x40) == 0x1;
        H = (newValue & 0x20) == 0x1;
        C = (newValue & 0x10) == 0x1;
    }
}
