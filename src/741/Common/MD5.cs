using System.Text;

namespace DarkAges.Library.Common;

public static class MD5
{
    private static readonly uint[] T =
    [
        0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee,
        0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
        0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be,
        0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821,
        0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa,
        0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
        0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed,
        0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a,
        0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c,
        0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
        0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05,
        0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665,
        0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039,
        0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
        0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1,
        0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
    ];

    private static uint F(uint x, uint y, uint z) => (x & y) | (~x & z);
    private static uint G(uint x, uint y, uint z) => (x & z) | (y & ~z);
    private static uint H(uint x, uint y, uint z) => x ^ y ^ z;
    private static uint I(uint x, uint y, uint z) => y ^ (x | ~z);

    private static uint RotateLeft(uint value, int shift)
    {
        return (value << shift) | (value >> (32 - shift));
    }
        
    private static void Transform(uint[] state, byte[] block)
    {
        uint a = state[0], b = state[1], c = state[2], d = state[3];
        var x = new uint[16];

        for (var i = 0; i < 16; i++)
        {
            x[i] = (uint)(block[i * 4] | block[i * 4 + 1] << 8 | block[i * 4 + 2] << 16 | block[i * 4 + 3] << 24);
        }

        // Round 1
        a = b + RotateLeft(a + F(b, c, d) + x[0] + T[0], 7);
        d = a + RotateLeft(d + F(a, b, c) + x[1] + T[1], 12);
        c = d + RotateLeft(c + F(d, a, b) + x[2] + T[2], 17);
        b = c + RotateLeft(b + F(c, d, a) + x[3] + T[3], 22);
        a = b + RotateLeft(a + F(b, c, d) + x[4] + T[4], 7);
        d = a + RotateLeft(d + F(a, b, c) + x[5] + T[5], 12);
        c = d + RotateLeft(c + F(d, a, b) + x[6] + T[6], 17);
        b = c + RotateLeft(b + F(c, d, a) + x[7] + T[7], 22);
        a = b + RotateLeft(a + F(b, c, d) + x[8] + T[8], 7);
        d = a + RotateLeft(d + F(a, b, c) + x[9] + T[9], 12);
        c = d + RotateLeft(c + F(d, a, b) + x[10] + T[10], 17);
        b = c + RotateLeft(b + F(c, d, a) + x[11] + T[11], 22);
        a = b + RotateLeft(a + F(b, c, d) + x[12] + T[12], 7);
        d = a + RotateLeft(d + F(a, b, c) + x[13] + T[13], 12);
        c = d + RotateLeft(c + F(d, a, b) + x[14] + T[14], 17);
        b = c + RotateLeft(b + F(c, d, a) + x[15] + T[15], 22);

        // Round 2
        a = b + RotateLeft(a + G(b, c, d) + x[1] + T[16], 5);
        d = a + RotateLeft(d + G(a, b, c) + x[6] + T[17], 9);
        c = d + RotateLeft(c + G(d, a, b) + x[11] + T[18], 14);
        b = c + RotateLeft(b + G(c, d, a) + x[0] + T[19], 20);
        a = b + RotateLeft(a + G(b, c, d) + x[5] + T[20], 5);
        d = a + RotateLeft(d + G(a, b, c) + x[10] + T[21], 9);
        c = d + RotateLeft(c + G(d, a, b) + x[15] + T[22], 14);
        b = c + RotateLeft(b + G(c, d, a) + x[4] + T[23], 20);
        a = b + RotateLeft(a + G(b, c, d) + x[9] + T[24], 5);
        d = a + RotateLeft(d + G(a, b, c) + x[14] + T[25], 9);
        c = d + RotateLeft(c + G(d, a, b) + x[3] + T[26], 14);
        b = c + RotateLeft(b + G(c, d, a) + x[8] + T[27], 20);
        a = b + RotateLeft(a + G(b, c, d) + x[13] + T[28], 5);
        d = a + RotateLeft(d + G(a, b, c) + x[2] + T[29], 9);
        c = d + RotateLeft(c + G(d, a, b) + x[7] + T[30], 14);
        b = c + RotateLeft(b + G(c, d, a) + x[12] + T[31], 20);

        // Round 3
        a = b + RotateLeft(a + H(b, c, d) + x[5] + T[32], 4);
        d = a + RotateLeft(d + H(a, b, c) + x[8] + T[33], 11);
        c = d + RotateLeft(c + H(d, a, b) + x[11] + T[34], 16);
        b = c + RotateLeft(b + H(c, d, a) + x[14] + T[35], 23);
        a = b + RotateLeft(a + H(b, c, d) + x[1] + T[36], 4);
        d = a + RotateLeft(d + H(a, b, c) + x[4] + T[37], 11);
        c = d + RotateLeft(c + H(d, a, b) + x[7] + T[38], 16);
        b = c + RotateLeft(b + H(c, d, a) + x[10] + T[39], 23);
        a = b + RotateLeft(a + H(b, c, d) + x[13] + T[40], 4);
        d = a + RotateLeft(d + H(a, b, c) + x[0] + T[41], 11);
        c = d + RotateLeft(c + H(d, a, b) + x[3] + T[42], 16);
        b = c + RotateLeft(b + H(c, d, a) + x[6] + T[43], 23);
        a = b + RotateLeft(a + H(b, c, d) + x[9] + T[44], 4);
        d = a + RotateLeft(d + H(a, b, c) + x[12] + T[45], 11);
        c = d + RotateLeft(c + H(d, a, b) + x[15] + T[46], 16);
        b = c + RotateLeft(b + H(c, d, a) + x[2] + T[47], 23);

        // Round 4
        a = b + RotateLeft(a + I(b, c, d) + x[0] + T[48], 6);
        d = a + RotateLeft(d + I(a, b, c) + x[7] + T[49], 10);
        c = d + RotateLeft(c + I(d, a, b) + x[14] + T[50], 15);
        b = c + RotateLeft(b + I(c, d, a) + x[5] + T[51], 21);
        a = b + RotateLeft(a + I(b, c, d) + x[12] + T[52], 6);
        d = a + RotateLeft(d + I(a, b, c) + x[3] + T[53], 10);
        c = d + RotateLeft(c + I(d, a, b) + x[10] + T[54], 15);
        b = c + RotateLeft(b + I(c, d, a) + x[1] + T[55], 21);
        a = b + RotateLeft(a + I(b, c, d) + x[8] + T[56], 6);
        d = a + RotateLeft(d + I(a, b, c) + x[15] + T[57], 10);
        c = d + RotateLeft(c + I(d, a, b) + x[6] + T[58], 15);
        b = c + RotateLeft(b + I(c, d, a) + x[13] + T[59], 21);
        a = b + RotateLeft(a + I(b, c, d) + x[4] + T[60], 6);
        d = a + RotateLeft(d + I(a, b, c) + x[11] + T[61], 10);
        c = d + RotateLeft(c + I(d, a, b) + x[2] + T[62], 15);
        b = c + RotateLeft(b + I(c, d, a) + x[9] + T[63], 21);

        state[0] += a;
        state[1] += b;
        state[2] += c;
        state[3] += d;
    }

