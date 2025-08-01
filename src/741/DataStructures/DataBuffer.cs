using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DarkAges.Library.DataStructures;

/// <summary>
/// Efficient data buffer for handling binary data with dynamic resizing
/// </summary>
public class DataBuffer : IDisposable
{
    private const int DEFAULT_CAPACITY = 1024;
    private const int GROWTH_FACTOR = 2;
    private const int MAX_CAPACITY = 0x7FEFFFFF;

    private byte[] buffer;
    private int position;
    private int length;
    private int capacity;
    private bool isDisposed;
    private readonly object syncLock = new object();

    // Events
    public event Action<int> BufferResized;
    public event Action<int> DataWritten;
    public event Action<int> DataRead;
    public event Action BufferCleared;

    public DataBuffer() : this(DEFAULT_CAPACITY)
    {
    }

    public DataBuffer(int initialCapacity)
    {
        if (initialCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity cannot be negative");

        capacity = Math.Max(initialCapacity, DEFAULT_CAPACITY);
        buffer = new byte[capacity];
        position = 0;
        length = 0;
        isDisposed = false;
    }

    public DataBuffer(byte[] data) : this(data?.Length ?? DEFAULT_CAPACITY)
    {
        if (data != null)
        {
            Write(data, 0, data.Length);
            position = 0;
        }
    }

    public int Position
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DataBuffer));
            return position;
        }
        set
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DataBuffer));

            if (value < 0 || value > length)
                throw new ArgumentOutOfRangeException(nameof(value));

            position = value;
        }
    }

    public int Length
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DataBuffer));
            return length;
        }
    }

    public int Capacity
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DataBuffer));
            return capacity;
        }
    }

    public int AvailableBytes
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DataBuffer));
            return length - position;
        }
    }

    public bool EndOfBuffer
    {
        get
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(DataBuffer));
            return position >= length;
        }
    }

    public void Write(byte value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        lock (syncLock)
        {
            EnsureCapacity(position + 1);
            buffer[position++] = value;
            if (position > length)
                length = position;
            DataWritten?.Invoke(1);
        }
    }

    public void Write(byte[] data)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        Write(data, 0, data.Length);
    }

    public void Write(byte[] data, int offset, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (offset < 0 || count < 0 || offset + count > data.Length)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            EnsureCapacity(position + count);
            Array.Copy(data, offset, buffer, position, count);
            position += count;
            if (position > length)
                length = position;
            DataWritten?.Invoke(count);
        }
    }

    public void Write(DataBuffer source)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (source == null)
            throw new ArgumentNullException(nameof(source));

        Write(source.buffer, 0, source.length);
    }

    public void WriteInt16(short value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteUInt16(ushort value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteInt32(int value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteUInt32(uint value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteInt64(long value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteUInt64(ulong value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteFloat(float value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteDouble(double value)
    {
        Write(BitConverter.GetBytes(value));
    }

    public void WriteString(string value)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var data = Encoding.UTF8.GetBytes(value);
        WriteInt32(data.Length);
        Write(data);
    }

    public void WriteStringFixed(string value, int length)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        var data = Encoding.UTF8.GetBytes(value);
        var writeLength = Math.Min(data.Length, length);
            
        Write(data, 0, writeLength);
            
        // Pad with zeros if needed
        for (var i = writeLength; i < length; i++)
        {
            Write(0);
        }
    }

    public byte ReadByte()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (position >= length)
            throw new EndOfStreamException("End of buffer reached");

        lock (syncLock)
        {
            var value = buffer[position++];
            DataRead?.Invoke(1);
            return value;
        }
    }

    public int Read(byte[] data, int offset, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (offset < 0 || count < 0 || offset + count > data.Length)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            var available = Math.Min(count, length - position);
            if (available > 0)
            {
                Array.Copy(buffer, position, data, offset, available);
                position += available;
                DataRead?.Invoke(available);
            }
            return available;
        }
    }

    public byte[] ReadBytes(int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var data = new byte[count];
        var read = Read(data, 0, count);
            
        if (read < count)
        {
            var result = new byte[read];
            Array.Copy(data, result, read);
            return result;
        }
            
        return data;
    }

    public short ReadInt16()
    {
        var data = ReadBytes(2);
        return BitConverter.ToInt16(data, 0);
    }

    public ushort ReadUInt16()
    {
        var data = ReadBytes(2);
        return BitConverter.ToUInt16(data, 0);
    }

    public int ReadInt32()
    {
        var data = ReadBytes(4);
        return BitConverter.ToInt32(data, 0);
    }

    public uint ReadUInt32()
    {
        var data = ReadBytes(4);
        return BitConverter.ToUInt32(data, 0);
    }

    public long ReadInt64()
    {
        var data = ReadBytes(8);
        return BitConverter.ToInt64(data, 0);
    }

    public ulong ReadUInt64()
    {
        var data = ReadBytes(8);
        return BitConverter.ToUInt64(data, 0);
    }

    public float ReadFloat()
    {
        var data = ReadBytes(4);
        return BitConverter.ToSingle(data, 0);
    }

    public double ReadDouble()
    {
        var data = ReadBytes(8);
        return BitConverter.ToDouble(data, 0);
    }

    public string ReadString()
    {
        var length = ReadInt32();
        if (length < 0)
            throw new InvalidDataException("Invalid string length");
            
        var data = ReadBytes(length);
        return Encoding.UTF8.GetString(data);
    }

    public string ReadStringFixed(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        var data = ReadBytes(length);
            
        // Find null terminator
        var nullIndex = Array.IndexOf(data, (byte)0);
        if (nullIndex >= 0)
        {
            var trimmedData = new byte[nullIndex];
            Array.Copy(data, trimmedData, nullIndex);
            return Encoding.UTF8.GetString(trimmedData);
        }
            
        return Encoding.UTF8.GetString(data);
    }

    public byte[] ToArray()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        lock (syncLock)
        {
            var result = new byte[length];
            Array.Copy(buffer, result, length);
            return result;
        }
    }

    public byte[] GetRange(int offset, int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (offset < 0 || count < 0 || offset + count > length)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            var result = new byte[count];
            Array.Copy(buffer, offset, result, 0, count);
            return result;
        }
    }

    public void SetRange(int offset, byte[] data)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (offset < 0 || offset + data.Length > length)
            throw new ArgumentOutOfRangeException();

        lock (syncLock)
        {
            Array.Copy(data, 0, buffer, offset, data.Length);
        }
    }

    public void Clear()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        lock (syncLock)
        {
            Array.Clear(buffer, 0, length);
            position = 0;
            length = 0;
            BufferCleared?.Invoke();
        }
    }

    public void Reset()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        position = 0;
    }

    public void Skip(int count)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        position = Math.Min(position + count, length);
    }

    public void Seek(int offset, SeekOrigin origin)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        var newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => position + offset,
            SeekOrigin.End => length + offset,
            _ => throw new ArgumentException("Invalid seek origin")
        };

        if (newPosition < 0 || newPosition > length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        position = newPosition;
    }

    public void EnsureCapacity(int requiredCapacity)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (requiredCapacity > capacity)
        {
            var newCapacity = capacity == 0 ? DEFAULT_CAPACITY : capacity * GROWTH_FACTOR;
            while (newCapacity < requiredCapacity)
            {
                newCapacity *= GROWTH_FACTOR;
            }

            if (newCapacity > MAX_CAPACITY)
                newCapacity = MAX_CAPACITY;

            if (newCapacity > capacity)
            {
                var newBuffer = new byte[newCapacity];
                if (length > 0)
                {
                    Array.Copy(buffer, newBuffer, length);
                }
                buffer = newBuffer;
                capacity = newCapacity;
                BufferResized?.Invoke(capacity);
            }
        }
    }

    public void TrimExcess()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        lock (syncLock)
        {
            if (length < capacity)
            {
                var newBuffer = new byte[length];
                Array.Copy(buffer, newBuffer, length);
                buffer = newBuffer;
                capacity = length;
                BufferResized?.Invoke(capacity);
            }
        }
    }

    public DataBuffer Clone()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        lock (syncLock)
        {
            var clone = new DataBuffer(length);
            Array.Copy(buffer, clone.buffer, length);
            clone.length = length;
            clone.position = position;
            return clone;
        }
    }

    public void CopyTo(DataBuffer destination)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(DataBuffer));

        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        destination.Write(buffer, 0, length);
    }

    public bool IsDisposed()
    {
        return isDisposed;
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
                lock (syncLock)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    buffer = null;
                    position = 0;
                    length = 0;
                    capacity = 0;
                }
            }

            isDisposed = true;
        }
    }
}