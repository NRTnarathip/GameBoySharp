

namespace GameBoySharp;

public interface IRegisterBase
{
    public ushort GetValue();
    public void SetValue(ushort val);
    public void SetValue(int val);
    public abstract void Increment();
    public abstract void Decrement();
}

public abstract class RegisterBase<T> : IRegisterBase
{
    public readonly string name;
    public Action OnSetterValue;
    protected T _value;
    public T value { get => GetterValue(); set => SetterValue(value); }
    public readonly int valueBytes;
    public readonly Type valueType;
    public readonly bool is8Bit;
    public readonly bool is16Bit;

    public RegisterBase(string name)
    {
        this.name = name;
        valueType = typeof(T);

        if (_value is byte)
            valueBytes = 1;
        else if (_value is ushort)
            valueBytes = 2;
        else
            throw new NotSupportedException();

        is8Bit = valueBytes == 1;
        is16Bit = !is8Bit;
    }

    protected virtual void SetterValue(T newValue)
    {
        _value = newValue;
        OnSetterValue?.Invoke();
    }

    protected virtual T GetterValue()
    {
        return _value;
    }

    public ushort GetValue()
    {
        return Convert.ToUInt16(value);
    }

    // support 8bit, 16bit
    public void SetValue(ushort newValue)
    {
        // match value within type
        if ((byte)newValue is T tByte)
            value = tByte;
        else if (newValue is T tUShort)
            value = tUShort;
        else
            throw new NotImplementedException();
    }

    public void SetValue(int val) => SetValue((ushort)val);

    public virtual void Increment()
    {
        throw new NotImplementedException();
    }

    public virtual void Decrement()
    {
        throw new NotImplementedException();
    }
}
