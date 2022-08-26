using System.Text;

namespace SH2FileUtil;

internal static class CryptCommand
{
    internal enum CryptMode
    {
        AssetDec,
        AssetEnc,
        Ignore,
    }

    private static readonly string _pass = "bEFacjdWNGU=";
    private static readonly string _salt = "X2JLbw==";

    private static readonly string _unityAssetMagic = "UnityFS";

    static CryptCommand()
    {
        _pass = Encoding.UTF8.GetString(Convert.FromBase64String(_pass));
        _salt = Encoding.UTF8.GetString(Convert.FromBase64String(_salt));
    }

    private static bool CheckMagic(Stream stream)
    {
        var streamPos = stream.Position;
        var magic = new byte[_unityAssetMagic.Length];
        stream.Read(magic, 0, magic.Length);
        stream.Position = streamPos;

        return _unityAssetMagic == Encoding.UTF8.GetString(magic);
    }

    public static void Process(string inputPath, string outputPath, CryptMode mode = CryptMode.AssetDec, string key = "")
    {
        inputPath = (inputPath ?? string.Empty).TrimEnd('\"');
        outputPath = (outputPath ?? string.Empty).TrimEnd('\"');

        if (string.IsNullOrEmpty(inputPath))
        {
            Console.WriteLine("Invalid input path");
            return;
        }

        try
        {
            if (Directory.Exists(inputPath))
            {
                ProcessDirectory(inputPath, outputPath, mode);
                return;
            }

            if (File.Exists(inputPath))
            {
                ProcessFile(inputPath, outputPath, mode, key);
                return;
            }

            Console.WriteLine("Invalid input path");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void ProcessFile(string inputPath, string outputPath, CryptMode mode = CryptMode.AssetDec, string key = "")
    {
        using var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.None);

        if (string.IsNullOrEmpty(key))
        {
            key = mode == CryptMode.Ignore ?
                Path.GetFileName(inputPath.Replace(".crypt", "")) :
                Path.GetFileNameWithoutExtension(inputPath.Replace(".crypt", ""));
        }

        if (mode != CryptMode.Ignore && inputStream.Length < _unityAssetMagic.Length)
        {
            Console.WriteLine($"File too small, skipping -- {inputPath}");
            return;
        }

        if (mode == CryptMode.AssetEnc && !CheckMagic(inputStream))
        {
            Console.WriteLine($"File is not an unencrypted asset, skipping -- {inputPath}");
            return;
        }

        var salt = Encoding.UTF8.GetBytes(key + _salt);
        using var aesStream = new SeekableAesStream(inputStream, _pass, salt);

        if (mode == CryptMode.AssetDec && !CheckMagic(aesStream))
        {
            Console.WriteLine($"File is not an encrypted asset, skipping -- {inputPath}");
            return;
        }

        var dataBuffer = new byte[aesStream.Length];
        aesStream.Read(dataBuffer, 0, dataBuffer.Length);

        if (string.IsNullOrEmpty(outputPath))
            outputPath = $"{inputPath}.crypt";

        using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        outputStream.Write(dataBuffer);

        Console.WriteLine($"Crypted asset -- {inputPath} to {outputPath}");
    }

    private static void ProcessDirectory(string inputPath, string outputPath, CryptMode mode = CryptMode.AssetDec)
    {
        var inputInfo = new DirectoryInfo(inputPath);

        if (string.IsNullOrEmpty(outputPath))
        {
            var parent = inputInfo.Parent?.FullName;
            var name = inputInfo.Name;
            outputPath = Path.Combine(parent ?? ".", $"{name}_crypt");
        }

        Directory.CreateDirectory(outputPath);

        foreach (var file in inputInfo.EnumerateFiles("*.*", SearchOption.AllDirectories))
        {
            var relPath = Path.GetRelativePath(inputPath, file.FullName);
            var relOutPath = Path.Combine(outputPath, relPath);

            ProcessFile(file.FullName, relOutPath, mode);
        }
    }
}