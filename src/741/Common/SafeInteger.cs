using System;

namespace DarkAges.Library.Common;

public class SafeInteger
{
    private int _value;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly bool _allowOverflow;

    public SafeInteger(int value = 0, int minValue = int.MinValue, int maxValue = int.MaxValue, bool allowOverflow = false)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _allowOverflow = allowOverflow;
        _value = ClampValue(value);
    }

    public int Value
    {
        get => _value;
        set => _value = ClampValue(value);
    }

    public int MinValue => _minValue;
    public int MaxValue => _maxValue;
    public bool AllowOverflow => _allowOverflow;

    private int ClampValue(int value)
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

    public static SafeInteger operator +(SafeInteger a, SafeInteger b)
    {
        return new SafeInteger(a._value + b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator +(SafeInteger a, int b)
    {
        return new SafeInteger(a._value + b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator -(SafeInteger a, SafeInteger b)
    {
        return new SafeInteger(a._value - b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator -(SafeInteger a, int b)
    {
        return new SafeInteger(a._value - b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator *(SafeInteger a, SafeInteger b)
    {
        return new SafeInteger(a._value * b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator *(SafeInteger a, int b)
    {
        return new SafeInteger(a._value * b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator /(SafeInteger a, SafeInteger b)
    {
        if (b._value == 0)
            throw new DivideByZeroException();
        return new SafeInteger(a._value / b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator /(SafeInteger a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        return new SafeInteger(a._value / b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator %(SafeInteger a, SafeInteger b)
    {
        if (b._value == 0)
            throw new DivideByZeroException();
        return new SafeInteger(a._value % b._value, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator %(SafeInteger a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        return new SafeInteger(a._value % b, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator ++(SafeInteger a)
    {
        return new SafeInteger(a._value + 1, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static SafeInteger operator --(SafeInteger a)
    {
        return new SafeInteger(a._value - 1, a._minValue, a._maxValue, a._allowOverflow);
    }

    public static bool operator ==(SafeInteger a, SafeInteger b)
    {
        return a._value == b._value;
    }

    public static bool operator ==(SafeInteger a, int b)
    {
        return a._value == b;
    }

    public static bool operator !=(SafeInteger a, SafeInteger b)
    {
        return a._value != b._value;
    }

    public static bool operator !=(SafeInteger a, int b)
    {
        return a._value != b;
    }

    public static bool operator <(SafeInteger a, SafeInteger b)
    {
        return a._value < b._value;
    }

    public static bool operator <(SafeInteger a, int b)
    {
        return a._value < b;
    }

    public static bool operator >(SafeInteger a, SafeInteger b)
    {
        return a._value > b._value;
    }

    public static bool operator >(SafeInteger a, int b)
    {
        return a._value > b;
    }

    public static bool operator <=(SafeInteger a, SafeInteger b)
    {
        return a._value <= b._value;
    }

    public static bool operator <=(SafeInteger a, int b)
    {
        return a._value <= b;
    }

    public static bool operator >=(SafeInteger a, SafeInteger b)
    {
        return a._value >= b._value;
    }

    public static bool operator >=(SafeInteger a, int b)
    {
        return a._value >= b;
    }

    public static explicit operator int(SafeInteger a)
    {
        return a._value;
    }

    public static implicit operator SafeInteger(int value)
    {
        return new SafeInteger(value);
    }

    public override bool Equals(object obj)
    {
        if (obj is SafeInteger other)
            return _value == other._value;
        if (obj is int intValue)
            return _value == intValue;
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

    public void Add(int amount)
    {
        _value = ClampValue(_value + amount);
    }

    public void Subtract(int amount)
    {
        _value = ClampValue(_value - amount);
    }

    public void Multiply(int factor)
    {
        _value = ClampValue(_value * factor);
    }

    public void Divide(int divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();
        _value = ClampValue(_value / divisor);
    }

    public void Modulo(int divisor)
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
        var value = _minValue + (int)((double)range * percentage / 100);
        _value = ClampValue(value);
    }
}