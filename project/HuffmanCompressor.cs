using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


public class HuffmanNode
{
    public char? Symbol;
    public int Frequency;
    public HuffmanNode Left;
    public HuffmanNode Right;
}

public class HuffmanCompressor
{
    private Dictionary<char, string> codes = new Dictionary<char, string>();

    public void Compress(string inputPath, string outputPath)
    {
        string text = File.ReadAllText(inputPath);
        var frequencies = text.GroupBy(c => c)
                              .ToDictionary(g => g.Key, g => g.Count());

        HuffmanNode root = BuildTree(frequencies);
        GenerateCodes(root, "");

        using (var writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
        {
            writer.Write(frequencies.Count);
            foreach (var pair in frequencies)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }

            string encoded = string.Concat(text.Select(c => codes[c]));
            BitWriter.WriteBits(writer, encoded);
        }
    }

    public void Decompress(string compressedPath, string outputPath)
    {
        using (var reader = new BinaryReader(File.Open(compressedPath, FileMode.Open)))
        {
            int symbolCount = reader.ReadInt32();
            var frequencies = new Dictionary<char, int>();

            for (int i = 0; i < symbolCount; i++)
            {
                char symbol = reader.ReadChar();
                int freq = reader.ReadInt32();
                frequencies[symbol] = freq;
            }

            HuffmanNode root = BuildTree(frequencies);
            string bitString = BitWriter.ReadBits(reader);
            string result = Decode(root, bitString);
            File.WriteAllText(outputPath, result);
        }
    }

    private HuffmanNode BuildTree(Dictionary<char, int> frequencies)
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

    private string Decode(HuffmanNode root, string bits)
    {
        var result = new List<char>();
        var node = root;

        foreach (char bit in bits)
        {
            node = bit == '0' ? node.Left : node.Right;

            if (node.Symbol != null)
            {
                result.Add(node.Symbol.Value);
                node = root;
            }
        }

        return new string(result.ToArray());
    }
}
