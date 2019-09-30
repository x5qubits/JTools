using System;
using System.IO;
using System.Windows.Forms;
using JCommon.FileDatabase;
using JCommon.FileDatabase.Containers;
using JHUI.Forms;

namespace GameDataBaseEditor
{
    public partial class MainForm : JForm
    {
        private bool locked;
        private FileItem selectedItem;
        public int version = 1;
        public int lastList = 0;
        public int lastItem = 0;

        private string path = null;
        public static int encriptPin = 0;

        public MainForm()
        {
            InitializeComponent();
            encriptPin = 1984;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog eLoad = new OpenFileDialog())
            {
                eLoad.Filter = "Elements File (*.data)|*.data|All Files (*.*)|*.*";
                eLoad.RestoreDirectory = false;
                if (eLoad.ShowDialog() == DialogResult.OK && File.Exists(eLoad.FileName))
                {
                    locked = true;
                    path = eLoad.FileName;
                    NestedFileDatabase.ReadFile(eLoad.FileName);
                    BuildLists();

                    locked = false;
                    comboBox_lists.SelectedIndex = 0;
                    listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList);
                    listBox_items.PerformLayout();
                }
            }
        }

        private void BuildLists()
        {
            comboBox_lists.Items.Clear();
            int index = 0;
            foreach (FileItems sub in NestedFileDatabase.Collection.GetLists())
            {
                comboBox_lists.Items.Add(sub.ListId + "-" + sub.ListName);
                index++;
            }
            if (index != 0)
                comboBox_lists.SelectedIndex = index - 1;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NestedFileDatabase.IsLoaded)
            {
                NestedFileDatabase.SaveFile(path);
                MessageBox.Show("Saved!");
            }
            else
            {
                OpenFileDialog eLoad = new OpenFileDialog();
                eLoad.Filter = "Elements File (*.data)|*.data|All Files (*.*)|*.*";
                eLoad.RestoreDirectory = false;
                if (eLoad.ShowDialog() == DialogResult.OK && File.Exists(eLoad.FileName))
                {
                    path = eLoad.FileName;
                    NestedFileDatabase.SaveFile(path);
                    MessageBox.Show("Saved!");
                }
            }
        }

        private void OnClickAddItem(object sender, EventArgs e)
        {
            locked = true;
            listBox_items.SuspendLayout();
            listBox_items.RowCount = 0;
            NestedFileDatabase.AddNew(GetList);
            locked = false;
            BuildLists();
            listBox_items.ResumeLayout();
            listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList);
            listBox_items.PerformLayout();
        }

        private int GetList
        {
            get
            {
                int ListId = comboBox_lists.SelectedIndex;
                if (ListId == -1)
                    ListId = 0;
                return ListId;
            }
        }

        private void ListBoxSelectItem(object sender, EventArgs e)
        {
            if (locked || listBox_items.CurrentCell == null) return;
            locked = true;
            
            dataGridView_item.SuspendLayout();
            dataGridView_item.RowCount = 0;
            selectedItem = NestedFileDatabase.Collection.GetItem(GetList, listBox_items.CurrentCell.RowIndex);
            if (selectedItem != null)
                dataGridView_item.RowCount = selectedItem.RowCount;

            locked = false;
            dataGridView_item.ResumeLayout();
            dataGridView_item.PerformLayout();
        }

        private void ListBox_items_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (locked) return;
            locked = true;
            FileItem item = NestedFileDatabase.Collection.GetItem(GetList, e.RowIndex);
            if (item != null)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        e.Value = item.ItemId;
                        break;
                    case 1:
                        e.Value = item.ItemName;
                        break;
                }
            }
            locked = false;
        }

        private void DataGridView_item_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (locked) return;
            locked = true;
            if (selectedItem != null)
            {
                switch (e.ColumnIndex)
                {
                    case 0:
                        e.Value = selectedItem.GetRow(e.RowIndex).RowIndex;
                        break;
                    case 1:
                        e.Value = selectedItem.GetRow(e.RowIndex).RowName;
                        break;
                    case 2:
                        e.Value = selectedItem.GetRow(e.RowIndex).RowType.ToString();
                        break;
                    case 3:
                        e.Value = selectedItem.GetRow(e.RowIndex).RowValue;
                        break;
                }
            }
            locked = false;
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (locked || listBox_items.CurrentCell == null) return;
            locked = true;
            dataGridView_item.SuspendLayout();
            dataGridView_item.RowCount = 0;
            selectedItem.AddRow();
            NestedFileDatabase.Collection.SetItem(GetList, selectedItem);
            selectedItem = NestedFileDatabase.Collection.GetItem(GetList, listBox_items.CurrentCell.RowIndex);

            if (selectedItem != null)
                dataGridView_item.RowCount = selectedItem.RowCount;

            locked = false;
            dataGridView_item.ResumeLayout();
            dataGridView_item.PerformLayout();

        }

        private void DataGridView_item_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (locked) return;
            locked = true;
            if (selectedItem != null)
            {
                var editedCell = dataGridView_item.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string newValue = editedCell.EditedFormattedValue.ToString();
                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    int ItemId = selected[x].Index;
                    FileItem item = NestedFileDatabase.Collection.GetItem(GetList, ItemId);
                    switch (e.ColumnIndex)
                    {
                        case 1:
                            item.GetRow(e.RowIndex).RowName = newValue;
                            break;
                        case 2:
                            item.GetRow(e.RowIndex).RowType = (FileRowType)Enum.Parse(typeof(FileRowType), newValue);
                            break;
                        case 3:
                            FileRow row = item.GetRow(e.RowIndex);
                            if (row != null)
                            {
                                row.RowValue = newValue;
                                item.SetRow(row);
                            }
                            break;
                    }
                    NestedFileDatabase.Collection.SetItem(GetList, item);
                }
            }
            locked = false;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked || listBox_items.CurrentCell == null) return;
            locked = true;
            int itemId = listBox_items.CurrentCell.RowIndex;
            
            listBox_items.SuspendLayout();
            listBox_items.RowCount = 0;
            NestedFileDatabase.Collection.RemoveItem(GetList, itemId);
            listBox_items.ResumeLayout();
            locked = false;
            listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList);
            listBox_items.PerformLayout();
           
            int lastItem = itemId - 1;
            if (lastItem > 0)
            {
                listBox_items.Rows[lastItem].Selected = true;
                listBox_items.CurrentCell = listBox_items.Rows[lastItem].Cells[0];
            }

        }

        private void ListBox_items_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (locked) return;
            locked = true;
            FileItem item = NestedFileDatabase.Collection.GetItem(GetList, e.RowIndex);
            if (item != null)
            {
                var editedCell = listBox_items.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string newValue = editedCell.EditedFormattedValue.ToString();
                switch (e.ColumnIndex)
                {
                    case 1:
                        item.ItemName = newValue;
                        break;
                }
                NestedFileDatabase.Collection.SetItem(GetList, item);
            }
            locked = false;
        }

        private void CloneToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            if (selectedItem != null && dataGridView_item.CurrentCell != null)
            {
                int itemId = listBox_items.CurrentCell.RowIndex;
                listBox_items.SuspendLayout();
                listBox_items.RowCount = 0;
                NestedFileDatabase.Collection.CloneAdd(GetList,selectedItem);
                listBox_items.ResumeLayout();
                locked = false;
                listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList);
                listBox_items.PerformLayout();

                int lastItem = listBox_items.RowCount - 1;
                if (lastItem > 0)
                {
                    listBox_items.Rows[lastItem].Selected = true;
                    listBox_items.CurrentCell = listBox_items.Rows[lastItem].Cells[0];
                }
            }
            locked = false;
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            if (selectedItem != null && dataGridView_item.CurrentCell!=null)
            {
                int RowIndex = dataGridView_item.CurrentCell.RowIndex;
                dataGridView_item.SuspendLayout();
                dataGridView_item.RowCount = 0;
                selectedItem.RemoveRow(RowIndex);
                NestedFileDatabase.Collection.SetItem(GetList, selectedItem);
                dataGridView_item.ResumeLayout();
                locked = false;
                dataGridView_item.PerformLayout();
                ListBoxSelectItem(null, null);
            }
            locked = false;
        }

        private void MoveUPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            if (selectedItem != null && dataGridView_item.CurrentCell != null)
            {

                dataGridView_item.SuspendLayout();
                
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    Application.DoEvents();
                    int idx = listBox_items.SelectedRows[x].Index;
                    FileItem item = NestedFileDatabase.Collection.GetItem(GetList, idx);
                    for (int i = 0; i < dataGridView_item.SelectedRows.Count; i++)
                    {
                        int xy = dataGridView_item.SelectedRows[i].Index;
                        item.MoveUp(xy);
                    }
                }
                dataGridView_item.RowCount = 0;
                NestedFileDatabase.Collection.SetItem(GetList, selectedItem);
                dataGridView_item.ResumeLayout();
                locked = false;
                dataGridView_item.PerformLayout();
                ListBoxSelectItem(null, null);
            }
            locked = false;
        }

        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            if (selectedItem != null && dataGridView_item.CurrentCell != null)
            {

                dataGridView_item.SuspendLayout();
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    Application.DoEvents();
                    int idx = listBox_items.SelectedRows[x].Index;
                    FileItem item = NestedFileDatabase.Collection.GetItem(GetList,idx);
                    for (int i = 0; i < dataGridView_item.SelectedRows.Count; i++)
                    {
                        int xy = dataGridView_item.SelectedRows[i].Index;
                        item.MoveDown(xy);
                    }
                }
                dataGridView_item.RowCount = 0;

                dataGridView_item.ResumeLayout();
                locked = false;
                dataGridView_item.PerformLayout();
                ListBoxSelectItem(null, null);
            }
            locked = false;
        }

        private void ComboBox_lists_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            FileItems list = NestedFileDatabase.Collection.GetList(GetList);
            if (list != null)
            {
                textBox_Name.Text = list.ListName; locked = true;
                listBox_items.SuspendLayout();
                listBox_items.RowCount = 0;
                locked = false;
                listBox_items.ResumeLayout();
                listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList);
                listBox_items.PerformLayout();
                ListBoxSelectItem(null, null);
            }
            locked = false;
        }

        private void SaveList(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            FileItems list = NestedFileDatabase.Collection.GetList(GetList);
            if (list != null)
            {
                NestedFileDatabase.Collection.SetListName(GetList, textBox_Name.Text); BuildLists();
            }
            locked = false;

        }

        private void DeleteList(object sender, EventArgs e)
        {
            locked = true;
            listBox_items.SuspendLayout();
            listBox_items.RowCount = 0;
            NestedFileDatabase.Collection.DeleteList(GetList);
            locked = false;
            BuildLists();
            listBox_items.ResumeLayout();
            listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList);
            listBox_items.PerformLayout();
        }

        private void AddNewList(object sender, EventArgs e)
        {
            locked = true;
            listBox_items.SuspendLayout();
            listBox_items.RowCount = 0;
            NestedFileDatabase.AddNew(GetList + 1);
            locked = false;
            BuildLists();
            listBox_items.ResumeLayout();
            listBox_items.RowCount = NestedFileDatabase.ItemCount(GetList + 1);
            listBox_items.PerformLayout();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (locked) return;
            locked = true;
            string search = searchInput1.Text.ToString();
            FileItems[] all = NestedFileDatabase.Collection.GetLists();
            for(int l = lastList; l < all.Length; l++)
            {
                FileItems List = all[l];
                if (List != null)
                {
                    var allItems = List.GetItems();
                    for (int i = 0; i < allItems.Length; i++)
                    {
                        var allRows = allItems[i].GetRows();
                        for(int r = 0; r < allRows.Length; r++)
                        {
                            var row = allRows[i];
                            if(
                               Compare(search.ToLower().Trim(), row.RowName.ToLower().Trim())
                            || Compare(search.ToLower().Trim(), row.RowValue.ToString().ToLower().Trim())
                                )
                            {
                                locked = false;
                                comboBox_lists.SelectedIndex = l;
 
                                listBox_items.Rows[i].Selected = true;
                                listBox_items.CurrentCell = listBox_items.Rows[i].Cells[0];
                                lastList = l;
                                lastItem = i + 1;
                                
                                break;
                            }
                        }
                    }
                }
            }
            locked = false;
            lastList = 0;
            lastItem = 0;
        }

        private bool Compare(string a, string b)
        {
            if (caseSensitiveCheckbox.Checked)
                return a.Equals(b);
            else
                return a.Contains(b);
        }
    }
}
