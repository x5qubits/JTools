using JCommon.FileDatabase;
using JCommon.FileDatabase.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Tests
{
    public partial class BinaryStringTable : Form
    {
        private StrFile filedatabase = new StrFile();
        List<string> database = new List<string>();

        public BinaryStringTable()
        {
            InitializeComponent();
        }
        class StrFile : DataFile
        {
            public string[] table = new string[0];
            public override void Deserialize(DataReader reader)
            {
                uint size = reader.ReadPackedUInt32();
                table = new string[size];
                for (int i = 0; i < size; i++)
                {
                    table[i] = reader.ReadString();
                }
            }
            public override void Serialize(DataWriter writer)
            {
                writer.WritePackedUInt32((uint)table.Length);
                for (int i = 0; i < table.Length; i++)
                {
                    writer.Write(table[i]);
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "String table file (*.jstr)|*.jstr|All Files (*.*)|*.*";
                dialog.RestoreDirectory = false;
                dataGridView1.Rows.Clear();
                if (dialog.ShowDialog() == DialogResult.OK && File.Exists(dialog.FileName))
                {
                    filedatabase = FileDatabase.ReadFile<StrFile>(dialog.FileName);
                    database = new List<string>(filedatabase.table);
                    DrawList();
                }
            }
        }

        private void DrawList()
        {
            dataGridView1.SuspendLayout();
            dataGridView1.Rows.Clear();
            for(int i = 0; i < database.Count; i++)
            {
                dataGridView1.Rows.Add(i.ToString(), database[i]);
            }
            dataGridView1.ResumeLayout();
            dataGridView1.PerformLayout();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "String table file (*.jstr)|*.jstr|All Files (*.*)|*.*";
                dialog.RestoreDirectory = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    filedatabase.table = database.ToArray();
                    FileDatabase.WriteFile(dialog.FileName, filedatabase);
                }
            }
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            database.Add("");
            DrawList();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(dataGridView1.CurrentCell != null)
            {
                database.RemoveAt(dataGridView1.CurrentCell.RowIndex);
                DrawList();
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
            {
                int index = dataGridView1.CurrentCell.RowIndex;
                if(index < database.Count)
                {
                    database[index] = dataGridView1.Rows[index].Cells[1].EditedFormattedValue.ToString();
                }
            }
        }
    }
}
