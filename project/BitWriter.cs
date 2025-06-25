using System.Collections.Generic;
using System.IO;
using System.Text;

public static class BitWriter
{
    public static void WriteBits(BinaryWriter writer, string bits)
    {
        List<byte> bytes = new List<byte>();
        int index = 0;
        while (index < bits.Length)
        {
            byte b = 0;
            for (int i = 0; i < 8 && index < bits.Length; i++)
            {
                if (bits[index] == '1')
                    b |= (byte)(1 << (7 - i));
                index++;
            }
            bytes.Add(b);
        }

        writer.Write(bits.Length);
        writer.Write(bytes.ToArray());
    }

    public static string ReadBits(BinaryReader reader)
    {
        int bitCount = reader.ReadInt32();
        byte[] bytes = reader.ReadBytes((bitCount + 7) / 8);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < bytes.Length; i++)
        {
            for (int j = 7; j >= 0; j--)
            {
                bool bit = (bytes[i] & (1 << j)) != 0;
                sb.Append(bit ? '1' : '0');
                if (sb.Length == bitCount)
                    break;
            }
        }

        return sb.ToString();
    }
}
