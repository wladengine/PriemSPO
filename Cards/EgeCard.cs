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

using EducServLib;

namespace Priem
{
    public partial class EgeCard : BookCard
    {  
        private Guid? _personId;
        private bool _isReadOnly;

        public EgeCard(Guid? personId)
            : this(null, personId, false)
        {
        }
        
        // ����������� ����� 
        public EgeCard(string egeId, Guid? personId, bool isReadOnly)
        {
            InitializeComponent();
            _Id = egeId;
            _personId = personId;
            _isReadOnly = isReadOnly;
            _isModified = false;
            _title = "������������� ���";

            InitControls();
        }

        protected Guid? GuidId
        {
            get { return new Guid(_Id); }
        }
       
        protected override void ExtraInit()
        {
            base.ExtraInit();
            _tableName = "ed.EgeCertificate";
            this.MdiParent = null;

           
            lblIsImported.Visible = false;

            if (_Id == null)
            {
                this.tbNumber.TextChanged += new System.EventHandler(this.tbNumber_TextChanged);
                InitGridNew();
            }

            if (MainClass.IsPasha() || MainClass.IsOwner())
            {
                btnSetStatusPasha.Visible = btnSetStatusPasha.Enabled = true;
                tbCommentFBSPasha.Visible = true;
            }
            else
            {
                btnSetStatusPasha.Visible = btnSetStatusPasha.Enabled = false;
                tbCommentFBSPasha.Visible = false;
            }
        }

