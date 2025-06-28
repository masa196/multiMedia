using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

        // ✅ Debug
        MessageBox.Show($"✅ WriteBits: عدد البتات = {bits.Length}, عدد البايتات = {bytes.Count}");
    }

    public static string ReadBits(BinaryReader reader)
    {
        int bitCount = reader.ReadInt32();
        byte[] bytes = reader.ReadBytes((bitCount + 7) / 8);

        StringBuilder sb = new StringBuilder(bitCount);
        int totalBits = 0;

        foreach (byte b in bytes)
        {
            for (int j = 7; j >= 0 && totalBits < bitCount; j--)
            {
                bool bit = (b & (1 << j)) != 0;
                sb.Append(bit ? '1' : '0');
                totalBits++;
            }
        }

        // ✅ Debug
        MessageBox.Show($"📥 ReadBits: تم تحميل {bitCount} بت ({bytes.Length} بايت)\nbitString.Length = {sb.Length}");

        return sb.ToString();
    }
}
