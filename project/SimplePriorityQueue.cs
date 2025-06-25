using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Text;

public class SimplePriorityQueue
{
    private List<HuffmanNode> nodes = new List<HuffmanNode>();

    public void Enqueue(HuffmanNode node)
    {
        nodes.Add(node);
        nodes = nodes.OrderBy(n => n.Frequency).ToList();
    }

    public HuffmanNode Dequeue()
    {
        var first = nodes[0];
        nodes.RemoveAt(0);
        return first;
    }

    public int Count()
    {
        return nodes.Count;
    }
}
public class SelectFileDialog : Form  // <== هنا أضفنا : Form
    {
        private ListBox listBox;
        private Button btnOK;
        private Button btnCancel;

        public string SelectedFileName { get; private set; }

        public SelectFileDialog(List<string> fileNames)
        {
            this.Text = "اختر ملف من الأرشيف";
            this.Width = 400;
            this.Height = 300;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            listBox = new ListBox();
            listBox.Dock = DockStyle.Top;
            listBox.Height = 200;
            listBox.SelectionMode = SelectionMode.One;
            listBox.Items.AddRange(fileNames.ToArray());
            this.Controls.Add(listBox);

            btnOK = new Button();
            btnOK.Text = "موافق";
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Top = 210;
            btnOK.Left = 200;
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Top = 210;
            btnCancel.Left = 280;
            this.Controls.Add(btnCancel);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                SelectedFileName = listBox.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("يرجى اختيار ملف أولاً.");
            }
        }
    }