using System;  
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;  // <<< هذي ضرورية للتشفير
using System.Text;
using System.Threading;
using System.Windows.Forms;  // <<< ضروري ل MessageBox وأدوات Windows Forms


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

    public double CompressSingleFile(string inputPath, string outputPath)
    {
            // كلمة السر فارغة، هذا يعني عدم التشفير
            return CompressSingleFile(inputPath, outputPath, "");
    }

    public double CompressSingleFile(string inputPath, string outputPath, string password)
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
            // كتابة كلمة السر أولاً
            writer.Write(password ?? "");

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
            File.Delete(outputPath);
        return 0;
    }

    long originalSize = new FileInfo(inputPath).Length;
    long compressedSize = new FileInfo(outputPath).Length;
    return 100.0 * (1 - (compressedSize / (double)originalSize));
}

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

   public bool DecompressSingleFile(string compressedPath, string outputPath = null)
{
    string password = PasswordDialog.ShowDialog("الرجاء إدخال كلمة السر لفك الضغط:");

    if (password == null)
    {
        MessageBox.Show("تم إلغاء فك الضغط بسبب عدم إدخال كلمة السر.");
        return false;
    }

    using (var reader = new BinaryReader(File.Open(compressedPath, FileMode.Open)))
    {
        // نقرأ كلمة السر المخزنة في الملف أولاً
        string storedPassword = reader.ReadString();

        if (storedPassword != password)
        {
            MessageBox.Show("❌ كلمة السر غير صحيحة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

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
            return false;

        byte[] result = Decode(root, bitString);

        if (isCancelledFunc != null && isCancelledFunc())
            return false;

        string actualOutputPath = outputPath ?? Path.ChangeExtension(compressedPath, extension);
        File.WriteAllBytes(actualOutputPath, result);
    }

    return true;
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
    public double CompressFolder(string folderPath, string outputPath)
    {
        // 1. الحصول على كل الملفات مع المسار النسبي داخل المجلد
        var allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

        // قائمة لتخزين معلومات الملفات (الاسم النسبي والحجم)
        var fileMeta = new List<(string relativePath, int size)>();

        // قائمة لتجميع كل البايتات للضغط
        var allBytes = new List<byte>();

        foreach (var file in allFiles)
        {
            // المسار النسبي داخل المجلد
            string relativePath = GetRelativePath(folderPath, file);

            byte[] content = File.ReadAllBytes(file);
            fileMeta.Add((relativePath, content.Length));
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
                // نكتب عدد الملفات
                writer.Write(fileMeta.Count);

                // نكتب اسم كل ملف مع حجمه
                foreach (var (relativePath, size) in fileMeta)
                {
                    writer.Write(relativePath);
                    writer.Write(size);
                }

                // نكتب جدول الترددات
                writer.Write(frequencies.Count);
                foreach (var pair in frequencies)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }

                // نضغط البيانات
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
                File.Delete(outputPath);
            return 0;
        }

        long originalSize = data.Length;
        long compressedSize = new FileInfo(outputPath).Length;
        return 100.0 * (1 - (compressedSize / (double)originalSize));
    }
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
            var fileMeta = new List<(string relativePath, int size)>();
            for (int i = 0; i < fileCount; i++)
            {
                string relativePath = reader.ReadString();
                int size = reader.ReadInt32();
                fileMeta.Add((relativePath, size));
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
            foreach (var (relativePath, size) in fileMeta)
            {
                if (isCancelledFunc != null && isCancelledFunc())
                    break;

                byte[] fileData = allData.Skip(offset).Take(size).ToArray();
                string fullPath = Path.Combine(outputFolder, relativePath);

                // تأكد من وجود المجلد الذي يحتوي الملف
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

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
    public static string GetRelativePath(string basePath, string fullPath)
    {
        Uri baseUri = new Uri(AppendDirectorySeparatorChar(basePath));
        Uri fullUri = new Uri(fullPath);
        Uri relativeUri = baseUri.MakeRelativeUri(fullUri);
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    private static string AppendDirectorySeparatorChar(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return path + Path.DirectorySeparatorChar;
        return path;
    }
 
}

public class HuffmanNode
{
    public byte? Symbol;
    public int Frequency;
    public HuffmanNode Left;
    public HuffmanNode Right;
} 