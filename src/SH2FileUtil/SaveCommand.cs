using System.IO.Compression;
using System.Text.Json;

namespace SH2FileUtil;

internal static class SaveCommand
{
    public static void Process(string inputPath, string outputPath)
    {
        if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
        {
            Console.WriteLine("Invalid input path");
            return;
        }

        var inputBuffer = File.ReadAllBytes(inputPath);

        try
        {
            if (IsCompressed(inputBuffer))
            {
                Console.WriteLine($"Decompressing {inputPath}");
                var save = Decompress(inputBuffer);

                if (string.IsNullOrEmpty(outputPath))
                    outputPath = $"{inputPath}.json";

                using var outputStream = File.OpenWrite(outputPath);
                JsonSerializer.Serialize(outputStream, JsonDocument.Parse(save),
                    new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"Decompressed to {outputPath}");
            }
            else
            {
                Console.WriteLine($"Compressing {inputPath}");
                var save = JsonSerializer.SerializeToUtf8Bytes(JsonDocument.Parse(inputBuffer, new JsonDocumentOptions { MaxDepth = 200 }),
                    new JsonSerializerOptions { WriteIndented = false });

                if (string.IsNullOrEmpty(outputPath))
                    outputPath = $"{inputPath}.dat";

                using var outputStream = File.OpenWrite(outputPath);
                outputStream.Write(Compress(save));
                Console.WriteLine($"Compressed to {outputPath}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
    }

    public static bool IsCompressed(byte[] buffer)
    {
        if (buffer.Length < 2) return false;
        return buffer[0] == 0x1f && buffer[1] == 0x8b;
    }

    private static byte[] Decompress(byte[] data)
    {
        using var jsonStream = new MemoryStream();
        using (var compStream = new MemoryStream(data))
        using (var gzStream = new GZipStream(compStream, CompressionMode.Decompress, true))
        {
            gzStream.CopyTo(jsonStream);
        }

        jsonStream.Position = 0;
        return jsonStream.ToArray();
    }

    private static byte[] Compress(byte[] json)
    {
        using var compStream = new MemoryStream();
        using (var jsonStream = new MemoryStream(json))
        using (var gzStream = new GZipStream(compStream, CompressionLevel.Optimal, true))
        {
            jsonStream.WriteTo(gzStream);
        }

        compStream.Position = 0;
        return compStream.ToArray();
    }
}