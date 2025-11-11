using System;
using System.Text;

namespace TpsParser;

/// <summary>
/// Represents a sequence of bytes hashed from a password string that is used to encrypt and decrypt the file.
/// </summary>
public sealed class Key
{
    private TpsRandomAccess Data { get; }

    /// <summary>
    /// Instantiates a key and initializes it using the given password.
    /// </summary>
    /// <param name="password">The password or "owner" of the file.</param>
    public Key(string password)
    {
        var encoding = CodePagesEncodingProvider.Instance.GetEncoding("Windows-1258");
        var passwordBytes = encoding.GetBytes(password);

        var keyBytes = new byte[passwordBytes.Length + 1];

        Array.Copy(passwordBytes, keyBytes, passwordBytes.Length);

        // Smear out the password over 64 bytes

        var block = new byte[64];

        for (int i = 0; i < block.Length; i++)
        {
            int x = i * 0x11 & 0x3F;
            block[x] = (byte)(i + keyBytes[(i + 1) % keyBytes.Length]);
        }

        Data = new TpsRandomAccess(block, encoding);

        // Two calls required.

        Shuffle();
        Shuffle();
    }

    /// <summary>
    /// Instantiates a key with an already initialized data state.
    /// </summary>
    /// <param name="rx"></param>
    public Key(TpsRandomAccess rx)
    {
        Data = rx ?? throw new ArgumentNullException(nameof(rx));
    }

    /// <summary>
    /// Shuffles the smeared key.  This method must be called twice to properly initialize the key.
    /// </summary>
    public void Shuffle()
    {
        for (int i = 0; i < 0x10; i++)
        {
            int wordA = GetWord(i);
            int positionB = wordA & 0x0F;
            int wordB = GetWord(positionB);

            int opAnd = wordA & wordB;
            int sum1 = wordA + opAnd;

            SetWord(positionB, sum1);

            int opOr = wordA | wordB;
            int sum2 = opOr + wordA;

            SetWord(i, sum2);
        }
    }

    public int GetWord(int word)
    {
        Data.JumpAbsolute(word * 4);
        return Data.ReadLongLE();
    }

    private void SetWord(int word, int value)
    {
        Data.JumpAbsolute(word * 4);
        Data.WriteLongLE(value);
    }

    /// <summary>
    /// Encrypts the given buffer. The buffer must be 64 bytes long.
    /// </summary>
    /// <param name="buffer">The buffer to encrypt.</param>
    public void Encrypt64(TpsRandomAccess buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (buffer.Length != 64)
        {
            throw new ArgumentException($"The given buffer was not 64 bytes ({buffer.Length}).", nameof(buffer));
        }

        for (int i = 0; i < 0x10; i++)
        {
            int positionA = i;
            int keyA = GetWord(i);
            int positionB = keyA & 0x0F;

            int data2 = buffer.JumpAbsolute(positionA * 4).ReadLongLE();
            int data1 = buffer.JumpAbsolute(positionB * 4).ReadLongLE();

            int opAnd1 = keyA & data2;
            int opNotA = ~keyA;
            int opAnd2 = opNotA & data1;
            int opOr1 = opAnd1 | opAnd2;

            int sum1 = keyA + opOr1;
            buffer.JumpAbsolute(positionA * 4).WriteLongLE(sum1);

            int opAnd3 = keyA & data1;
            int opAnd4 = opNotA & data2;
            int opOr2 = opAnd3 | opAnd4;
            int sum2 = opOr2 + keyA;

            buffer.JumpAbsolute(positionB * 4).WriteLongLE(sum2);
        }
    }

    /// <summary>
    /// Decrypts the given buffer. The buffer must be 64 bytes long.
    /// </summary>
    /// <param name="buffer"></param>
    public void Decrypt64(TpsRandomAccess buffer)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (buffer.Length != 64)
        {
            throw new ArgumentException($"The given buffer was not 64 bytes ({buffer.Length}).", nameof(buffer));
        }

        for (int i = 0x0F; i >= 0; i--)
        {
            int positionA = i;
            int keyA = GetWord(positionA);
            int positionB = keyA & 0x0F;

            int data1 = buffer.JumpAbsolute(positionA * 4).ReadLongLE();
            data1 -= keyA;
            int data2 = buffer.JumpAbsolute(positionB * 4).ReadLongLE();
            data2 -= keyA;

            int opAnd1 = data1 & keyA;
            int opNotA = ~keyA;
            int opAnd2 = data2 & opNotA;
            int opOr1 = opAnd1 | opAnd2;
            buffer.JumpAbsolute(positionA * 4).WriteLongLE(opOr1);

            int opAnd3 = data2 & keyA;
            int opAnd4 = data1 & opNotA;
            int opOr2 = opAnd3 | opAnd4;
            buffer.JumpAbsolute(positionB * 4).WriteLongLE(opOr2);
        }
    }

    /// <summary>
    /// Decodes the given encrypted data in blocks of 64 bytes.
    /// The data must have a position of 0, and an offset and length that are divisible by 64.
    /// </summary>
    /// <param name="encrypted">The encrypted data to decrypt.</param>
    public void Decrypt(TpsRandomAccess encrypted)
    {
        if (encrypted == null)
        {
            throw new ArgumentNullException(nameof(encrypted));
        }

        if (encrypted.Position != 0)
        {
            throw new ArgumentException($"The position must start at 0 ({encrypted.Position}).", nameof(encrypted));
        }

        if (encrypted.BaseOffset % 64 != 0)
        {
            throw new ArgumentException($"The offset must be divisible by 64 ({encrypted.BaseOffset})", nameof(encrypted));
        }

        if (encrypted.Length % 64 != 0)
        {
            throw new ArgumentException($"The length must be divisible by 64 ({encrypted.Length}).", nameof(encrypted));
        }

        for (int offset = 0; offset < encrypted.Length / 64; offset++)
        {
            var buffer = new TpsRandomAccess(encrypted, offset * 64, 64);
            Decrypt64(buffer);
        }
    }

    public override string ToString() =>
        Data.ToHexString(step: 64, ascii: false);
}
