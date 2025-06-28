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
            needUpdateFlagAfterSetValue = false;
            this.value = value ? (byte)(this.value | 0x80) : (byte)(this.value & ~0x80);
            _Z = (this.value & 0x80) == 1;
        }
    }
    bool _N;
    public bool N
    {
        get => _N;
        set
        {
            needUpdateFlagAfterSetValue = false;
            this.value = value ? (byte)(this.value | 0x40) : (byte)(this.value & ~0x40);
            _N = (this.value & 0x40) == 1;
        }
    }
    bool _H;
    public bool H
    {
        get => _H;
        set
        {
            needUpdateFlagAfterSetValue = false;
            this.value = value ? (byte)(this.value | 0x20) : (byte)(this.value & ~0x20);
            _H = (this.value & 0x20) == 1;
        }
    }
    bool _C;
    public bool C
    {
        get => _C;
        set
        {
            needUpdateFlagAfterSetValue = false;
            this.value = value ? (byte)(this.value | 0x10) : (byte)(this.value & ~0x10);
            _C = (this.value & 0x10) == 1;
        }
    }

    public RegisterFlag() : base("F")
    {

    }

    bool needUpdateFlagAfterSetValue = true;
    public override void SetValue(byte newValue)
    {
        if (needUpdateFlagAfterSetValue is false)
        {
            needUpdateFlagAfterSetValue = true;
            return;
        }

        // update flags
        Z = (newValue & 0x80) == 0x1;
        N = (newValue & 0x40) == 0x1;
        H = (newValue & 0x20) == 0x1;
        C = (newValue & 0x10) == 0x1;
    }
}
