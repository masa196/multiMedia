namespace project
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnCompress;
        private System.Windows.Forms.Button btnDecompress;
        private System.Windows.Forms.Button btnExtractOne;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDeleteSelected;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblResultMessage;
        private System.Windows.Forms.ComboBox comboMode;
        private System.Windows.Forms.ComboBox comboAlgorithm; 

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.listBoxFiles = new System.Windows.Forms.ListBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnDeleteSelected = new System.Windows.Forms.Button();
            this.btnCompress = new System.Windows.Forms.Button();
            this.btnDecompress = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnExtractOne = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.comboMode = new System.Windows.Forms.ComboBox();
            this.comboAlgorithm = new System.Windows.Forms.ComboBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblResultMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // 
            // listBoxFiles
            // 
            this.listBoxFiles.Location = new System.Drawing.Point(12, 12);
            this.listBoxFiles.Name = "listBoxFiles";
            this.listBoxFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxFiles.Size = new System.Drawing.Size(500, 69);
            this.listBoxFiles.TabIndex = 0;

            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(520, 10);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(100, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "اختيار ملف";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // 
            // btnDeleteSelected
            // 
            this.btnDeleteSelected.Location = new System.Drawing.Point(520, 40);
            this.btnDeleteSelected.Name = "btnDeleteSelected";
            this.btnDeleteSelected.Size = new System.Drawing.Size(100, 23);
            this.btnDeleteSelected.TabIndex = 2;
            this.btnDeleteSelected.Text = "حذف المحدد";
            this.btnDeleteSelected.UseVisualStyleBackColor = true;
            this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);

            // 
            // comboAlgorithm
            // 
            this.comboAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAlgorithm.Items.AddRange(new object[] {
            "Huffman",
            "Shannon-Fano"});
            this.comboAlgorithm.Location = new System.Drawing.Point(450, 90);
            this.comboAlgorithm.Name = "comboAlgorithm";
            this.comboAlgorithm.Size = new System.Drawing.Size(170, 21);
            this.comboAlgorithm.TabIndex = 11;
            this.comboAlgorithm.SelectedIndex = 0;

            // 
            // btnCompress
            // 
            this.btnCompress.Location = new System.Drawing.Point(12, 100);
            this.btnCompress.Name = "btnCompress";
            this.btnCompress.Size = new System.Drawing.Size(100, 23);
            this.btnCompress.TabIndex = 3;
            this.btnCompress.Text = "ضغط";
            this.btnCompress.UseVisualStyleBackColor = true;
            this.btnCompress.Click += new System.EventHandler(this.btnCompress_Click);

            // 
            // btnDecompress
            // 
            this.btnDecompress.Location = new System.Drawing.Point(120, 100);
            this.btnDecompress.Name = "btnDecompress";
            this.btnDecompress.Size = new System.Drawing.Size(100, 23);
            this.btnDecompress.TabIndex = 4;
            this.btnDecompress.Text = "فك الضغط";
            this.btnDecompress.UseVisualStyleBackColor = true;
            this.btnDecompress.Click += new System.EventHandler(this.btnDecompress_Click);

            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(230, 100);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 23);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "إيقاف";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);

            // 
            // btnExtractOne
            // 
            this.btnExtractOne.Location = new System.Drawing.Point(340, 100);
            this.btnExtractOne.Name = "btnExtractOne";
            this.btnExtractOne.Size = new System.Drawing.Size(100, 23);
            this.btnExtractOne.TabIndex = 6;
            this.btnExtractOne.Text = "استخراج ملف";
            this.btnExtractOne.UseVisualStyleBackColor = true;
            this.btnExtractOne.Click += new System.EventHandler(this.btnExtractOne_Click);

            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(340, 130);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "إلغاء العملية";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // 
            // comboMode
            // 
            this.comboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMode.Items.AddRange(new object[] {
            "ضغط كل ملف بشكل منفصل",
            "ضغط الملفات كأرشيف واحد"});
            this.comboMode.Location = new System.Drawing.Point(450, 130);
            this.comboMode.Name = "comboMode";
            this.comboMode.Size = new System.Drawing.Size(170, 21);
            this.comboMode.TabIndex = 7;

            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 130);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 8;

            // 
            // lblResultMessage
            // 
            this.lblResultMessage.AutoSize = true;
            this.lblResultMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResultMessage.ForeColor = System.Drawing.Color.Green;
            this.lblResultMessage.Location = new System.Drawing.Point(12, 150);
            this.lblResultMessage.Name = "lblResultMessage";
            this.lblResultMessage.Size = new System.Drawing.Size(0, 15);
            this.lblResultMessage.TabIndex = 9;

            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(640, 190);
            this.Controls.Add(this.listBoxFiles);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnDeleteSelected);
            this.Controls.Add(this.btnCompress);
            this.Controls.Add(this.btnDecompress);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnExtractOne);
            this.Controls.Add(this.comboMode);
            this.Controls.Add(this.comboAlgorithm); 
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblResultMessage);
            this.Name = "Form1";
            this.Text = "ضغط وفك ضغط باستخدام هوفمان";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
