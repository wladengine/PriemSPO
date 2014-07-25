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
    public partial class Groups : Form
    {
        private FormFilter _owner;
        //private List<ListItem> _columnList;

        private bool flag;

        public Groups(FormFilter la)
        {
            InitializeComponent();

            _owner = la;

            foreach (ListItem li in FillGroups())
                lbNo.Items.Add(li);
            chbPrintGroup.Checked = true;
        }

        private List<ListItem> FillGroups()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem("���","������� ��� ��������"));
            list.Add(new ListItem("�����������", "�����������"));
            list.Add(new ListItem("�����_���������", "��������������� ���������"));
            list.Add(new ListItem("���_��������","��� ���������"));            
            list.Add(new ListItem("������_���", "������ ���������"));
            list.Add(new ListItem("���_�������", "���"));
            list.Add(new ListItem("��������", "��������"));
            list.Add(new ListItem("���_�������", "��� �������"));
            list.Add(new ListItem("��_����", "����������� ����"));
            list.Add(new ListItem("�������", "�������"));
            list.Add(new ListItem("�����_��������", "����� ��������"));
            list.Add(new ListItem("������_��������", "������ ��������"));
            list.Add(new ListItem("���_��������", "��� ��������"));
            list.Add(new ListItem("������_������", "������ ������"));
            list.Add(new ListItem("������", "������"));
            list.Add(new ListItem("�����������", "�����������"));
            list.Add(new ListItem("�������", "�������"));
            list.Add(new ListItem("���������", "���������"));           
            list.Add(new ListItem("�������������_���������_�����������", "������������� ��������� �� ����� �����������"));
            list.Add(new ListItem("������", "������"));
            list.Add(new ListItem("���_��_���������", "��� �������� ���������"));
            //list.Add(new ListItem("", ""));
                                      
            return list;
        }           

        //������
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //������������� � ��������� ����������
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (lbYes.Items.Count > 0)
            {
                List<ListItem> list = new List<ListItem>();
                foreach (ListItem li in lbYes.Items)
                {
                    list.Add(li);
                    if(_owner is ListAbit)
                        MainClass._config.AddColumnNameAbit(li.Id);
                    else if(_owner is ListPersonFilter)
                        MainClass._config.AddColumnNamePerson(li.Id);
                }
                _owner.GroupList = list;
                _owner.GroupPrint = chbPrintGroup.Checked;

                _owner.UpdateDataGrid();
            }            
            this.Close();
        }

        //������� �������� �����
        private void btnLeft_Click(object sender, EventArgs e)
        {
            WinFormsServ.MoveRows(lbNo, lbYes, false);
        }

        //
        private void btnRight_Click(object sender, EventArgs e)
        {
            WinFormsServ.MoveRows(lbYes, lbNo, false);
        }

        //
        private void btnLeftAll_Click(object sender, EventArgs e)
        {
            WinFormsServ.MoveRows(lbNo, lbYes, true);
        }

        //
        private void btnRightAll_Click(object sender, EventArgs e)
        {
            WinFormsServ.MoveRows(lbYes, lbNo, true);
        }

        //������� �����
        private void btnUp_Click(object sender, EventArgs e)
        {
            flag = true;

            if (lbYes.Items.Count == 0)
                return;

            int i = lbYes.SelectedIndex;
            if (i == 0)
                return;

            object obj = lbYes.Items[i];
            lbYes.Items.RemoveAt(i);
            lbYes.Items.Insert(i - 1, obj);
            lbYes.SetSelected(i - 1, true);
            flag = false;
        }

        //������� ����
        private void btnDown_Click(object sender, EventArgs e)
        {
            flag = true;

            if (lbYes.Items.Count == 0)
                return;

            int i = lbYes.SelectedIndex;
            if (i == lbYes.Items.Count - 1)
                return;

            object obj = lbYes.Items[i];
            lbYes.Items.RemoveAt(i);
            lbYes.Items.Insert(i + 1, obj);
            lbYes.SetSelected(i + 1, true);
            flag = false;                
        }

        private void lbYes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //���� ���� �� ��������, ������ �� ����������
            if (flag)
            {
                return;
            }

            if (lbYes.SelectedIndex < 0)
            {
                btnUp.Enabled = false;
                btnDown.Enabled = false;
            }
            else
            {
                btnUp.Enabled = true;
                btnDown.Enabled = true;
            }
        }
    }
}