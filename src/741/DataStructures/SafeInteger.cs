using System;
using System.Runtime.InteropServices;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// Provides safe integer operations with obfuscated storage
/// </summary>
public class SafeInteger : IDisposable
{
    private const int STORAGE_SIZE = 16;
    private const int BYTE_COUNT = 8;
        
    private byte[] storage;
    private bool isDisposed;

    // Scrambled byte positions for obfuscation
    private static readonly int[] bytePositions = [7, 15, 3, 0, 9, 2, 13, 5];

    public SafeInteger()
    {
        storage = new byte[STORAGE_SIZE];
        isDisposed = false;
        Clear();
    }

    public SafeInteger(int value) : this()
    {
        SetValue(value);
    }

    /// <summary>
    /// Gets the current integer value
    /// </summary>
    /// <returns>The stored integer value</returns>
    public int GetValue()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(SafeInteger));

        var result = 0;
            
        for (var i = 0; i < BYTE_COUNT; i++)
        {
            var byteValue = GetByte(i);
            result |= ((byteValue >> 2) & 0xF) << (4 * i);
        }
            
        return result;
    }

    /// <summary>
    /// Sets the integer value with obfuscation
    /// </summary>
    /// <param name="value">The value to store</param>
    public void SetValue(int value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(SafeInteger));

        for (var i = 0; i < BYTE_COUNT; i++)
        {
            var byteValue = GetByte(i);
            byteValue &= 0xC3; // Clear the 4-bit value area
            byteValue |= (byte)(4 * ((value >> (4 * i)) & 0xF));
            SetByte(i, byteValue);
        }
    }

    /// <summary>
    /// Increments the stored value
    /// </summary>
    public void Increment()
    {
        SetValue(GetValue() + 1);
    }

    /// <summary>
    /// Decrements the stored value
    /// </summary>
    public void Decrement()
    {
        SetValue(GetValue() - 1);
    }

    /// <summary>
    /// Adds a value to the stored integer
    /// </summary>
    /// <param name="value">Value to add</param>
    public void Add(int value)
    {
        SetValue(GetValue() + value);
    }

    /// <summary>
    /// Subtracts a value from the stored integer
    /// </summary>
    /// <param name="value">Value to subtract</param>
    public void Subtract(int value)
    {
        SetValue(GetValue() - value);
    }

    /// <summary>
    /// Multiplies the stored value by a factor
    /// </summary>
    /// <param name="factor">Multiplication factor</param>
    public void Multiply(int factor)
    {
        SetValue(GetValue() * factor);
    }

    /// <summary>
    /// Divides the stored value by a divisor
    /// </summary>
    /// <param name="divisor">Division divisor</param>
    public void Divide(int divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();
            
        SetValue(GetValue() / divisor);
    }

    /// <summary>
    /// Performs modulo operation on the stored value
    /// </summary>
    /// <param name="modulus">Modulus value</param>
    public void Modulo(int modulus)
    {
        if (modulus == 0)
            throw new DivideByZeroException();
            
        SetValue(GetValue() % modulus);
    }

    /// <summary>
    /// Performs bitwise AND operation
    /// </summary>
    /// <param name="value">Value to AND with</param>
    public void And(int value)
    {
        SetValue(GetValue() & value);
    }

    /// <summary>
    /// Performs bitwise OR operation
    /// </summary>
    /// <param name="value">Value to OR with</param>
    public void Or(int value)
    {
        SetValue(GetValue() | value);
    }

    /// <summary>
    /// Performs bitwise XOR operation
    /// </summary>
    /// <param name="value">Value to XOR with</param>
    public void Xor(int value)
    {
        SetValue(GetValue() ^ value);
    }

    /// <summary>
    /// Performs bitwise NOT operation
    /// </summary>
    public void Not()
    {
        SetValue(~GetValue());
    }

    /// <summary>
    /// Performs left shift operation
    /// </summary>
    /// <param name="shift">Number of bits to shift</param>
    public void LeftShift(int shift)
    {
        SetValue(GetValue() << shift);
    }

    /// <summary>
    /// Performs right shift operation
    /// </summary>
    /// <param name="shift">Number of bits to shift</param>
    public void RightShift(int shift)
    {
        SetValue(GetValue() >> shift);
    }

    /// <summary>
    /// Compares the stored value with another value
    /// </summary>
    /// <param name="value">Value to compare with</param>
    /// <returns>Comparison result (-1 if less, 0 if equal, 1 if greater)</returns>
    public int CompareTo(int value)
    {
        var currentValue = GetValue();
        if (currentValue < value) return -1;
        if (currentValue > value) return 1;
        return 0;
    }

    /// <summary>
    /// Checks if the stored value equals another value
    /// </summary>
    /// <param name="value">Value to compare with</param>
    /// <returns>True if values are equal</returns>
    public bool Equals(int value)
    {
        return GetValue() == value;
    }

    /// <summary>
    /// Checks if the stored value is zero
    /// </summary>
    /// <returns>True if the value is zero</returns>
    public bool IsZero()
    {
        return GetValue() == 0;
    }

    /// <summary>
    /// Checks if the stored value is positive
    /// </summary>
    /// <returns>True if the value is positive</returns>
    public bool IsPositive()
    {
        return GetValue() > 0;
    }

    /// <summary>
    /// Checks if the stored value is negative
    /// </summary>
    /// <returns>True if the value is negative</returns>
    public bool IsNegative()
    {
        return GetValue() < 0;
    }

    /// <summary>
    /// Gets the absolute value
    /// </summary>
    /// <returns>The absolute value</returns>
    public int GetAbsoluteValue()
    {
        var value = GetValue();
        return value < 0 ? -value : value;
    }

    /// <summary>
    /// Sets the absolute value
    /// </summary>
    public void SetAbsoluteValue()
    {
        SetValue(GetAbsoluteValue());
    }

    /// <summary>
    /// Clamps the value to a specified range
    /// </summary>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    public void Clamp(int min, int max)
    {
        var value = GetValue();
        if (value < min) value = min;
        if (value > max) value = max;
        SetValue(value);
    }

    /// <summary>
    /// Swaps the value with another SafeInteger
    /// </summary>
    /// <param name="other">The other SafeInteger to swap with</param>
    public void Swap(SafeInteger other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        var temp = GetValue();
        SetValue(other.GetValue());
        other.SetValue(temp);
    }

    /// <summary>
    /// Copies the value from another SafeInteger
    /// </summary>
    /// <param name="other">The SafeInteger to copy from</param>
    public void CopyFrom(SafeInteger other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        SetValue(other.GetValue());
    }

    /// <summary>
    /// Clears the stored value (sets to zero)
    /// </summary>
    public void Clear()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(SafeInteger));

        Array.Clear(storage, 0, storage.Length);
    }

    /// <summary>
    /// Gets a byte from the obfuscated storage
    /// </summary>
    /// <param name="index">Byte index (0-7)</param>
    /// <returns>The byte value</returns>
    private byte GetByte(int index)
    {
        if (index < 0 || index >= BYTE_COUNT)
            throw new ArgumentOutOfRangeException(nameof(index));

        return storage[bytePositions[index]];
    }

    /// <summary>
    /// Sets a byte in the obfuscated storage
    /// </summary>
    /// <param name="index">Byte index (0-7)</param>
    /// <param name="value">Byte value to set</param>
    private void SetByte(int index, byte value)
    {
        if (index < 0 || index >= BYTE_COUNT)
            throw new ArgumentOutOfRangeException(nameof(index));

        storage[bytePositions[index]] = value;
    }

    /// <summary>
    /// Gets a hash code for the current value
    /// </summary>
    /// <returns>Hash code</returns>
    public override int GetHashCode()
    {
        return GetValue().GetHashCode();
    }

    /// <summary>
    /// Converts the SafeInteger to a string representation
    /// </summary>
    /// <returns>String representation of the value</returns>
    public override string ToString()
    {
        return GetValue().ToString();
    }

    /// <summary>
    /// Converts the SafeInteger to a string with a specific format
    /// </summary>
    /// <param name="format">Format string</param>
    /// <returns>Formatted string representation</returns>
    public string ToString(string format)
    {
        return GetValue().ToString(format);
    }

    /// <summary>
    /// Converts the SafeInteger to a string with a specific format and provider
    /// </summary>
    /// <param name="format">Format string</param>
    /// <param name="provider">Format provider</param>
    /// <returns>Formatted string representation</returns>
    public string ToString(string format, IFormatProvider provider)
    {
        return GetValue().ToString(format, provider);
    }

    // Implicit conversion operators
    public static implicit operator int(SafeInteger safeInt)
    {
        return safeInt?.GetValue() ?? 0;
    }

    public static implicit operator SafeInteger(int value)
    {
        return new SafeInteger(value);
    }

    // Arithmetic operators
    public static SafeInteger operator +(SafeInteger a, int b)
    {
        if (a == null) return new SafeInteger(b);
        return new SafeInteger(a.GetValue() + b);
    }

    public static SafeInteger operator +(SafeInteger a, SafeInteger b)
    {
        if (a == null) return b;
        if (b == null) return a;
        return new SafeInteger(a.GetValue() + b.GetValue());
    }

    public static SafeInteger operator -(SafeInteger a, int b)
    {
        if (a == null) return new SafeInteger(-b);
        return new SafeInteger(a.GetValue() - b);
    }

    public static SafeInteger operator -(SafeInteger a, SafeInteger b)
    {
        if (a == null) return b == null ? new SafeInteger(0) : new SafeInteger(-b.GetValue());
        if (b == null) return a;
        return new SafeInteger(a.GetValue() - b.GetValue());
    }

    public static SafeInteger operator *(SafeInteger a, int b)
    {
        if (a == null) return new SafeInteger(0);
        return new SafeInteger(a.GetValue() * b);
    }

    public static SafeInteger operator *(SafeInteger a, SafeInteger b)
    {
        if (a == null || b == null) return new SafeInteger(0);
        return new SafeInteger(a.GetValue() * b.GetValue());
    }

    public static SafeInteger operator /(SafeInteger a, int b)
    {
        if (a == null) return new SafeInteger(0);
        if (b == 0) throw new DivideByZeroException();
        return new SafeInteger(a.GetValue() / b);
    }

    public static SafeInteger operator /(SafeInteger a, SafeInteger b)
    {
        if (a == null) return new SafeInteger(0);
        if (b == null || b.GetValue() == 0) throw new DivideByZeroException();
        return new SafeInteger(a.GetValue() / b.GetValue());
    }

    public static SafeInteger operator %(SafeInteger a, int b)
    {
        if (a == null) return new SafeInteger(0);
        if (b == 0) throw new DivideByZeroException();
        return new SafeInteger(a.GetValue() % b);
    }

    public static SafeInteger operator %(SafeInteger a, SafeInteger b)
    {
        if (a == null) return new SafeInteger(0);
        if (b == null || b.GetValue() == 0) throw new DivideByZeroException();
        return new SafeInteger(a.GetValue() % b.GetValue());
    }

    // Comparison operators
    public static bool operator ==(SafeInteger a, int b)
    {
        if (a == null) return b == 0;
        return a.GetValue() == b;
    }

    public static bool operator !=(SafeInteger a, int b)
    {
        return !(a == b);
    }

    public static bool operator <(SafeInteger a, int b)
    {
        if (a == null) return 0 < b;
        return a.GetValue() < b;
    }

    public static bool operator >(SafeInteger a, int b)
    {
        if (a == null) return 0 > b;
        return a.GetValue() > b;
    }

    public static bool operator <=(SafeInteger a, int b)
    {
        return !(a > b);
    }

    public static bool operator >=(SafeInteger a, int b)
    {
        return !(a < b);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                Array.Clear(storage, 0, storage.Length);
                storage = null;
            }
            isDisposed = true;
        }
    }
}