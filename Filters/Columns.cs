using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using EducServLib;
using BDClassLib;
using PriemLib;

namespace Priem
{
    public partial class Columns : Form
    {
        private FormFilter own;
        private DBPriem _bdc;
        private string _fac;
        private SortedList<string,string> _columnList;

        //�����������
        public Columns(FormFilter owner, string facId)
        {
            InitializeComponent();
            this.CenterToParent();
            own = owner;
            _fac = facId;
            _bdc = MainClass.Bdc;
            _columnList = FillColumns();

            foreach (DataGridViewColumn col in own.Dgv.Columns)
                if (col.Visible)
                    try
                    {
                        lbYes.Items.Add(_columnList[col.Name]);
                    }
                    catch { }


            foreach(string li in _columnList.Values)
            {
                try
                {
                    if (!lbYes.Items.Contains(li))
                        lbNo.Items.Add(li);
                }
                catch
                {
                }
            }
        }

        private SortedList<string, string> FillColumns()
        {
            SortedList<string, string> list = new SortedList<string, string>();
            list.Add("���_�����", "���. �����");
            list.Add("��_�����", "�����. �����");
            list.Add("�������", "�������");
            list.Add("���", "���");
            list.Add("��������", "��������");
            list.Add("���", "���");
            list.Add("����_��������", "���� ��������");
            list.Add("�����_��������", "����� ��������");
            list.Add("���_��������", "��� ��������");
            list.Add("�����_��������", "����� ��������");
            list.Add("�����_��������", "����� ��������");
            list.Add("���_�����_�������", "��� ����� �������");
            list.Add("����_������_��������", "���� ������ ��������");
            list.Add("���_�������������_��������", "��� ������������� (�������)");
            list.Add("������_���_�������", "������ ��� (�������)");
            list.Add("���_�������", "���");
            list.Add("�������", "�������");
            list.Add("Email", "E-mail");
            list.Add("�����_�����������", "����� �����������");
            list.Add("�����_����������", "����� ����������");
            list.Add("������", "������");
            list.Add("�����������", "�����������");
            list.Add("������", "������");
            list.Add("�������������_���������_�����������", "������������� ��������� �� ����� �����������");
            list.Add("������_�����������_��_���������", "������ ����������� �� ���������");
            list.Add("���������_��������_�����������", "���������, �������� �����������");           
            list.Add("��_����", "��. ���� ���������");           
            list.Add("������_�����_����_�����", "������ ��������� ����.�����������");            
            list.Add("��������_��_���������", "�������� ��. ���������"); 
            list.Add("�������������_���������_��������", "������������� ��������� �� ����� ��������");        
            list.Add("��������_�_�������", "�������� � �������");
            list.Add("�������������", "������������� � ����������");
            list.Add("�����������_�����������", "����������� �����������");
            list.Add("�����_������", "����� ������");
            list.Add("�����_�������", "����� �������� ������");
            list.Add("�������_���", "��� ��������");
            list.Add("�������_����_������", "������� ���� ������");
            list.Add("������_��������", "������ ��������");
            list.Add("���������", "���������");
            list.Add("���_��������", "���. ��������");
            list.Add("������_�_���������", "������ � ���������");
            
            list.Add("���������", "���������");
            list.Add("�������", "�������");
            list.Add("������_���", "������ ���.");
            list.Add("������_���������", "������ ���������");
            list.Add("����_��������_���", "���� �������� ���."); 
            list.Add("����_������_���", "���� ������ ���.");
            list.Add("������_���������_���", "����� ��������� ��������� �� �����������");            
            list.Add("�����_����������", "����� ����������");
                        
            list.Add("���������", "���������");
            list.Add("�����������", "�����������");
            list.Add("�����_���������", "��������������� ���������");
            list.Add("���_�����������", "��� �����������");
            list.Add("�����_��������", "����� ��������");
            list.Add("������_��������", "������ ��������");            
            list.Add("���_��������", "��� ��������");
            list.Add("���_���_��������", "���. ��� ��������");
           
            list.Add("������", "������");
            list.Add("����������", "����������");
            list.Add("��������������", "��������������");
            list.Add("����������", "����������");
            list.Add("�������", "�������");
            list.Add("��.����.", "��.����.");
            list.Add("�������", "�������");
            list.Add("���.-������", "���.-������");
            list.Add("���.�����������", "���.�����������");
            list.Add("����_�_����", "������ ������� ���� � ����");
            list.Add("����_������", "����. ������ �� ����");

            list.Add("�����_�������_�_����������", "����� ������� � ����������");
            list.Add("����_�������_�_����������", "���� ������� � ����������");
            list.Add("�����_�������_�_����������_������", "����� ������� � ���������� (������)");
            list.Add("����_�������_�_����������_������", "���� ������� � ���������� (������)");
                                  
            string exemQuery = null;

            if (!string.IsNullOrEmpty(_fac) && _fac != "0")
            {
                exemQuery = string.Format("SELECT DISTINCT ed.extExamInEntry.ExamId, ed.extExamInEntry.ExamName AS Name FROM ed.extExamInEntry Where FacultyId={0}", _fac);
            }
            else
            {
                exemQuery = "SELECT DISTINCT ed.extExamInEntry.ExamId, ed.extExamInEntry.ExamName AS Name FROM ed.extExamInEntry";
            }
            
            DataSet dsExams = _bdc.GetDataSet(exemQuery);
            foreach (DataRow dr in dsExams.Tables[0].Rows)
                list.Add(dr["Name"].ToString(), "������� "+dr["Name"].ToString());


            if (MainClass.dbType == PriemType.PriemMag)
            {
                list.Add("��������", "������� ������");                
                list.Add("�������", "������������ ���������");
                list.Add("�����������_����������", "�������_�����������(�������������)");
                list.Add("���_�������", "��� �������");              

                list.Add("�����_�������", "����� �������");
                list.Add("�����_�������", "����� �������");

                list.Add("������������", "������������");
                list.Add("�����_�����������_�����������_���", "����� ����������� ����������� (��� ���)");
            }
            else
            {
                list.Add("���������_���_��", "��������� ��� ��� � ��");
                list.Add("���������_����", "����������� ���������");
                list.Add("���������_�����", "������������ ���������");

                list.Add("�����_��_���������", "����� ��.���������");
                list.Add("���_��_���������", "��� ��.���������");
                list.Add("��������", "��������");
                list.Add("�����_�����", "����� �����");
                list.Add("�����_���", "����� ���������");
                list.Add("�����_���", "����� ���������");
                list.Add("������_���������", "������ ���������");                

                list.Add("�����_�������", "����� �������");
                list.Add("�����_�������", "����� �������");

                list.Add("���_�������", "��� �������");

                list.Add("�������_����_���", "������� ���� ���������");               
                list.Add("������_���", "������ ���");
                list.Add("������_����������_���", "������ ���������� ���");

                list.Add("�������", "�������");

                list.Add("�������������_���_2011", "������������� ��� 2011 ����");
                list.Add("�������������_���_2012", "������������� ��� 2012 ����");
                list.Add("���������_����-��_���_2012", "��������� ����-�� ��� 2012 ����");

                list.Add("���_����.��", "��� ����.��");
                list.Add("���_�����.����", "��� �����.����");
                list.Add("���_����������", "��� ����������");
                list.Add("���_������", "��� ������");
                list.Add("���_�����", "��� �����");
                list.Add("���_��������", "��� ��������");
                list.Add("���_�������", "��� �������");
                list.Add("���_���������", "��� ���������");
                list.Add("���_�����.����", "��� �����.����");
                list.Add("���_�����.����", "��� �����.����");
                list.Add("���_��������������", "��� ��������������");
                list.Add("���_����������", "��� ����������");
                list.Add("���_�����.����", "��� �����.����");
                list.Add("���_�����������", "��� �����������");

                list.Add("��������_�������", "A������� �������");
                list.Add("��������_����_����", "A������� ����. ����");
                list.Add("��������_����������", "A������� ����������");
                list.Add("��������_��������", "A������� ��������");
                list.Add("��������_���������_���", "A������� ��������� ��������");
                list.Add("��������_��_�������", "A������� �������� �������");
                list.Add("��������_���������", "A������� ���������");
                list.Add("��������_���������", "A������� ���������");
                list.Add("��������_�����������", "A������� �����������");
                list.Add("��������_�������_���", "A������� ������� � �������� �����-����������");
                list.Add("��������_���_������", "A������� ������� ������");
                list.Add("��������_����������", "A������� ����������");
                list.Add("��������_�������_���_��������", "A������� ������� �������������� ��������");
                list.Add("��������_��������������", "A������� ��������������");
                list.Add("��������_���", "A������� ���");
                list.Add("��������_�����_����", "A������� ������� ����");
                list.Add("��������_����������", "A������� ����������");
                list.Add("��������_������", "A������� ������");
                list.Add("��������_���_��������", "A������� ���������� ��������");
                list.Add("��������_�����", "A������� �����");
                list.Add("��������_��������", "A������� ��������");
                list.Add("��������_��������_����", "A������� �������� ����");
                list.Add("��������_���������_����", "A������� ��������� ����");
                list.Add("��������_�����������_����", "A������� ����������� ����");
                list.Add("��������_�����������_����", "A������� ����������� ����");                

                // ���������
                //list.Add("������������", "������������ ���������");
                //list.Add("�������������", "������������� ���������");
                //list.Add("������������", "������������ ���������");
                //list.Add("������������", "������������ ���������");
                //list.Add("�����", "����� ���������");
                //list.Add("����������", "��������� ����������");  

                list.Add("�������_�������", "������� �������"); 

            }

            return list;
        }
        
