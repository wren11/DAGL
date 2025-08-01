namespace DarkAges.Library.Common;

public class SafeUInteger
{
    private uint _value;
    private readonly uint _minValue;
    private readonly uint _maxValue;
    private readonly bool _allowOverflow;

    public SafeUInteger(uint value = 0, uint minValue = 0, uint maxValue = uint.MaxValue, bool allowOverflow = false)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _allowOverflow = allowOverflow;
        _value = ClampValue(value);
    }

    public uint Value
    {
        get => _value;
        set => _value = ClampValue(value);
    }

    public uint MinValue => _minValue;
    public uint MaxValue => _maxValue;
    public bool AllowOverflow => _allowOverflow;

    private uint ClampValue(uint value)
    {
        if (_allowOverflow)
        {
            if (value > _maxValue)
                return _minValue + (value - _maxValue - 1);
            if (value < _minValue)
                return _maxValue - (_minValue - value - 1);
        }
        else
        {
            if (value > _maxValue)
                return _maxValue;
            if (value < _minValue)
                return _minValue;
        }

        return value;
    }

    public static SafeUInteger operator +(SafeUInteger a, SafeUInteger b)
    {
        return new SafeUInteger(a._value + b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator +(SafeUInteger a, uint b)
    {
        return new SafeUInteger(a._value + b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator -(SafeUInteger a, SafeUInteger b)
    {
        return new SafeUInteger(a._value - b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator -(SafeUInteger a, uint b)
    {
        return new SafeUInteger(a._value - b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator *(SafeUInteger a, SafeUInteger b)
    {
        return new SafeUInteger(a._value * b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator *(SafeUInteger a, uint b)
    {
        return new SafeUInteger(a._value * b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator /(SafeUInteger a, SafeUInteger b)
    {
        if (b._value == 0)
            throw new DivideByZeroException();
        return new SafeUInteger(a._value / b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator /(SafeUInteger a, uint b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        return new SafeUInteger(a._value / b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator %(SafeUInteger a, SafeUInteger b)
    {
        if (b._value == 0)
            throw new DivideByZeroException();
        return new SafeUInteger(a._value % b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator %(SafeUInteger a, uint b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        return new SafeUInteger(a._value % b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator ++(SafeUInteger a)
    {
        return new SafeUInteger(a._value + 1, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeUInteger operator --(SafeUInteger a)
    {
        return new SafeUInteger(a._value - 1, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static bool operator ==(SafeUInteger a, SafeUInteger b)
    {
        return a._value == b._value;
    }

    public static bool operator ==(SafeUInteger a, uint b)
    {
        return a._value == b;
    }

    public static bool operator !=(SafeUInteger a, SafeUInteger b)
    {
        return a._value != b._value;
    }

    public static bool operator !=(SafeUInteger a, uint b)
    {
        return a._value != b;
    }

    public static bool operator <(SafeUInteger a, SafeUInteger b)
    {
        return a._value < b._value;
    }

    public static bool operator <(SafeUInteger a, uint b)
    {
        return a._value < b;
    }

    public static bool operator >(SafeUInteger a, SafeUInteger b)
    {
        return a._value > b._value;
    }

    public static bool operator >(SafeUInteger a, uint b)
    {
        return a._value > b;
    }

    public static bool operator <=(SafeUInteger a, SafeUInteger b)
    {
        return a._value <= b._value;
    }

    public static bool operator <=(SafeUInteger a, uint b)
    {
        return a._value <= b;
    }

    public static bool operator >=(SafeUInteger a, SafeUInteger b)
    {
        return a._value >= b._value;
    }

    public static bool operator >=(SafeUInteger a, uint b)
    {
        return a._value >= b;
    }

    public static explicit operator uint(SafeUInteger a)
    {
        return a._value;
    }

    public static implicit operator SafeUInteger(uint value)
    {
        return new SafeUInteger(value);
    }

    public override bool Equals(object obj)
    {
        if (obj is SafeUInteger other)
            return _value == other._value;
        if (obj is uint uintValue)
            return _value == uintValue;
        return false;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString();
    }

    public bool IsAtMin => _value == _minValue;
    public bool IsAtMax => _value == _maxValue;
    public bool IsInRange => _value >= _minValue && _value <= _maxValue;

    public void SetToMin()
    {
        _value = _minValue;
    }

    public void SetToMax()
    {
        _value = _maxValue;
    }

    public void Add(uint amount)
    {
        _value = ClampValue(_value + amount);
    }

    public void Subtract(uint amount)
    {
        _value = ClampValue(_value - amount);
    }

    public void Multiply(uint factor)
    {
        _value = ClampValue(_value * factor);
    }

    public void Divide(uint divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();
        _value = ClampValue(_value / divisor);
    }

    public void Modulo(uint divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();
        _value = ClampValue(_value % divisor);
    }

    public void Increment()
    {
        _value = ClampValue(_value + 1);
    }

    public void Decrement()
    {
        _value = ClampValue(_value - 1);
    }

    public int GetPercentage()
    {
        if (_maxValue == _minValue)
            return 0;

        var range = _maxValue - _minValue;
        var current = _value - _minValue;
        return (int)((double)current / range * 100);
    }

    public void SetPercentage(int percentage)
    {
        if (percentage < 0) percentage = 0;
        if (percentage > 100) percentage = 100;

        var range = _maxValue - _minValue;
        var value = _minValue + (uint)((double)range * percentage / 100);
        _value = ClampValue(value);
    }
}