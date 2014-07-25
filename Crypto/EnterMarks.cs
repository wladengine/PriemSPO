using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using System.Transactions;

using WordOut;
using EducServLib;

using BaseFormsLib;

namespace Priem
{
    public partial class EnterMarks : BaseForm
    {  
        private Guid? _vedId;
        private bool _isModified;
        private string _examId;
        private string _dateExam;
        private string _facultyId;
        private string _studybasisId;       
        private bool _isLoad;
        private bool _isAdditional;

        private string _vedNum;
        private bool _isOral;
        private List<string> lstNumbers;        

        public EnterMarks(Guid? vedId, bool isOral)
        {
            InitializeComponent();
            this._vedId = vedId;
            
            _isModified = true;
            _isLoad = false;
            _isOral = isOral;
           
            InitControls();
            FillGridMarks();
        }        

        //�������������� ������������� ���������
        private void InitControls()
        {
            InitFocusHandlers();

            this.CenterToParent();
            //this.MdiParent = MainClass.mainform;
                      
            if (!_isModified)
            {
                dgvMarks.ReadOnly = true;
                btnSave.Text = "��������";
            }

            lblAdd.Text = string.Empty;

            using (PriemEntities context = new PriemEntities())
            {
                extExamsVed exVed = (from ev in context.extExamsVed
                                     where ev.Id == _vedId
                                     select ev).FirstOrDefault(); 

                _isAdditional = exVed.IsAddVed;
                _examId = exVed.ExamId.ToString();
                _dateExam = exVed.Date.ToString("yyyyMMdd");
                _facultyId = exVed.FacultyId.ToString();
                _studybasisId = exVed.StudyBasisId.ToString();
                _vedNum = exVed.Number.ToString();
                
                lblFaculty.Text += exVed.FacultyName;

                if (exVed.StudyBasisId == null)
                    lblStudyBasis.Text += "���";
                else
                    lblStudyBasis.Text += exVed.StudyBasisName;

                lblExam.Text += exVed.ExamName;
                lblDate.Text += exVed.Date.ToShortDateString();
                if (exVed.IsAddVed)
                    lblAdd.Text += "�������������� (" + exVed.AddCount.ToString() + ")";

                lblVedType.Text = _isOral ? "������ �����" : "���������� �����";
                               
                
                if (exVed.IsLoad)
                {
                    _isLoad = true;
                    _isModified = false;
                    dgvMarks.ReadOnly = true;
                    btnSave.Text = "��������";
                    btnSave.Enabled = false;
                    lblIsLoad.Text = "���������";
                    tbPersonVedNum.Enabled = false;                    
                }                      
            }       
        }

        private void FillGridMarks()
        {
            dgvMarks.Columns.Clear();
            lstNumbers = new List<string>();
           
            DataTable examTable = new DataTable();         

            DataColumn clm;
            clm = new DataColumn();
            clm.ColumnName = "PersonId";
            clm.ReadOnly = true;
            examTable.Columns.Add(clm);

            clm = new DataColumn();
            clm.ColumnName = "���";
            clm.ReadOnly = true;
            examTable.Columns.Add(clm);
            
            clm = new DataColumn();
            clm.ColumnName = "�����";
            clm.ReadOnly = true;
            examTable.Columns.Add(clm);           

            clm = new DataColumn();
            clm.ColumnName = "�����";            
            examTable.Columns.Add(clm);

            using (PriemEntities context = new PriemEntities())
            {
                var persMark = from evh in context.ExamsVedHistory
                               join pers in context.extPerson
                               on evh.PersonId equals pers.Id
                               where evh.ExamsVedId == _vedId
                               select new
                               {
                                   evh.ExamsVedId,
                                   pers.FIO,
                                   evh.PersonId,
                                   evh.PersonVedNumber,
                                   evh.Mark,
                                   evh.OralMark
                               };                

                foreach (var pm in persMark)
                {
                    DataRow newRow;
                    newRow = examTable.NewRow();
                    newRow["PersonId"] = pm.PersonId;
                    newRow["���"] = pm.FIO;
                    newRow["�����"] = pm.PersonVedNumber;
                    lstNumbers.Add(pm.PersonVedNumber.ToString());
                    newRow["�����"] = _isOral ? pm.OralMark : pm.Mark;
                    examTable.Rows.Add(newRow);
                }

                DataView dv = new DataView(examTable);
                dv.AllowNew = false;

                dgvMarks.DataSource = dv;
                dgvMarks.Columns["PersonId"].Visible = false;
                dgvMarks.Columns["���"].Visible = false;

                //if (!_forLoad && (bdc.IsCryptoMain() || bdc.IsSuperman()))
                //    dgvMarks.Columns["���"].Visible = true;

                dgvMarks.Columns["�����"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvMarks.Columns["�����"].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvMarks.Update();

                lblCount.Text = dgvMarks.RowCount.ToString();
            }
        }  

        private bool SaveMarks()
        {            
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {                       
                        string mark = null;

                        for (int i = 0; i < dgvMarks.Rows.Count; i++)
                        {
                            int persNum = int.Parse(dgvMarks["�����", i].Value.ToString());
                            Guid persId = new Guid(dgvMarks["PersonId", i].Value.ToString());
                            
                            if (dgvMarks["�����", i].Value != null)
                                mark = dgvMarks["�����", i].Value.ToString().Trim();

                            int? updatedMark;
                            int mrk;
                                                        
                            if(string.IsNullOrEmpty(mark))
                                updatedMark = null;
                            else if (int.TryParse(mark, out mrk) && mrk >= 0 && mrk < 101)
                                updatedMark = mrk;
                            else
                            {
                                dgvMarks.CurrentCell = dgvMarks["�����", i];
                                WinFormsServ.Error(dgvMarks["�����", i].Value.ToString() + ": ����������� ������� ������");
                                return false;
                            }

                            if (_isOral)
                                context.ExamsVedHistory_UpdateMarkOral(_vedId, persId, persNum, updatedMark);
                            else
                                context.ExamsVedHistory_UpdateMark(_vedId, persId, persNum, updatedMark);
                        }

                        transaction.Complete();                       
                        return true;
                    }
                }
            }

