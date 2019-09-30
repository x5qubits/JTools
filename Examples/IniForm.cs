using JCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tests
{
    public partial class IniForm : Form
    {
        IniFile file = null;
        public IniForm()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Ini File (*.ini)|*.ini|All Files (*.*)|*.*";
                dialog.RestoreDirectory = false;
                dataGridView1.Rows.Clear();
                if (dialog.ShowDialog() == DialogResult.OK && File.Exists(dialog.FileName))
                {
                    file = new IniFile(dialog.FileName);
                    foreach (string Key in file.GetKeys())
                    {
                        string value = file.GetValue<string>(Key);
                        dataGridView1.Rows.Add(Key, value);
                    }
                }
            }
        }

    }
}