        protected override void FillCard()
        {
            if (_Id == null)
                return;
            
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    EgeCertificate egeCert = (from ec in context.EgeCertificate                                             
                                              where ec.Id == GuidId
                                              select ec).FirstOrDefault();

                    if (egeCert == null)
                        return;

                    Number = egeCert.Number;
                    PrintNumber = egeCert.PrintNumber;
                    Year = egeCert.Year;
                    NewFIO = egeCert.NewFIO;
                    FBSStatus = (from fs in context.FBSStatus where fs.Id == egeCert.FBSStatusId select fs.Name).FirstOrDefault();
                    FBSComment = egeCert.FBSComment;
                    IsImported = egeCert.IsImported;
                    NoNumber = egeCert.NoNumber ?? false;

                    //���������� ����� � ��������
                    DataTable examTable = new DataTable();

                    DataColumn clm;
                    clm = new DataColumn();
                    clm.ColumnName = "�������";
                    clm.ReadOnly = true;
                    examTable.Columns.Add(clm);

                    clm = new DataColumn();
                    clm.ColumnName = "ExamId";
                    clm.ReadOnly = true;
                    clm.DataType = typeof(int);
                    examTable.Columns.Add(clm);

                    clm = new DataColumn();
                    clm.ColumnName = "�����";
                    clm.DataType = typeof(int);
                    examTable.Columns.Add(clm);

                    clm = new DataColumn();
                    clm.ColumnName = "��������";
                    clm.DataType = typeof(bool);
                    examTable.Columns.Add(clm);

                    clm = new DataColumn();
                    clm.ColumnName = "MarkId";
                    clm.DataType = typeof(Guid);
                    examTable.Columns.Add(clm);

                    clm = new DataColumn();
                    clm.ColumnName = "���������";
                    clm.DataType = typeof(bool);
                    examTable.Columns.Add(clm);

                    IEnumerable<EgeExamName> examNames = from en in context.EgeExamName
                                            select en;

                    foreach (EgeExamName eName in examNames)
                    {
                        DataRow newRow;
                        newRow = examTable.NewRow();
                        newRow["�������"] = eName.Name;
                        newRow["ExamId"] = eName.Id;
                        examTable.Rows.Add(newRow);
                    }

                    // ������
                    IEnumerable<EgeMark> egeMarks = from em in context.EgeMark
                                                    where em.EgeCertificateId == GuidId
                                                    select em;
                    
                    foreach (EgeMark eMark in egeMarks)
                    {
                        for (int i = 0; i < examTable.Rows.Count; i++)
                        {
                            if (examTable.Rows[i]["ExamId"].ToString() == eMark.EgeExamNameId.ToString())
                            {
                                examTable.Rows[i]["MarkId"] = eMark.Id;
                                examTable.Rows[i]["�����"] = eMark.Value;
                                examTable.Rows[i]["���������"] = eMark.IsAppeal;
                                examTable.Rows[i]["��������"] = eMark.IsCurrent;
                            }
                        }
                    }

                    DataView dv = new DataView(examTable);
                    dv.AllowNew = false;

                    dgvExams.DataSource = dv;
                    dgvExams.Columns["�����"].ValueType = typeof(int);                    
                    dgvExams.Columns["ExamId"].Visible = false;
                    dgvExams.Columns["MarkId"].Visible = false;
                    dgvExams.Update();
                }          
            }
            catch (DataException de)
            {
                WinFormsServ.Error("������ ��� ���������� ����� " + de.Message);
            }

        }

        private void InitGridNew()
        { 
            using (PriemEntities context = new PriemEntities())
            {
                var source = from en in context.EgeExamName
                             select new
                             {
                                 en.Id,
                                 ������� = en.Name,
                                 ExamId = en.Id
                             };

                dgvExams.DataSource = source;
                
                dgvExams.Columns["Id"].Visible = false;
                dgvExams.Columns["ExamId"].Visible = false;
                dgvExams.Columns["�������"].ReadOnly = true;
                
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.Name = "�����";                
                col.HeaderText = "�����";
                col.ValueType = typeof(int);
                dgvExams.Columns.Add(col);

                DataGridViewCheckBoxColumn cl1 = new DataGridViewCheckBoxColumn();
                cl1.Name = "��������";
                cl1.HeaderText = "��������";
                dgvExams.Columns.Add(cl1);
                
                DataGridViewCheckBoxColumn cl = new DataGridViewCheckBoxColumn();
                cl.Name = "���������";
                cl.HeaderText = "���������";               
                dgvExams.Columns.Add(cl);

                dgvExams.Update();
            }
        }

        protected override void SetReadOnlyFieldsAfterFill()
        {
            base.SetReadOnlyFieldsAfterFill();

            if (_isReadOnly)
                btnSaveChange.Enabled = false;

            if (_Id == null)
            {
                //tbPrintNumber.Enabled = false;
                tbYear.Enabled = false;
                tbFBSComment.Enabled = false;
                tbFBSStatus.Enabled = false;
            }
        }

        protected override void  SetAllFieldsEnabled()
        {
            base.SetAllFieldsEnabled();

            this.tbNumber.TextChanged += new System.EventHandler(this.tbNumber_TextChanged);
            
            tbYear.Enabled = false;
            tbFBSComment.Enabled = false;
            tbFBSStatus.Enabled = false;

            tbName.Enabled = chbNewFIO.Checked;
            tbSecondName.Enabled = chbNewFIO.Checked;
            tbSurname.Enabled = chbNewFIO.Checked;

            if (MainClass.IsPasha() || MainClass.IsOwner())            
                btnSetStatusPasha.Enabled = tbCommentFBSPasha.Enabled = true; 
            else           
                btnSetStatusPasha.Enabled = false;            

            //if (!MainClass.IsPasha())
            //{
            //    SetAllFieldsNotEnabled();
            //    btnSaveChange.Enabled = true;
                
            //    dgvExams.Enabled = true;
            //    dgvExams.Columns["���������"].ReadOnly = true;
            //    dgvExams.Columns["�����"].ReadOnly = true;
            //}
        }

        protected override void SetAllFieldsNotEnabled()
        {
            base.SetAllFieldsNotEnabled();

            if (MainClass.IsPasha() || MainClass.IsOwner())
                btnSetStatusPasha.Enabled = tbCommentFBSPasha.Enabled = true;
            else
                btnSetStatusPasha.Enabled = false;  
        }

        private bool CheckEge()
        {
            try
            {
                for (int i = 0; i < dgvExams.Rows.Count; i++)
                {
                    if ((dgvExams["�����", i].Value != null) && dgvExams["�����", i].Value.ToString().Trim() != "")
                    {
                        string balls = dgvExams["�����", i].Value.ToString().Trim();
                        if (!Regex.IsMatch(balls, @"^\d{1,3}$"))
                        {
                            WinFormsServ.Error("������������ �������� ������");
                            return false;
                        }
                        else
                        {
                            if (int.Parse(balls) > 100)
                            {
                                WinFormsServ.Error("������������ �������� ������");
                                return false;
                            }
                        }

                        // �������� ��� ��� ���� ��� �� ������ ��������
                        // ���� ������, ��� ����� ������, ��� �����, � ����� ��� �������� ������� ������������ �� ������

                        //int res = int.Parse(_bdc.GetStringValue(string.Format("SELECT Count(*) FROM EgeMark INNER JOIN EgeCertificate ON EgeMark.EgeCertificateId = EgeCertificate.Id WHERE EgeMark.EgeExamNameId = {0} {1} AND EgeCertificate.PersonId = '{2}'", dgvExams["ExamId", i].Value.ToString(), _Id == null ? "" : " AND EgeCertificate.Id <> '" + _Id + "'", _personId)));
                        //if (res > 0)
                        //{
                        //    int value = int.Parse(_bdc.GetStringValue(string.Format("SELECT MAX(Value) FROM EgeMark INNER JOIN EgeCertificate ON EgeMark.EgeCertificateId = EgeCertificate.Id WHERE EgeMark.EgeExamNameId = {0} {1} AND EgeCertificate.PersonId = '{2}'", dgvExams["ExamId", i].Value.ToString(), _Id == null ? "" : " AND EgeCertificate.Id <> '" + _Id + "'", _personId)));
                        //    if (value >= int.Parse(balls))
                        //    {
                        //        dgvExams.Rows[i].Selected = true;
                        //        WinFormsServ.Error("����� �� ���� ������� ��� ���� � ������ ����������� ��� ����� �����������. \r\n� ������ ����������� ����� ���� � ������� �� �����.");
                        //        dgvExams["�����", i].Value = "";
                        //        //return false;
                        //    }
                        //    else
                        //    {
                        //        WinFormsServ.Error("����� �� ���� ������� ��� ���� � ������ ����������� ��� ����� �����������. \r\n�� ������ ����������� ����� ����, ������� ����� �������. ������ � ������ ����������� ����� �������.");
                        //        // ����� ����� ������� ������ � ������� �
                        //        string sporId = _bdc.GetStringValue(string.Format("SELECT EgeMark.Id FROM EgeMark INNER JOIN EgeCertificate ON EgeMark.EgeCertificateId = EgeCertificate.Id WHERE EgeMark.EgeExamNameId = {0} {1} AND EgeCertificate.PersonId = '{2}'", dgvExams["ExamId", i].Value.ToString(), _Id == null ? "" : " AND EgeCertificate.Id <> '" + _Id + "'", _personId));
                        //        _bdc.ExecuteQuery("DELETE FROM EgeMark WHERE Id = '" + sporId + "'");
                        //    }
                        //}
                    }
                }

                return true; 
            }
            catch (Exception de)
            {
                WinFormsServ.Error("������ ���������� ������" + de.Message);
                return false;
            }
        }

        protected override bool CheckFields()
        {
            if (Number.Length <= 0 && !NoNumber)
            {
                epErrorInput.SetError(tbNumber, "������� ����� ������������");
                return false;
            }
            else
                epErrorInput.Clear();

            if (chbNewFIO.Checked)
            {
                if (tbSurname.Text.Trim().Length == 0 || tbName.Text.Trim().Length == 0)
                {
                    epErrorInput.SetError(tbSurname, "������� ���");
                    return false;
                }
                else
                    epErrorInput.Clear();
            }
            else
                epErrorInput.Clear();

            if (!IsMatchEgeNumber() && !NoNumber)
            {
                WinFormsServ.Error("����� ������������� �� ������������� ������� **-*********-**, ��� ��������� ��� ����� - ��� ��������� �������������.");
                return false;
            }

            using (PriemEntities context = new PriemEntities())
            {
                // �������� �� ���������� ���������� ������������   
                int rez;
                if (_Id == null)
                    rez = (from ec in context.EgeCertificate
                           where ec.Number == Number
                           select ec).Count();
                else
                    rez = (from ec in context.EgeCertificate
                           where ec.Number == Number && ec.Id != GuidId
                           select ec).Count();

                if (rez > 0 && !NoNumber)
                {
                    WinFormsServ.Error("������������� � ����� ������� ��� ���� � ����!");
                    return false;
                }
            }

            if (!CheckEge())
                return false;

            return true;
        }                

        public bool IsMatchEgeNumber()
        {
            if (Regex.IsMatch(Number, @"^\d{2}-\d{9}-(08|09|10|11|12|00)$")) 
                return true;
            else
                return false;
        }

        protected override void InsertRec(PriemEntities context, System.Data.Objects.ObjectParameter idParam)
        {
            context.EgeCertificate_Insert(Number, PrintNumber, Year, _personId, NewFIO, false, idParam);
        }

        protected override void UpdateRec(PriemEntities context, Guid id)
        {
            context.EgeCertificate_Update(Number, PrintNumber, Year, NewFIO, id);
        }

        protected override void SaveManyToMany(PriemEntities context, Guid id)
        { 
            //��������� ������ ������
            if (_Id == null)
            {
                for (int i = 0; i < dgvExams.Rows.Count; i++)
                {
                    if (dgvExams["�����", i].Value != null && dgvExams["�����", i].Value.ToString() != "")
                    {
                        context.EgeMark_Insert((int)dgvExams["�����", i].Value, (int)dgvExams["ExamId", i].Value, id, Util.ToBool(dgvExams["���������", i].Value), Util.ToBool(dgvExams["��������", i].Value));
                    }
                }
            }
            else
            {
                for (int i = 0; i < dgvExams.Rows.Count; i++)
                {
                    if (dgvExams["MarkId", i].Value != null && dgvExams["MarkId", i].Value.ToString() != "")
                    {
                        if (dgvExams["�����", i].Value == null || dgvExams["�����", i].Value.ToString() == "")
                            context.EgeMark_Delete((Guid)dgvExams["MarkId", i].Value);
                        else
                        {
                            context.EgeMark_Update((int)dgvExams["�����", i].Value, (int)dgvExams["ExamId", i].Value, Util.ToBool(dgvExams["���������", i].Value), (Guid)dgvExams["MarkId", i].Value);
                            context.EgeMark_UpdateCurMark(Util.ToBool(dgvExams["��������", i].Value), (Guid)dgvExams["MarkId", i].Value);
                        }
                    }
                    else
                    {
                        if (dgvExams["�����", i].Value != null && dgvExams["�����", i].Value.ToString() != "")
                        {
                            context.EgeMark_Insert((int)dgvExams["�����", i].Value, (int)dgvExams["ExamId", i].Value, id, Util.ToBool(dgvExams["���������", i].Value), Util.ToBool(dgvExams["��������", i].Value));
                        }
                    }
                }
            }
        }
        
        protected override void OnSave()
        {
            base.OnSave();
            MainClass.DataRefresh();            
        }

        private void chbNewFIO_CheckedChanged(object sender, EventArgs e)
        {
            if (chbNewFIO.Checked)
            {
                tbSurname.Enabled = true;
                tbName.Enabled = true;
                tbSecondName.Enabled = true;
            }
            else
            {
                tbSurname.Text = string.Empty;
                tbName.Text = string.Empty;
                tbSecondName.Text = string.Empty;                
                tbSurname.Enabled = false;
                tbName.Enabled = false;
                tbSecondName.Enabled = false;
            }
        }
       
        private void tbNumber_TextChanged(object sender, EventArgs e)
        {
            if (Number.Length == 15)
                Year = "20" + Number.Substring(Number.Length - 2, 2);
            else
                Year = "";
        }

        protected override void CloseCardAfterSave()
        {
            if (!_isModified)
                this.Close();
        }

        private void btnSetStatusPasha_Click(object sender, EventArgs e)
        {
            if (MainClass.IsPasha() || MainClass.IsOwner())
            {
                if (_Id == null)
                    return;

                using (PriemEntities context = new PriemEntities())
                {
                    var cert = (from ec in context.EgeCertificate
                                where ec.Id == GuidId
                                select ec).FirstOrDefault();

                    if (cert != null)
                    {
                        if (MessageBox.Show(string.Format("���������� ������ '���������' ��� ������������� {0}?", cert.Number), "��������", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            context.EgeCertificate_UpdateFBSStatus(4, tbCommentFBSPasha.Text.Trim(), cert.Id);
                            MessageBox.Show("���������");

                            var c = (from ec in context.EgeCertificate
                                     where ec.Id == GuidId
                                     select ec).FirstOrDefault();

                            FBSStatus = (from fs in context.FBSStatus where fs.Id == c.FBSStatusId select fs.Name).FirstOrDefault();
                            FBSComment = c.FBSComment;
                        }
                    }
                    else
                    {
                        MessageBox.Show("��� ������������, ��������������� ���������");
                    }
                }
            }       
        }      
    }
}