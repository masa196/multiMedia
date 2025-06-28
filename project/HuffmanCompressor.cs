using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public class HuffmanCompressor : ICompressor
{
    private Dictionary<byte, string> codes = new Dictionary<byte, string>();
    private ManualResetEventSlim pauseEvent;
    private Func<bool> isCancelledFunc;

    public HuffmanCompressor(ManualResetEventSlim pauseEvent, Func<bool> isCancelledFunc = null)
    {
        this.pauseEvent = pauseEvent;
        this.isCancelledFunc = isCancelledFunc;
    }

    // ✅ ضغط ملف واحد فقط (القديم)
    public double CompressSingleFile(string inputPath, string outputPath)
{
    byte[] data = File.ReadAllBytes(inputPath);
    var frequencies = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());

    HuffmanNode root = BuildTree(frequencies);
    GenerateCodes(root, "");

    try
    {
        using (var stream = File.Open(outputPath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            string extension = Path.GetExtension(inputPath);
            writer.Write(extension);
            writer.Write(frequencies.Count);

            foreach (var pair in frequencies)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            var encodedBuilder = new StringBuilder();
            int count = 0;
            foreach (byte b in data)
            {
                pauseEvent.Wait();
                if (isCancelledFunc != null && isCancelledFunc())
                    throw new OperationCanceledException();  // ✅ نرمي استثناء لإلغاء

                encodedBuilder.Append(codes[b]);
                count++;
                if (count % 1000 == 0)
                    System.Windows.Forms.Application.DoEvents();
            }

            // ✅ تحقق من الإلغاء قبل الكتابة النهائية
            if (isCancelledFunc != null && isCancelledFunc())
                throw new OperationCanceledException();

            BitWriter.WriteBits(writer, encodedBuilder.ToString());
        }
    }
    catch (OperationCanceledException)
    {
        if (File.Exists(outputPath))
            File.Delete(outputPath); // ✅ حذف الملف الناتج
        return 0;
    }

    long originalSize = new FileInfo(inputPath).Length;
    long compressedSize = new FileInfo(outputPath).Length;
    return 100.0 * (1 - (compressedSize / (double)originalSize));
}


    // ✅ ضغط عدة ملفات في ملف واحد
    public double CompressMultipleFiles(string[] inputFiles, string outputPath)
{
    var allBytes = new List<byte>();
    var fileMeta = new List<(string name, int size)>();

    foreach (string file in inputFiles)
    {
        byte[] content = File.ReadAllBytes(file);
        fileMeta.Add((Path.GetFileName(file), content.Length));
        allBytes.AddRange(content);
    }

    var data = allBytes.ToArray();
    var frequencies = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
    HuffmanNode root = BuildTree(frequencies);
    GenerateCodes(root, "");

    try
    {
        using (var stream = File.Open(outputPath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(fileMeta.Count);
            foreach (var (name, size) in fileMeta)
            {
                writer.Write(name);
                writer.Write(size);
            }

            writer.Write(frequencies.Count);
            foreach (var pair in frequencies)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            var encodedBuilder = new StringBuilder();
            int count = 0;
            foreach (byte b in data)
            {
                pauseEvent.Wait();

                if (isCancelledFunc != null && isCancelledFunc())
                    throw new OperationCanceledException();

                encodedBuilder.Append(codes[b]);
                count++;
                if (count % 1000 == 0)
                    System.Windows.Forms.Application.DoEvents();
            }

            if (isCancelledFunc != null && isCancelledFunc())
                throw new OperationCanceledException();

            BitWriter.WriteBits(writer, encodedBuilder.ToString());
        }
    }
    catch (OperationCanceledException)
    {
        if (File.Exists(outputPath))
            File.Delete(outputPath); // حذف الأرشيف إذا أُلغي
        return 0;
    }

    long originalSize = data.Length;
    long compressedSize = new FileInfo(outputPath).Length;
    return 100.0 * (1 - (compressedSize / (double)originalSize));
}

    // ✅ فك ملف واحد (القديم)
    public void DecompressSingleFile(string compressedPath, string outputPath = null)
{
    using (var reader = new BinaryReader(File.Open(compressedPath, FileMode.Open)))
    {
        string extension = reader.ReadString();

        int symbolCount = reader.ReadInt32();
        var frequencies = new Dictionary<byte, int>();
        for (int i = 0; i < symbolCount; i++)
        {
            byte symbol = reader.ReadByte();
            int freq = reader.ReadInt32();
            frequencies[symbol] = freq;
        }

        HuffmanNode root = BuildTree(frequencies);
        string bitString = BitWriter.ReadBits(reader);

        if (isCancelledFunc != null && isCancelledFunc())
            return;

        byte[] result = Decode(root, bitString);

        if (isCancelledFunc != null && isCancelledFunc())
            return;

        string actualOutputPath = outputPath ?? Path.ChangeExtension(compressedPath, extension);
        File.WriteAllBytes(actualOutputPath, result);
    }
}

    // ✅ فك ملف الأرشيف المتعدد
    public void DecompressArchive(string archivePath, string outputFolder = null)
{
    outputFolder = outputFolder ?? Path.Combine(
        Path.GetDirectoryName(archivePath),
        Path.GetFileNameWithoutExtension(archivePath) + "_Extracted"
    );

    if (!Directory.Exists(outputFolder))
        Directory.CreateDirectory(outputFolder);

    using (var reader = new BinaryReader(File.Open(archivePath, FileMode.Open)))
    {
        int fileCount = reader.ReadInt32();
        var fileMeta = new List<(string name, int size)>();
        for (int i = 0; i < fileCount; i++)
        {
            string name = reader.ReadString();
            int size = reader.ReadInt32();
            fileMeta.Add((name, size));
        }

        int symbolCount = reader.ReadInt32();
        var frequencies = new Dictionary<byte, int>();
        for (int i = 0; i < symbolCount; i++)
        {
            byte symbol = reader.ReadByte();
            int freq = reader.ReadInt32();
            frequencies[symbol] = freq;
        }

        HuffmanNode root = BuildTree(frequencies);
        string bitString = BitWriter.ReadBits(reader);

        if (isCancelledFunc != null && isCancelledFunc())
            return;

        byte[] allData = Decode(root, bitString);

        int offset = 0;
        foreach (var (name, size) in fileMeta)
        {
            if (isCancelledFunc != null && isCancelledFunc())
                break;

            byte[] fileData = allData.Skip(offset).Take(size).ToArray();
            string fullPath = Path.Combine(outputFolder, name);
            File.WriteAllBytes(fullPath, fileData);
            offset += size;
        }

        // في حال الإلغاء بعد بدء الكتابة، حذف الملفات الناتجة
        if (isCancelledFunc != null && isCancelledFunc())
        {
            try
            {
                Directory.Delete(outputFolder, true);
            }
            catch { }
        }
    }
}

    public void ExtractSingleFileFromArchive(string archivePath, string fileNameToExtract, string savePath)
    {
    using (var reader = new BinaryReader(File.Open(archivePath, FileMode.Open)))
    {
        int fileCount = reader.ReadInt32();
        var fileMeta = new List<(string name, int size)>();
        for (int i = 0; i < fileCount; i++)
        {
            string name = reader.ReadString();
            int size = reader.ReadInt32();
            fileMeta.Add((name, size));
        }

        int symbolCount = reader.ReadInt32();
        var frequencies = new Dictionary<byte, int>();
        for (int i = 0; i < symbolCount; i++)
        {
            byte symbol = reader.ReadByte();
            int freq = reader.ReadInt32();
            frequencies[symbol] = freq;
        }

        HuffmanNode root = BuildTree(frequencies);
        string bitString = BitWriter.ReadBits(reader);
        byte[] allData = Decode(root, bitString);

        int offset = 0;
        foreach (var (name, size) in fileMeta)
        {
            if (name == fileNameToExtract)
            {
                byte[] fileData = allData.Skip(offset).Take(size).ToArray();
                File.WriteAllBytes(savePath, fileData);
                break;
            }
            offset += size;
        }
    }
    }

    public List<string> GetFileListFromArchive(string archivePath)
    {
        var result = new List<string>();
        using (var reader = new BinaryReader(File.Open(archivePath, FileMode.Open)))
        {
            int fileCount = reader.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                string name = reader.ReadString();
                int size = reader.ReadInt32();
                result.Add(name);
            }
        }
        return result;
    }

    private HuffmanNode BuildTree(Dictionary<byte, int> frequencies)
    {
        var queue = new SimplePriorityQueue();
        foreach (var pair in frequencies)
            queue.Enqueue(new HuffmanNode { Symbol = pair.Key, Frequency = pair.Value });

        while (queue.Count() > 1)
        {
            HuffmanNode left = queue.Dequeue();
            HuffmanNode right = queue.Dequeue();
            var parent = new HuffmanNode
            {
                Frequency = left.Frequency + right.Frequency,
                Left = left,
                Right = right
            };
            queue.Enqueue(parent);
        }

        return queue.Dequeue();
    }

    private void GenerateCodes(HuffmanNode node, string code)
    {
        if (node.Symbol != null)
            codes[node.Symbol.Value] = code;
        else
        {
            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }
    }

    private byte[] Decode(HuffmanNode root, string bits)
    {
        var result = new List<byte>();
        var node = root;
        int count = 0;
        foreach (char bit in bits)
        {
            pauseEvent.Wait();
            node = bit == '0' ? node.Left : node.Right;
            if (node.Symbol != null)
            {
                result.Add(node.Symbol.Value);
                node = root;
            }
            count++;
            if (count % 1000 == 0)
                System.Windows.Forms.Application.DoEvents();
        }
        return result.ToArray();
    }
}

public class HuffmanNode
{
    public byte? Symbol;
    public int Frequency;
    public HuffmanNode Left;
    public HuffmanNode Right;
}