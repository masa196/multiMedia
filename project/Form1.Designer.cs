namespace project
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnCompress;
        private System.Windows.Forms.Button btnDecompress;

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
            this.SuspendLayout();
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(12, 12);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(500, 20);
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
            this.btnCompress.Location = new System.Drawing.Point(12, 50);
            this.btnCompress.Name = "btnCompress";
            this.btnCompress.Size = new System.Drawing.Size(100, 23);
            this.btnCompress.TabIndex = 2;
            this.btnCompress.Text = "ضغط";
            this.btnCompress.UseVisualStyleBackColor = true;
            this.btnCompress.Click += new System.EventHandler(this.btnCompress_Click);
            // 
            // btnDecompress
            // 
            this.btnDecompress.Location = new System.Drawing.Point(120, 50);
            this.btnDecompress.Name = "btnDecompress";
            this.btnDecompress.Size = new System.Drawing.Size(100, 23);
            this.btnDecompress.TabIndex = 3;
            this.btnDecompress.Text = "فك الضغط";
            this.btnDecompress.UseVisualStyleBackColor = true;
            this.btnDecompress.Click += new System.EventHandler(this.btnDecompress_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(640, 100);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnCompress);
            this.Controls.Add(this.btnDecompress);
            this.Name = "Form1";
            this.Text = "ضغط وفك ضغط باستخدام هوفمان";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
