using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace project
{
    public partial class Form1 : Form
    {
        private string[] selectedFiles = new string[0];
        private bool isPaused = false;
        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
        private CancellationTokenSource cancelTokenSource;

        public Form1()
        {
            InitializeComponent();
            lblStatus.Text = "";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Title = "اختر ملف أو أكثر";
            ofd.Filter = "كل الملفات|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFiles = selectedFiles.Concat(ofd.FileNames).Distinct().ToArray();
                txtFilePath.Text = string.Join(Environment.NewLine, selectedFiles);
            }
        }

        private async void btnCompress_Click(object sender, EventArgs e)
        {
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                MessageBox.Show("يرجى اختيار ملف واحد أو أكثر أولاً.");
                return;
            }

            btnCompress.Enabled = false;
            btnDecompress.Enabled = false;
            btnStop.Enabled = true;
            btnCancel.Enabled = true;
            btnStop.Text = "إيقاف";
            lblStatus.Text = "🔄 جاري الضغط...";

            cancelTokenSource = new CancellationTokenSource();
            pauseEvent.Set();
            isPaused = false;

            HuffmanCompressor compressor = new HuffmanCompressor(pauseEvent);
            string mode = comboMode.SelectedItem.ToString();

            try
            {
                if (mode == "ضغط كل ملف بشكل منفصل")
                {
                    StringBuilder report = new StringBuilder();
                    foreach (string input in selectedFiles)
                    {
                        string output = input + ".huff";
                        double ratio = await Task.Run(() => compressor.CompressSingleFile(input, output, cancelTokenSource.Token));
                        if (ratio < 0) throw new OperationCanceledException();
                        report.AppendLine($"📄 {Path.GetFileName(input)}: نسبة الضغط = {ratio:F2}%");
                    }
                    MessageBox.Show("✅ تم ضغط الملفات كلٌ على حدة:\n\n" + report.ToString());
                }
                else if (mode == "ضغط الملفات كأرشيف واحد")
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Huffman Archive|*.huffarc";
                    sfd.Title = "احفظ الأرشيف المضغوط";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        double ratio = await Task.Run(() => compressor.CompressMultipleFiles(selectedFiles, sfd.FileName, cancelTokenSource.Token));
                        if (ratio < 0) throw new OperationCanceledException();
                        MessageBox.Show($"✅ تم ضغط الملفات في أرشيف.\nنسبة الضغط: {ratio:F2}%");
                    }
                }

                lblStatus.Text = "✅ تم الضغط.";
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("❌ تم إلغاء عملية الضغط.");
                lblStatus.Text = "⚠️ أُلغي الضغط.";
            }

            btnCompress.Enabled = true;
            btnDecompress.Enabled = true;
            btnStop.Enabled = false;
            btnCancel.Enabled = false;
        }

        private async void btnDecompress_Click(object sender, EventArgs e)
        {
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                MessageBox.Show("يرجى اختيار ملفات .huff أو .huffarc.");
                return;
            }

            btnCompress.Enabled = false;
            btnDecompress.Enabled = false;
            btnStop.Enabled = true;
            btnCancel.Enabled = true;
            btnStop.Text = "إيقاف";
            lblStatus.Text = "🔄 جاري فك الضغط...";

            cancelTokenSource = new CancellationTokenSource();
            pauseEvent.Set();
            isPaused = false;

            HuffmanCompressor compressor = new HuffmanCompressor(pauseEvent);
            try
            {
                foreach (string file in selectedFiles)
                {
                    if (file.EndsWith(".huffarc"))
                        await Task.Run(() => compressor.DecompressArchive(file, null, cancelTokenSource.Token));
                    else if (file.EndsWith(".huff"))
                        await Task.Run(() => compressor.DecompressSingleFile(file, null, cancelTokenSource.Token));
                }
                MessageBox.Show("✅ تم فك جميع الملفات.");
                lblStatus.Text = "✅ تم فك الضغط.";
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("❌ تم إلغاء عملية فك الضغط.");
                lblStatus.Text = "⚠️ أُلغي فك الضغط.";
            }

            btnCompress.Enabled = true;
            btnDecompress.Enabled = true;
            btnStop.Enabled = false;
            btnCancel.Enabled = false;
            selectedFiles = new string[0];
            txtFilePath.Text = "";
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
                lblStatus.Text = "⛔ يتم الآن الإلغاء...";
            }
        }

        private void btnExtractOne_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Length == 0 || !selectedFiles[0].EndsWith(".huffarc"))
            {
                MessageBox.Show("يرجى اختيار أرشيف .huffarc أولاً.");
                return;
            }

            string archivePath = selectedFiles[0];
            HuffmanCompressor compressor = new HuffmanCompressor(pauseEvent);
            List<string> fileNames = compressor.GetFileListFromArchive(archivePath);

            using (var dialog = new SelectFileDialog(fileNames))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = dialog.SelectedFileName;
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.FileName = selectedFile;
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        compressor.ExtractSingleFileFromArchive(archivePath, selectedFile, sfd.FileName);
                        MessageBox.Show("✅ تم استخراج الملف بنجاح.");
                    }
                }
            }
        }
    }
}
