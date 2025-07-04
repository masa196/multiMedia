﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class ShannonFanoCompressor : ICompressor
{
    private Dictionary<byte, string> codes = new Dictionary<byte, string>();
    private ManualResetEventSlim pauseEvent;
    private Func<bool> isCancelledFunc;

    public ShannonFanoCompressor(ManualResetEventSlim pauseEvent, Func<bool> isCancelledFunc = null)
    {
        this.pauseEvent = pauseEvent;
        this.isCancelledFunc = isCancelledFunc;
    }
    public double CompressSingleFile(string inputPath, string outputPath)
    {
        // إذا لم يتم تمرير كلمة سر، نمرر نص فارغ تلقائيًا
        return CompressSingleFile(inputPath, outputPath, "");
    }

  public double CompressSingleFile(string inputPath, string outputPath, string password)
{
    byte[] data = File.ReadAllBytes(inputPath);
    var frequencies = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());

    codes.Clear();
    BuildCode(frequencies.OrderByDescending(p => p.Value).ToList(), "");

    try
    {
        using (var stream = File.Open(outputPath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
            // اكتب كلمة السر أولاً
            writer.Write(password ?? "");

            // ثم باقي البيانات
            writer.Write(Path.GetFileName(inputPath));

            writer.Write(frequencies.Count);
            foreach (var pair in frequencies)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            writer.Write(codes.Count);
            foreach (var pair in codes)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            var encodedBuilder = new StringBuilder();
            int count = 0;
            foreach (byte b in data)
            {
                pauseEvent.Wait();
                if (isCancelledFunc?.Invoke() == true)
                    throw new OperationCanceledException();

                encodedBuilder.Append(codes[b]);

                if (++count % 1000 == 0)
                    Application.DoEvents();
            }

            if (isCancelledFunc?.Invoke() == true)
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



 public bool DecompressSingleFile(string compressedPath, string outputPath = null)
{
    string password = PasswordDialog.ShowDialog("الرجاء إدخال كلمة السر لفك الضغط:");

    if (password == null)
    {
        MessageBox.Show("تم إلغاء فك الضغط بسبب عدم إدخال كلمة السر.");
        return false;
    }

    try
    {
        using (var reader = new BinaryReader(File.Open(compressedPath, FileMode.Open)))
        {
            // اقرأ كلمة السر المخزنة
            string storedPassword = reader.ReadString();


            if (storedPassword != password)
            {
                MessageBox.Show("❌ كلمة السر غير صحيحة.");
                return false;
            }

            string originalName = reader.ReadString();

            int symbolCount = reader.ReadInt32();
            var frequencies = new Dictionary<byte, int>();
            for (int i = 0; i < symbolCount; i++)
            {
                byte symbol = reader.ReadByte();
                int freq = reader.ReadInt32();
                frequencies[symbol] = freq;
            }

            int codeCount = reader.ReadInt32();
            codes.Clear();
            for (int i = 0; i < codeCount; i++)
            {
                byte symbol = reader.ReadByte();
                string code = reader.ReadString();
                codes[symbol] = code;
            }

            string bitString = BitWriter.ReadBits(reader);
            if (isCancelledFunc?.Invoke() == true)
                return false;

            byte[] result = Decode(bitString);
            if (isCancelledFunc?.Invoke() == true)
                return false;

            string baseDir = Path.GetDirectoryName(compressedPath);
            string baseName = Path.GetFileNameWithoutExtension(originalName);
            string actualOutputPath = outputPath ?? Path.Combine(baseDir, baseName + "_extracted" + Path.GetExtension(originalName));

            File.WriteAllBytes(actualOutputPath, result);
            MessageBox.Show($"✅ تم فك الضغط:\n{actualOutputPath}", "نجاح");

            return true;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"❌ حدث خطأ أثناء فك الضغط:\n{ex.Message}", "خطأ");
        return false;
    }
}




    // 👇 بإمكانك ترك Archive methods كما هي إذا أردت نفس التحسينات هناك أيضاً
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
        codes.Clear();
        BuildCode(frequencies.OrderByDescending(p => p.Value).ToList(), "");

        try
        {
            using (var writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
            {
                // 🗂️ معلومات الملفات
                writer.Write(fileMeta.Count);
                foreach (var (name, size) in fileMeta)
                {
                    writer.Write(name);
                    writer.Write(size);
                }

                // 🔢 التكرارات
                writer.Write(frequencies.Count);
                foreach (var pair in frequencies)
                {
                    writer.Write(pair.Key);
                    writer.Write(pair.Value);
                }

                // 🧠 كتابة البتات المشفرة
                var encodedBuilder = new StringBuilder();
                int count = 0;
                foreach (byte b in data)
                {
                    pauseEvent.Wait();
                    if (isCancelledFunc?.Invoke() == true)
                        throw new OperationCanceledException();

                    encodedBuilder.Append(codes[b]);
                    if (++count % 1000 == 0)
                        Application.DoEvents();
                }

                if (isCancelledFunc?.Invoke() == true)
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
            Path.GetFileNameWithoutExtension(archivePath) + "_Extracted");

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

            int freqCount = reader.ReadInt32();
            var frequencies = new Dictionary<byte, int>();
            for (int i = 0; i < freqCount; i++)
            {
                byte symbol = reader.ReadByte();
                int freq = reader.ReadInt32();
                frequencies[symbol] = freq;
            }

            codes.Clear();
            BuildCode(frequencies.OrderByDescending(p => p.Value).ToList(), "");

            string bitString = BitWriter.ReadBits(reader);
            byte[] allData = Decode(bitString);

            int offset = 0;
            foreach (var (name, size) in fileMeta)
            {
                if (isCancelledFunc?.Invoke() == true)
                    break;

                byte[] fileData = allData.Skip(offset).Take(size).ToArray();
                string fullPath = Path.Combine(outputFolder, name);
                File.WriteAllBytes(fullPath, fileData);
                offset += size;
            }

            if (isCancelledFunc?.Invoke() == true)
            {
                try { Directory.Delete(outputFolder, true); } catch { }
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

            int freqCount = reader.ReadInt32();
            var frequencies = new Dictionary<byte, int>();
            for (int i = 0; i < freqCount; i++)
            {
                byte symbol = reader.ReadByte();
                int freq = reader.ReadInt32();
                frequencies[symbol] = freq;
            }

            codes.Clear();
            BuildCode(frequencies.OrderByDescending(p => p.Value).ToList(), "");

            string bitString = BitWriter.ReadBits(reader);
            byte[] allData = Decode(bitString);

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

    private void BuildCode(List<KeyValuePair<byte, int>> symbols, string prefix)
    {
        if (symbols.Count == 1)
        {
            codes[symbols[0].Key] = string.IsNullOrEmpty(prefix) ? "0" : prefix;
            return;
        }

        int total = symbols.Sum(p => p.Value);
        int sum = 0, split = 0;
        for (; split < symbols.Count; split++)
        {
            sum += symbols[split].Value;
            if (sum >= total / 2)
                break;
        }

        BuildCode(symbols.Take(split + 1).ToList(), prefix + "0");
        BuildCode(symbols.Skip(split + 1).ToList(), prefix + "1");
    }

    private byte[] Decode(string bits)
    {
        var reverseCodes = codes.ToDictionary(kv => kv.Value, kv => kv.Key);
        var result = new List<byte>();
        var current = new StringBuilder();

        int count = 0;
        foreach (char bit in bits)
        {
            pauseEvent.Wait();
            current.Append(bit);

            if (reverseCodes.TryGetValue(current.ToString(), out byte symbol))
            {
                result.Add(symbol);
                current.Clear();
            }

            if (++count % 1000 == 0)
                Application.DoEvents();
        }

        return result.ToArray();
    }
    public double CompressFolder(string folderPath, string outputPath)
    {
        // نفس فكرة CompressMultipleFiles لكن تضيف دعم المسارات النسبية للملفات داخل المجلد (subfolders)
        var allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

        var fileMeta = new List<(string relativePath, int size)>();
        var allBytes = new List<byte>();

        foreach (var file in allFiles)
        {
            string relativePath = GetRelativePath(folderPath, file);
            byte[] content = File.ReadAllBytes(file);
            fileMeta.Add((relativePath, content.Length));
            allBytes.AddRange(content);
        }

        var data = allBytes.ToArray();
        var frequencies = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());

        codes.Clear();
        BuildCode(frequencies.OrderByDescending(p => p.Value).ToList(), "");

        try
        {
            using (var writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
            {
                writer.Write(fileMeta.Count);
                foreach (var (relativePath, size) in fileMeta)
                {
                    writer.Write(relativePath);
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
                    if (isCancelledFunc?.Invoke() == true)
                        throw new OperationCanceledException();

                    encodedBuilder.Append(codes[b]);

                    if (++count % 1000 == 0)
                        Application.DoEvents();
                }

                if (isCancelledFunc?.Invoke() == true)
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

    // تابع مساعدة: مسار نسبي للملفات داخل المجلد
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