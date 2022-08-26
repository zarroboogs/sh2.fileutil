using System.Security.Cryptography;

namespace SH2FileUtil;

internal class SeekableAesStream : Stream
{
    private readonly Stream baseStream;
    private readonly Aes aes;
    private readonly ICryptoTransform cryptor;
    
    public bool AutoDisposeBaseStream { get; set; } = true;

    public SeekableAesStream(Stream baseStream, string password, byte[] salt)
    {
        this.baseStream = baseStream;
        
        using var key = new PasswordDeriveBytes(password, salt);
        
        aes = Aes.Create();
        aes.KeySize = 128;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = new byte[16];
        
        cryptor = aes.CreateEncryptor(aes.Key, aes.IV);
    }

    private void Cipher(byte[] buffer, int offset, int count, long streamPos)
    {
        var blockSizeInByte = aes.BlockSize / 8;
        var blockNumber = (streamPos / blockSizeInByte) + 1;
        var keyPos = streamPos % blockSizeInByte;

        var outBuffer = new byte[blockSizeInByte];
        var nonce = new byte[blockSizeInByte];
        var init = false;

        for (int i = offset; i < count; i++)
        {
            if (!init || (keyPos % blockSizeInByte) == 0)
            {
                BitConverter.GetBytes(blockNumber).CopyTo(nonce, 0);
                cryptor.TransformBlock(nonce, 0, nonce.Length, outBuffer, 0);
                if (init) keyPos = 0;
                init = true;
                blockNumber++;
            }

            buffer[i] ^= outBuffer[keyPos]; 
            keyPos++;
        }
    }

    public override bool CanRead => baseStream.CanRead;

    public override bool CanSeek => baseStream.CanSeek;

    public override bool CanWrite => baseStream.CanWrite;

    public override long Length => baseStream.Length;

    public override long Position
    {
        get => baseStream.Position;
        set => baseStream.Position = value;
    }

    public override void Flush() => baseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var streamPos = Position;
        var ret = baseStream.Read(buffer, offset, count);
        Cipher(buffer, offset, count, streamPos);
        return ret;
    }

    public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);

    public override void SetLength(long value) => baseStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        Cipher(buffer, offset, count, Position);
        baseStream.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cryptor?.Dispose();
            aes?.Dispose();

            if (AutoDisposeBaseStream)
                baseStream?.Dispose();
        }

        base.Dispose(disposing);
    }
}