        //������ ��
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (own is ListAbit)
            {                
                MainClass._config.ClearColumnListAbit();

                foreach (string li in lbYes.Items)
                {
                    MainClass._config.AddColumnNameAbit(_columnList.Keys[_columnList.IndexOfValue(li)]);
                }
            }
            else if (own is ListPersonFilter)
            {
                MainClass._config.ClearColumnListPerson();

                foreach (string li in lbYes.Items)
                {
                    MainClass._config.AddColumnNamePerson(_columnList.Keys[_columnList.IndexOfValue(li)]);
                }
            }

            own.UpdateDataGrid();
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
            if (lbYes.Items.Count == 0)
                return;

            int i = lbYes.SelectedIndex;
            if (i == 0)
                return;

            object obj = lbYes.Items[i];
            lbYes.Items.RemoveAt(i);
            lbYes.Items.Insert(i - 1, obj);
            lbYes.SetSelected(i - 1, true);
        }

        //������� ����
        private void btnDown_Click(object sender, EventArgs e)
        {
            if (lbYes.Items.Count == 0)
                return;

            int i = lbYes.SelectedIndex;
            if (i == lbYes.Items.Count - 1)
                return;

            object obj = lbYes.Items[i];
            lbYes.Items.RemoveAt(i);
            lbYes.Items.Insert(i + 1, obj);
            lbYes.SetSelected(i + 1, true);
        }
    }
}