            catch (Exception exc)
            {
                WinFormsServ.Error("������ ���������� ������: \n" + exc.Message);
                return false;
            }             
        }
        
        private void btnCancel_Click(object sender, EventArgs e)        
        {            
            this.Close();            
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveClick();
        }

        private bool SaveClick()
        {
            if (btnSave.Enabled && btnSave.Visible)
            {
                if (_isModified)
                {
                    if (SaveMarks())
                    {
                        btnSave.Text = "��������";
                        _isModified = false;
                        dgvMarks.ReadOnly = true;
                        return true;
                    }
                    return false;
                }
                else
                {
                    btnSave.Text = "���������";
                    _isModified = true;
                    dgvMarks.ReadOnly = false;
                    dgvMarks.Columns["PersonId"].ReadOnly = true;
                    dgvMarks.Columns["�����"].ReadOnly = true;
                    return true;
                }                
            }
            return false;
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            try
            {
                WordDoc wd = new WordDoc(string.Format(@"{0}\Templates\EmptyTemplate.dot", Application.StartupPath));
                
                int colCount = 0;
                foreach (DataGridViewColumn clm in dgvMarks.Columns)
                {
                    if (clm.Visible)
                        colCount++;
                }

                wd.AddNewTable(2, colCount);
                TableDoc td = wd.Tables[0];

                // ������ �� �����
                int i = 0;
                foreach (DataGridViewColumn clm in dgvMarks.Columns)
                {
                    if (clm.Visible)
                    {
                        td[i, 0] = clm.HeaderText;
                        i++;
                    }
                }

                i = 1;
                int j;
                foreach (DataGridViewRow dgvr in dgvMarks.Rows)
                {
                    j = 0;
                    foreach (DataGridViewColumn clm in dgvMarks.Columns)
                    {
                        if (clm.Visible)
                        {
                            td[j, i] = dgvr.Cells[clm.Index].Value.ToString();
                            j++;
                        }
                    }
                    i++;
                    td.AddRow(1);
                }                
            }
            catch (Exception exc)
            {
                WinFormsServ.Error("������ ������ � Word: \n" + exc.Message);
            }
        }       
        
        private void EnterMarks_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isModified)
            {
                DialogResult res = MessageBox.Show("��������� ���������?", "����������", MessageBoxButtons.YesNoCancel);
                if (res == DialogResult.Yes)
                {                    
                    if (!SaveClick())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else if (res == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            } 
        }        

        private void EnterMarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                tbPersonVedNum.Text = "";
                tbPersonVedNum.Focus();
            }

        }

        private void dgvMarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                tbPersonVedNum.Text = "";
                tbPersonVedNum.Focus();
            }
            if (e.KeyCode == Keys.Enter)
            {
                AfterNumEnter(sender, e);
            }
        }         

        private void tbPersonVedNum_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 189)
            {
                AfterNumEnter(sender, e);  
            }          
        }

        private void AfterNumEnter(object sender, KeyEventArgs e)
        {           
            if (!tbPersonVedNum.Text.Trim().Contains("=="))
                return;                               

            string vedNum;
            vedNum = tbPersonVedNum.Text.Trim();
            vedNum = vedNum.Substring(0, vedNum.IndexOf("=="));           

            if (vedNum != _vedNum)
            {
                MessageBox.Show("������ ����������� ������ ���������!","������");
                tbPersonVedNum.Text = string.Empty;
                tbPersonVedNum.Focus();
                return;
            }

            string t = tbPersonVedNum.Text.Trim();
            t = t.Substring(t.IndexOf("==") + 2);
            t = t.Remove(t.Length - 1);

            if(!lstNumbers.Contains(t))
            {
                MessageBox.Show("����� �� ����������� ������ ���������!", "������");
                tbPersonVedNum.Text = string.Empty;
                tbPersonVedNum.Focus();
                return;
            }

            new AddMark(this, t).Show();
            tbPersonVedNum.Text = "";

            for (int i = 0; i < dgvMarks.Rows.Count; i++)
            {
                if (dgvMarks.Rows[i].Cells["�����"].Value.ToString() == t)
                {
                    dgvMarks.CurrentCell = dgvMarks["�����", i];
                    break;
                }
            }
        }
    }
}