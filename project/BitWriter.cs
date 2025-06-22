using System.IO;
using System.Text;

class BitWriter
{
    public static void WriteBits(BinaryWriter writer, string bits)
    {
        byte buffer = 0;
        int bitCount = 0;

        foreach (char bit in bits)
        {
            if (bit == '1')
                buffer |= (byte)(1 << (7 - bitCount));
            bitCount++;

            if (bitCount == 8)
            {
                writer.Write(buffer);
                buffer = 0;
                bitCount = 0;
            }
        }

        if (bitCount > 0)
            writer.Write(buffer);
    }

    public static string ReadBits(BinaryReader reader)
    {
        var sb = new StringBuilder();
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte b = reader.ReadByte();
            for (int i = 7; i >= 0; i--)
            {
                sb.Append(((b >> i) & 1) == 1 ? '1' : '0');
            }
        }
        return sb.ToString();
    }
}
