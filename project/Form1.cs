using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project
{
    public partial class Form1 : Form
    {
        private string selectedFile = "";
        private Task currentTask;
        private System.Windows.Forms.Label lblStatus;

        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
        private bool isPaused = false;

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

        private async void btnCompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("يرجى اختيار ملف أولاً.");
                return;
            }

            btnCompress.Enabled = false;
            btnDecompress.Enabled = false;
            btnStop.Enabled = true;
            btnStop.Text = "إيقاف";
            lblStatus.Text = "🔄 جاري الضغط...";

            pauseEvent.Set();
            isPaused = false;

            string outputPath = selectedFile + ".huff"; // امتداد موحد للملف المضغوط
            long originalSize = new FileInfo(selectedFile).Length;

            currentTask = Task.Run(() =>
            {
                HuffmanCompressor h = new HuffmanCompressor(pauseEvent);
                h.Compress(selectedFile, outputPath);
            });

            await currentTask;

            long compressedSize = new FileInfo(outputPath).Length;
            string ratioText = compressedSize > 0
                ? (originalSize / (double)compressedSize).ToString("0.00") + "x"
                : "خطأ في الضغط";

            MessageBox.Show(
                $"📄 الحجم الأصلي: {originalSize} بايت\n" +
                $"📦 الحجم بعد الضغط: {compressedSize} بايت\n" +
                $"🔻 نسبة الضغط: {ratioText}",
                "نتيجة الضغط");

            lblStatus.Text = "✅ تم الضغط بنجاح.";

            btnCompress.Enabled = true;
            btnDecompress.Enabled = true;
            btnStop.Enabled = false;
        }

        private async void btnDecompress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile) || !selectedFile.EndsWith(".huff"))
            {
                MessageBox.Show("يرجى اختيار ملف .huff لفك الضغط.");
                return;
            }

            btnCompress.Enabled = false;
            btnDecompress.Enabled = false;
            btnStop.Enabled = true;
            btnStop.Text = "إيقاف";
            lblStatus.Text = "🔄 جاري فك الضغط...";

            pauseEvent.Set();
            isPaused = false;

            string outputPath = null; // سيتم توليده تلقائيًا داخل Decompress

            currentTask = Task.Run(() =>
            {
                HuffmanCompressor h = new HuffmanCompressor(pauseEvent);
                h.Decompress(selectedFile, outputPath);
            });

            await currentTask;

            lblStatus.Text = "✅ تم فك الضغط.";

            MessageBox.Show("تم فك الضغط بنجاح.");
            btnCompress.Enabled = true;
            btnDecompress.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (isPaused)
            {
                pauseEvent.Set();
                btnStop.Text = "إيقاف";
                lblStatus.Text = "🔄 جاري التنفيذ...";
                isPaused = false;
            }
            else
            {
                pauseEvent.Reset();
                btnStop.Text = "استئناف";
                lblStatus.Text = "⏸️ موقوف مؤقتًا.";
                isPaused = true;
            }
        }
    }
}
