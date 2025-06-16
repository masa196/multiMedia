using System;
using System.IO;
using System.Windows.Forms;

namespace project
{
    public partial class Form1 : Form
    {
        private string selectedFile = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFile = ofd.FileName;
                txtFilePath.Text = selectedFile;
            }
        }

        private void btnCompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("يرجى اختيار ملف أولاً.");
                return;
            }

            string outputPath = Path.ChangeExtension(selectedFile, ".huff");

            // حساب الحجم الأصلي قبل الضغط
            long originalSize = new FileInfo(selectedFile).Length;

            // تنفيذ الضغط
            HuffmanCompressor h = new HuffmanCompressor();
            h.Compress(selectedFile, outputPath);

            // حساب الحجم بعد الضغط
            long compressedSize = new FileInfo(outputPath).Length;

            // حساب نسبة الضغط
            string ratioText = compressedSize > 0
                ? (originalSize / (double)compressedSize).ToString("0.00") + "x"
                : "خطأ في الضغط";

            // عرض المعلومات
            MessageBox.Show(
                $"تم ضغط الملف بنجاح:\n\n" +
                $"📄 الحجم الأصلي: {originalSize} بايت\n" +
                $"📦 الحجم بعد الضغط: {compressedSize} بايت\n" +
                $"🔻 نسبة الضغط: {ratioText}",
                "نتيجة الضغط");
        }


        private void btnDecompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile) || !selectedFile.EndsWith(".huff"))
            {
                MessageBox.Show("يرجى اختيار ملف .huff لفك الضغط.");
                return;
            }

            HuffmanCompressor h = new HuffmanCompressor();
            string outputPath = Path.ChangeExtension(selectedFile, ".decompressed.txt");
            h.Decompress(selectedFile, outputPath);

            MessageBox.Show("تم فك الضغط بنجاح:\n" + outputPath);
        }
    }
}
