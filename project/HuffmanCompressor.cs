using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

public class HuffmanCompressor
{
    private Dictionary<byte, string> codes = new Dictionary<byte, string>();
    private ManualResetEventSlim pauseEvent;

    public HuffmanCompressor(ManualResetEventSlim pauseEvent)
    {
        this.pauseEvent = pauseEvent;
    }

    public void Compress(string inputPath, string outputPath)
    {
        byte[] data = File.ReadAllBytes(inputPath);
        var frequencies = data.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());

        HuffmanNode root = BuildTree(frequencies);
        GenerateCodes(root, "");

        using (var writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
        {
            // 🟢 1. حفظ الامتداد الأصلي
            string extension = Path.GetExtension(inputPath);
            writer.Write(extension);

            // 🟢 2. حفظ جدول التكرارات
            writer.Write(frequencies.Count);
            foreach (var pair in frequencies)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            // 🟢 3. تشفير البيانات
            var encodedBuilder = new System.Text.StringBuilder();
            int count = 0;

            foreach (byte b in data)
            {
                pauseEvent.Wait();

                encodedBuilder.Append(codes[b]);
                count++;

                if (count % 1000 == 0)
                    System.Windows.Forms.Application.DoEvents();
            }

            string encoded = encodedBuilder.ToString();
            BitWriter.WriteBits(writer, encoded);
        }
    }

    public void Decompress(string compressedPath, string outputPath = null)
    {
        using (var reader = new BinaryReader(File.Open(compressedPath, FileMode.Open)))
        {
            // 🟢 1. قراءة الامتداد الأصلي
            string extension = reader.ReadString();

            // 🟢 2. استعادة جدول التكرارات
            int symbolCount = reader.ReadInt32();
            var frequencies = new Dictionary<byte, int>();

            for (int i = 0; i < symbolCount; i++)
            {
                byte symbol = reader.ReadByte();
                int freq = reader.ReadInt32();
                frequencies[symbol] = freq;
            }

            // 🟢 3. بناء الشجرة وفك التشفير
            HuffmanNode root = BuildTree(frequencies);
            string bitString = BitWriter.ReadBits(reader);
            byte[] result = Decode(root, bitString);

            // 🟢 4. حفظ الملف بالامتداد الأصلي
            string actualOutputPath = outputPath ?? Path.ChangeExtension(compressedPath, extension);
            File.WriteAllBytes(actualOutputPath, result);
        }
    }

    private HuffmanNode BuildTree(Dictionary<byte, int> frequencies)
    {
        var queue = new SimplePriorityQueue();

        foreach (var pair in frequencies)
        {
            queue.Enqueue(new HuffmanNode { Symbol = pair.Key, Frequency = pair.Value });
        }

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
        {
            codes[node.Symbol.Value] = code;
        }
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
