using System;
using System.Security.Cryptography;

namespace DarkAges.Library.Network;

public class NetworkEncryption : IDisposable
{
    private readonly Aes _aes;
    private bool _isDisposed;

    public NetworkEncryption()
    {
        _aes = Aes.Create();
        _aes.GenerateKey();
        _aes.GenerateIV();
    }

    public byte[] Encrypt(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkEncryption));

        using var encryptor = _aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public byte[] Decrypt(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (_isDisposed)
            throw new ObjectDisposedException(nameof(NetworkEncryption));

        using var decryptor = _aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _aes.Dispose();
        _isDisposed = true;
    }
}