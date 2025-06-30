

namespace GameBoySharp;

public interface IRegisterBase
{
    public void SetValue(ushort val);
    public void SetValue(int val);
    // support 8bit, 16bit
    public ushort GetValue();
}

public abstract class RegisterBase<T> : IRegisterBase
{
    public readonly string name;
    protected T _value;
    public T value { get => GetValueProperty(); set => SetValueProperty(value); }
    public int valueBytes { get; private set; }
    public Type valueType { get; private set; }

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
    }

    protected virtual void SetValueProperty(T newValue)
    {
        this._value = newValue;
    }

    protected virtual T GetValueProperty()
    {
        return _value;
    }

    public ushort GetValue()
    {
        return Convert.ToUInt16(_value);
    }

    public void SetValue(T newValue) => _value = newValue;

    // support 8bit, 16bit
    public void SetValue(ushort newValue)
    {
        // match value within type
        if (newValue is T newValueTypeCast)
        {
            SetValue(newValueTypeCast);
            return;
        }

        // convert newValue ushort to byte
        var newValueByte = (object)Convert.ToByte(newValue);
        SetValue((T)newValueByte);
    }

    public abstract void Increment();
    public abstract void Decrement();

    public void SetValue(int val) => SetValue((ushort)val);
}
