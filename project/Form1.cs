using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace project
{
    public partial class Form1 : Form
    {
        private string[] selectedFiles = new string[0];
        private bool isPaused = false;
        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
        private bool isCancelled = false;


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
                List<string> fileList = selectedFiles.ToList();
                fileList.AddRange(ofd.FileNames);
                selectedFiles = fileList.Distinct().ToArray();

                listBoxFiles.Items.Clear();
                listBoxFiles.Items.AddRange(selectedFiles);
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            var itemsToRemove = listBoxFiles.SelectedItems.Cast<string>().ToList();

            if (itemsToRemove.Count == 0)
            {
                MessageBox.Show("يرجى اختيار ملف أو أكثر للحذف.");
                return;
            }

            List<string> fileList = selectedFiles.ToList();
            foreach (string item in itemsToRemove)
            {
                fileList.Remove(item);
            }
            selectedFiles = fileList.ToArray();

            listBoxFiles.Items.Clear();
            listBoxFiles.Items.AddRange(selectedFiles);
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
    btnExtractOne.Enabled = false;
    btnStop.Enabled = true;
    btnCancel.Enabled = true;
    btnStop.Text = "إيقاف";
    lblResultMessage.Text = "🔄 جاري الضغط....";
    pauseEvent.Set();
    isPaused = false;
    isCancelled = false;

    HuffmanCompressor compressor = new HuffmanCompressor(pauseEvent, () => isCancelled);

    string mode = comboMode.SelectedItem.ToString();

    if (mode == "ضغط كل ملف بشكل منفصل")
    {
        StringBuilder report = new StringBuilder();
        foreach (string input in selectedFiles)
        {
            if (isCancelled)
                break;

            string output = input + ".huff";
            double ratio = await Task.Run(() => compressor.CompressSingleFile(input, output));
            if (!isCancelled)
                report.AppendLine($"📄 {Path.GetFileName(input)}: نسبة الضغط = {ratio:F2}%");
        }

        if (!isCancelled)
        {
            MessageBox.Show("✅ تم ضغط الملفات كلٌ على حدة:\n\n" + report.ToString());
        }
    }
    else if (mode == "ضغط الملفات كأرشيف واحد")
    {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Filter = "Huffman Archive|*.huffarc";
        sfd.Title = "احفظ الأرشيف المضغوط";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            string archivePath = sfd.FileName;
            double ratio = await Task.Run(() => compressor.CompressMultipleFiles(selectedFiles, archivePath));
            if (!isCancelled)
            {
                MessageBox.Show($"✅ تم ضغط الملفات في أرشيف.\nنسبة الضغط: {ratio:F2}%");
            }
            else if (File.Exists(archivePath))
            {
                File.Delete(archivePath); // حذف الملف إذا تم الإلغاء
            }
        }
    }

    lblResultMessage.Text = isCancelled ? "❌ تم إلغاء العملية." : "✅ تم الضغط.";
    btnCompress.Enabled = true;
    btnDecompress.Enabled = true;
    btnExtractOne.Enabled = true;
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
    btnExtractOne.Enabled = false;
    btnStop.Enabled = true;
    btnCancel.Enabled = true;
    btnStop.Text = "إيقاف";
    lblResultMessage.Text = "🔄 جاري فك الضغط...";
    GC.Collect();

    isCancelled = false;
    HuffmanCompressor compressor = new HuffmanCompressor(pauseEvent, () => isCancelled);

    foreach (string file in selectedFiles)
    {
        pauseEvent.Set();
        isPaused = false;

        if (isCancelled)
            break;

        if (file.EndsWith(".huffarc"))
        {
            await Task.Run(() => compressor.DecompressArchive(file));
        }
        else if (file.EndsWith(".huff"))
        {
            await Task.Run(() => compressor.DecompressSingleFile(file));
        }
    }

    if (!isCancelled)
        MessageBox.Show("✅ تم فك جميع الملفات.");

    lblResultMessage.Text = isCancelled ? "❌ تم إلغاء العملية." : "✅ تم فك الضغط.";

    GC.Collect();
    btnCompress.Enabled = true;
    btnDecompress.Enabled = true;
    btnExtractOne.Enabled = true;
    btnStop.Enabled = false;
    btnCancel.Enabled = false;
    selectedFiles = new string[0];
    listBoxFiles.Items.Clear();
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

        private void btnCancel_Click(object sender, EventArgs e)
         {
             isCancelled = true;
             pauseEvent.Set(); // إذا كانت العملية متوقفة مؤقتًا، نعيد تشغيلها حتى تنتهي فورًا
             lblStatus.Text = "❌ تم إلغاء العملية.";
             lblResultMessage.Text = "❌ تم إلغاء العملية.";
         }

    }
}
