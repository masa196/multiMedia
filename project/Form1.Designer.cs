namespace project
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnCompress;
        private System.Windows.Forms.Button btnDecompress;
        private System.Windows.Forms.Button btnExtractOne;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnCancel; // زر الإلغاء الجديد
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox comboMode;

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
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnCompress = new System.Windows.Forms.Button();
            this.btnDecompress = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button(); // الزر الجديد
            this.lblStatus = new System.Windows.Forms.Label();
            this.comboMode = new System.Windows.Forms.ComboBox();
            this.btnExtractOne = new System.Windows.Forms.Button();

            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(12, 12);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(500, 80);
            this.txtFilePath.Multiline = true;
            this.txtFilePath.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFilePath.TabIndex = 0;

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
            // btnCompress
            // 
            this.btnCompress.Location = new System.Drawing.Point(12, 100);
            this.btnCompress.Name = "btnCompress";
            this.btnCompress.Size = new System.Drawing.Size(100, 23);
            this.btnCompress.TabIndex = 2;
            this.btnCompress.Text = "ضغط";
            this.btnCompress.UseVisualStyleBackColor = true;
            this.btnCompress.Click += new System.EventHandler(this.btnCompress_Click);

            // 
            // btnDecompress
            // 
            this.btnDecompress.Location = new System.Drawing.Point(120, 100);
            this.btnDecompress.Name = "btnDecompress";
            this.btnDecompress.Size = new System.Drawing.Size(100, 23);
            this.btnDecompress.TabIndex = 3;
            this.btnDecompress.Text = "فك الضغط";
            this.btnDecompress.UseVisualStyleBackColor = true;
            this.btnDecompress.Click += new System.EventHandler(this.btnDecompress_Click);

            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(230, 100);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(100, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "إيقاف";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            this.btnStop.Enabled = false;

            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(230, 130);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "إلغاء نهائي";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.Enabled = false;

            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(340, 105);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 5;

            // 
            // comboMode
            // 
            this.comboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMode.Items.AddRange(new object[] {
                "ضغط كل ملف بشكل منفصل",
                "ضغط الملفات كأرشيف واحد"});
            this.comboMode.SelectedIndex = 0;
            this.comboMode.Location = new System.Drawing.Point(450, 100);
            this.comboMode.Name = "comboMode";
            this.comboMode.Size = new System.Drawing.Size(170, 21);
            this.comboMode.TabIndex = 6;

            // 
            // btnExtractOne
            // 
            this.btnExtractOne.Location = new System.Drawing.Point(340, 100);
            this.btnExtractOne.Name = "btnExtractOne";
            this.btnExtractOne.Size = new System.Drawing.Size(100, 23);
            this.btnExtractOne.TabIndex = 7;
            this.btnExtractOne.Text = "استخراج ملف";
            this.btnExtractOne.UseVisualStyleBackColor = true;
            this.btnExtractOne.Click += new System.EventHandler(this.btnExtractOne_Click);

            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(640, 170);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnCompress);
            this.Controls.Add(this.btnDecompress);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.comboMode);
            this.Controls.Add(this.btnExtractOne);
            this.Name = "Form1";
            this.Text = "ضغط وفك ضغط باستخدام هوفمان";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
