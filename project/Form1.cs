using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace project
{
    public partial class Form1 : Form
    {
        private string[] selectedFiles = new string[0];
        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);
        private bool isPaused = false;
        private bool isCancelled = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboAlgorithm.SelectedIndex = 0;
            comboMode.SelectedIndex = 0;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFiles = ofd.FileNames;
                    listBoxFiles.Items.Clear();
                    listBoxFiles.Items.AddRange(selectedFiles);
                }
            }
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    selectedFiles = Directory.GetFiles(fbd.SelectedPath);
                    listBoxFiles.Items.Clear();
                    listBoxFiles.Items.AddRange(selectedFiles);
                }
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            var selectedItems = listBoxFiles.SelectedItems;
            var newList = new System.Collections.Generic.List<string>(selectedFiles);
            foreach (var item in selectedItems)
                newList.Remove(item.ToString());

            selectedFiles = newList.ToArray();
            listBoxFiles.Items.Clear();
            listBoxFiles.Items.AddRange(selectedFiles);
        }

        private string AskPasswordFromUser()
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "أدخل كلمة السر";
                var textBox = new TextBox() { PasswordChar = '*', Width = 200, Left = 10, Top = 10 };
                var btnOk = new Button() { Text = "موافق", DialogResult = DialogResult.OK, Left = 220, Top = 10 };
                inputForm.Controls.Add(textBox);
                inputForm.Controls.Add(btnOk);
                inputForm.AcceptButton = btnOk;
                inputForm.StartPosition = FormStartPosition.CenterParent;

                if (inputForm.ShowDialog() == DialogResult.OK)
                    return textBox.Text;
                else
                    return null;
            }
        }

       private async void btnCompress_Click(object sender, EventArgs e)
{
    if (listBoxFiles.Items.Count == 0)
    {
        MessageBox.Show("يرجى إضافة ملف واحد على الأقل للضغط.");
        return;
    }

    var selectedFiles = listBoxFiles.Items.Cast<string>().ToList();

    btnCompress.Enabled = false;
    btnDecompress.Enabled = false;
    btnExtractOne.Enabled = false;
    btnStop.Enabled = true;
    btnCancel.Enabled = true;
    lblResultMessage.Text = "";
    lblStatus.Text = "";

    isCancelled = false;
    isPaused = false;
    pauseEvent.Set();

    string mode = comboMode.SelectedItem?.ToString() ?? "ضغط كل ملف بشكل منفصل";
    string password = null;

    if (mode == "ضغط كل ملف بشكل منفصل")
    {
        password = PasswordDialog.ShowDialog("الرجاء إدخال كلمة السر للضغط (ستُستخدم لفك الضغط لاحقًا):");
        if (password == null)
        {
            MessageBox.Show("تم إلغاء العملية بسبب عدم إدخال كلمة السر.");
            ResetButtons();
            return;
        }
    }

    ICompressor compressor = CreateSelectedCompressor();

    try
    {
        if (mode == "ضغط كل ملف بشكل منفصل")
        {
            StringBuilder report = new StringBuilder();

            foreach (string input in selectedFiles)
            {
                if (isCancelled) break;

                string extension = comboAlgorithm.SelectedItem.ToString() == "Huffman" ? ".huff" : ".sf";
                string output = input + extension;

                double ratio = 0;

                if (comboAlgorithm.SelectedItem.ToString() == "Huffman")
                {
                    ratio = await Task.Run(() => ((HuffmanCompressor)compressor).CompressSingleFile(input, output, password));
                }
                else if (comboAlgorithm.SelectedItem.ToString() == "Shannon-Fano")
                {
                    ratio = await Task.Run(() => ((ShannonFanoCompressor)compressor).CompressSingleFile(input, output, password));
                }
                else
                {
                    ratio = await Task.Run(() => compressor.CompressSingleFile(input, output));
                }

                if (!isCancelled)
                    report.AppendLine($"📄 {Path.GetFileName(input)}: نسبة الضغط = {ratio:F2}%");
            }

            if (!isCancelled)
                MessageBox.Show("✅ تم ضغط الملفات كلٌ على حدة:\n\n" + report.ToString());
        }
        else if (mode == "ضغط الملفات كأرشيف واحد")
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Huffman Archive|*.huffarc";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string extension = comboAlgorithm.SelectedItem.ToString() == "Huffman" ? ".huffarc" : ".sfarc";
                string archivePath = Path.Combine(
                    Path.GetDirectoryName(sfd.FileName),
                    Path.GetFileNameWithoutExtension(sfd.FileName) + extension
                );

                double ratio = await Task.Run(() => compressor.CompressMultipleFiles(selectedFiles.ToArray(), archivePath));
                if (!isCancelled)
                    MessageBox.Show($"✅ تم ضغط الملفات في أرشيف.\nنسبة الضغط: {ratio:F2}%");
                else if (File.Exists(archivePath))
                    File.Delete(archivePath);
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("حدث خطأ أثناء الضغط: " + ex.Message);
    }
    finally
    {
        lblResultMessage.Text = isCancelled ? "❌ تم إلغاء العملية." : "✅ تم الضغط.";
        ResetButtons();
    }
}

private void ResetButtons()
{
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
           ICompressor compressor = CreateSelectedCompressor();
       
           foreach (string file in selectedFiles)
           {
               pauseEvent.Set();
               isPaused = false;
           
               if (isCancelled)
                   break;
           
               if (file.EndsWith(".huffarc") || file.EndsWith(".sfarc"))
               {
                   await Task.Run(() => compressor.DecompressArchive(file));
               }
               else if (file.EndsWith(".huff") || file.EndsWith(".sf"))
               {
                   bool success = await Task.Run(() => compressor.DecompressSingleFile(file));
                   if (!success)
                   {
                       isCancelled = true;
                       break;
                   }
               }
           }

       
           if (!isCancelled)
           {
               MessageBox.Show("✅ تم فك جميع الملفات.");
               lblResultMessage.Text = "✅ تم فك الضغط.";
           }
           else
           {
               lblResultMessage.Text = "❌ تم إلغاء العملية.";
           }
       
           GC.Collect();
           btnCompress.Enabled = true;
           btnDecompress.Enabled = true;
           btnExtractOne.Enabled = true;
           btnStop.Enabled = false;
           btnCancel.Enabled = false;
           selectedFiles = new string[0];
           listBoxFiles.Items.Clear();
       }


        private ICompressor CreateSelectedCompressor()
        {
            string algorithm = comboAlgorithm.SelectedItem.ToString();
            if (algorithm == "Huffman")
                return new HuffmanCompressor(pauseEvent, () => isCancelled);
            else
                return new ShannonFanoCompressor(pauseEvent, () => isCancelled);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                pauseEvent.Reset();
                btnStop.Text = "استئناف";
                lblStatus.Text = "⏸️ تم الإيقاف المؤقت.";
            }
            else
            {
                pauseEvent.Set();
                btnStop.Text = "إيقاف";
                lblStatus.Text = "";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            isCancelled = true;
            pauseEvent.Set();
            lblStatus.Text = "❌ تم إلغاء العملية.";
        }

        private void btnExtractOne_Click(object sender, EventArgs e)
        {
            if (selectedFiles.Length != 1)
            {
                MessageBox.Show("يرجى تحديد أرشيف واحد فقط.");
                return;
            }

            ICompressor compressor = CreateSelectedCompressor();
            compressor.DecompressArchive(selectedFiles[0]);
            MessageBox.Show("✅ تم استخراج الملفات.");
        }
    }
} 