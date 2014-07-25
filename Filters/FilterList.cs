using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EducServLib;

namespace Priem
{
    public partial class FilterList : Form
    {
        FormFilter _owner;
        bool _IsNew;

        //�����������
        public FilterList(FormFilter la, bool isNew)
        {
            InitializeComponent();

            _owner = la;
            _IsNew = isNew;

            foreach (SavedFilter sf in MainClass._config.SavedFilters)
                lbSchemas.Items.Add(sf);            

            if (isNew)
            {
                btnDelete.Visible = false;
            }
            else
            {
                tbName.Enabled = false;
                btnDelete.Visible = true;
            }
        }

        //������ �����������
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (_IsNew)
            {
                string name = tbName.Text.Trim();
                MainClass._config.AddSavedFilter(new SavedFilter(name, _owner.FilterList));
            }
            else//�������
            {
                int i = lbSchemas.SelectedIndex;
                if (i < 0)
                    return;

                _owner.FilterList = (lbSchemas.SelectedItem as SavedFilter).FilterList;
                _owner.UpdateDataGrid();
            }
            this.Close();
        }

        //������ �������
        private void btnDelete_Click(object sender, EventArgs e)
        {
            object obj = lbSchemas.SelectedItem;
            MainClass._config.DeleteSavedFilter(obj.ToString());
            lbSchemas.Items.Remove(obj);
        }

        //��������� ������� �����
        private void lbSchemas_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = lbSchemas.SelectedIndex;

            if (i < 0)
            {
                btnOk.Enabled = false;
                btnDelete.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
                btnDelete.Enabled = true;
                tbName.Text = lbSchemas.SelectedItem.ToString();
            }
        }

        //��������� ������
        private void tbName_TextChanged(object sender, EventArgs e)
        {
            string s = tbName.Text.Trim();

            if (s.Length == 0)
            {
                btnOk.Enabled = false;
            }
            else
            {
                btnOk.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}