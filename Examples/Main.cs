using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Tests
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }
        ChatExample x = null;
        private void Button5_Click(object sender, EventArgs e)
        {
            if (x != null)
                return;

            x = new ChatExample();
            x.ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "PropertiesEditor.exe");
            if(File.Exists(path))
            {
                Process p = new Process() { StartInfo = new ProcessStartInfo() { FileName = path } };
                p.Start();
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            new IniForm().Show();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            new BinaryStringTable().Show();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            new SDDownloader().Show();
        }
    }
}