    public static string ComputeHash(string input)
    {
        var bytes = Encoding.ASCII.GetBytes(input);
        return ComputeHash(bytes);
    }

    public static string ComputeHash(byte[] input)
    {
        uint[] state = [0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476];
        var bitCount = (ulong)input.Length * 8;

        var paddingSize = 56 - (input.Length % 64);
        if (paddingSize <= 0)
            paddingSize += 64;

        var padding = new byte[paddingSize];
        padding[0] = 0x80;
            
        var lengthBytes = System.BitConverter.GetBytes(bitCount);
            
        var totalLength = input.Length + paddingSize + 8;
        var message = new byte[totalLength];
            
        System.Buffer.BlockCopy(input, 0, message, 0, input.Length);
        System.Buffer.BlockCopy(padding, 0, message, input.Length, paddingSize);
        System.Buffer.BlockCopy(lengthBytes, 0, message, input.Length + paddingSize, 8);
            
        for (var i = 0; i < message.Length; i += 64)
        {
            var block = new byte[64];
            System.Buffer.BlockCopy(message, i, block, 0, 64);
            Transform(state, block);
        }

        var sb = new StringBuilder();
        for (var i = 0; i < 4; i++)
        {
            sb.Append(state[i].ToString("x2"));
        }
        return sb.ToString();
    }
}