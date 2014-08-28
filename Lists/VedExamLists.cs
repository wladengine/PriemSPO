using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using EducServLib;
using BDClassLib;
using WordOut;
using BaseFormsLib;
using PriemLib;

namespace Priem
{
    public partial class VedExamLists : BookList
    {       
        //�����������
        public VedExamLists()
        {
            InitializeComponent();
          
            Dgv = dgvVed;        
           _title = "��������� ������ ��������������� ������";

            InitControls();                     
            InitHandlers();
        }

        protected override void ExtraInit()
        {
            base.ExtraInit();
            Dgv.ReadOnly = false;

            using (PriemEntities context = new PriemEntities())
            {
                ComboServ.FillCombo(cbFaculty, HelpClass.GetComboListByTable("ed.qFaculty", "ORDER BY Acronym"), false, false);
                ComboServ.FillCombo(cbStudyBasis, HelpClass.GetComboListByTable("ed.StudyBasis", "ORDER BY Name"), false, true);
                cbStudyBasis.SelectedIndex = 0;

                ComboServ.FillCombo(cbStudyForm, HelpClass.GetComboListByTable("ed.StudyForm", "ORDER BY Id"), false, true);
                cbStudyForm.SelectedIndex = 0;

                FillLicenseProgram();

                ComboServ.FillCombo(cbCompetition, HelpClass.GetComboListByTable("ed.Competition"), false, true);

                UpdateDataGrid();

                if (MainClass.IsFacMain() || MainClass.IsOwner())
                    btnLists.Enabled = true;
                else
                    btnLists.Enabled = false;
            }
        }

        public int? FacultyId
        {
            get { return ComboServ.GetComboIdInt(cbFaculty); }
            set { ComboServ.SetComboId(cbFaculty, value); }
        }

        public int? LicenseProgramId
        {
            get { return ComboServ.GetComboIdInt(cbLicenseProgram); }
            set { ComboServ.SetComboId(cbLicenseProgram, value); }
        }

        public int? StudyBasisId
        {
            get { return ComboServ.GetComboIdInt(cbStudyBasis); }
            set { ComboServ.SetComboId(cbStudyBasis, value); }
        }

        public int? StudyFormId
        {
            get { return ComboServ.GetComboIdInt(cbStudyForm); }
            set { ComboServ.SetComboId(cbStudyForm, value); }
        }

        public int? CompetitionId
        {
            get { return ComboServ.GetComboIdInt(cbCompetition); }
            set { ComboServ.SetComboId(cbCompetition, value); }
        }
       
       
        private void FillLicenseProgram()
        {
            using (PriemEntities context = new PriemEntities())
            {
                List<KeyValuePair<string, string>> lst = ((from ent in MainClass.GetEntry(context)
                                                           where ent.FacultyId == FacultyId && ent.StudyFormId == StudyFormId
                                                           select new
                                                           {
                                                               Id = ent.LicenseProgramId,
                                                               Name = ent.LicenseProgramName
                                                           }).Distinct()).ToList().Select(u => new KeyValuePair<string, string>(u.Id.ToString(), u.Name)).ToList();

                ComboServ.FillCombo(cbLicenseProgram, lst, false, false);
                cbLicenseProgram.SelectedIndex = 0;
            }
        }
        
        //������������� ������������ ����������
        public override void  InitHandlers() 
        {
            cbFaculty.SelectedIndexChanged += new EventHandler(cbFaculty_SelectedIndexChanged);
            cbStudyForm.SelectedIndexChanged += new EventHandler(cbStudyForm_SelectedIndexChanged);
            cbStudyBasis.SelectedIndexChanged += new EventHandler(cbStudyBasis_SelectedIndexChanged);
            cbLicenseProgram.SelectedIndexChanged += new EventHandler(cbLicenseProgram_SelectedIndexChanged);
            cbCompetition.SelectedIndexChanged += new EventHandler(cbCompetition_SelectedIndexChanged);                             
        }

        void cbCompetition_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbLicenseProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbStudyBasis_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();            
        }

        void cbStudyForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillLicenseProgram();
            UpdateDataGrid();
        }

        void cbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillLicenseProgram();
            UpdateDataGrid();
        }       

        protected override void OpenCard(string id, BaseFormEx formOwner, int? index)
        {
            MainClassCards.OpenCardAbit(id, this, dgvVed.CurrentRow.Index);
        }

        //���������� �����
        public override void UpdateDataGrid()
        {
            string sFilters = GetFilterString();

            string _query = @"SELECT ed.extAbit.Id as Id, RegNum as ���_�����, 
                        FIO as ���, 
                        ed.Competition.Name AS '��� ��������', ed.extEnableProtocol.Number AS '��������_�_�������' 
                        FROM ed.extAbit LEFT JOIN ed.extEnableProtocol ON ed.extEnableProtocol.AbiturientId = ed.extAbit.Id 
                        LEFT JOIN ed.Competition ON ed.extAbit.CompetitionId = ed.Competition.Id 
                        WHERE ed.extAbit.BackDoc = 0 AND ed.extAbit.Id IN (SELECT ed.extProtocol.AbiturientId FROM ed.extProtocol 
                        WHERE ed.extProtocol.ProtocolTypeId = 1 AND ed.extProtocol.IsOld = 0)";           

            DataSet ds = _bdc.GetDataSet(_query + " " + sFilters + " ORDER BY ���");
            DataTable dt = new DataTable();
            dt = ds.Tables[0];
           
            DataColumn clm;
            clm = new DataColumn();
            clm.ColumnName = "������";
            clm.DataType = typeof(bool);
            dt.Columns.Add(clm);
                       
            DataView dv = new DataView(dt);
            dv.AllowNew = false;

            dgvVed.DataSource = dv;
            dgvVed.Columns["������"].DisplayIndex = 0;
            dgvVed.Columns["Id"].Visible = false;
            dgvVed.ReadOnly = false;
            dgvVed.Columns["���"].ReadOnly = true; 
            dgvVed.Columns["��� ��������"].ReadOnly = true;
            dgvVed.Columns["���_�����"].ReadOnly = true; 
            dgvVed.Columns["������"].ReadOnly = false;
            dgvVed.Update();

            lblCount.Text = dgvVed.RowCount.ToString();
        }        

        private string GetFilterString()
        {
            string s = MainClass.GetStLevelFilter("ed.extAbit");
            
            //���������� ���������       
            if (FacultyId != null)
                s += string.Format(" AND ed.extAbit.FacultyId = {0}", FacultyId);           
            
            //���������� ����� ��������           
            if (StudyFormId != null)
                s += string.Format(" AND ed.extAbit.StudyFormId = {0}", StudyFormId);

            //���������� ������ �������� 
            if (StudyBasisId != null)
                s += string.Format(" AND ed.extAbit.StudyBasisId = {0}", StudyBasisId);

            //���������� ��� ��������            
            if (CompetitionId != null)
                s += string.Format(" AND ed.extAbit.CompetitionId = {0}", CompetitionId);  

            //���������� �����������
            if (LicenseProgramId != null)
                s += string.Format(" AND ed.extAbit.LicenseProgramId = {0}", LicenseProgramId); 

            return s;
        } 
       
        private void VedExamLists_Activated(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        // ������ ������
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                WordDoc wd = new WordDoc(string.Format(@"{0}\ExamVed.dot", MainClass.dirTemplates));
                TableDoc td = wd.Tables[0];

                //����������
                string sFac = cbFaculty.Text.ToString().ToLower();
                if (sFac.CompareTo("���") == 0)
                    sFac = "���� ����������� ";
                else
                {                   
                    if (FacultyId == 3)
                        sFac = "������ ����� ����������� ";
                    else
                        sFac = sFac.Replace("���", "���� ").Replace("��", "��� ").Replace("��������", "���������");
                }

                string sForm = cbStudyForm.Text.ToString().ToLower();
                if (sForm.CompareTo("���") == 0)
                    sForm = " ���� ���� �������� ";
                else
                    sForm = sForm.Replace("��", "��").Replace("��", "��") + " ����� �������� ";
                wd.Fields["Faculty"].Text = sFac;
                wd.Fields["Section"].Text = sForm;

                int i = 1;

                // ������ �� �����
                foreach (DataGridViewRow dgvr in dgvVed.Rows)
                {
                    if (chbPrintChecked.Checked)
                    {
                        if (dgvr.Cells["������"].Value.ToString() == "True")
                        {
                            td[0, i] = i.ToString();
                            td[1, i] = dgvr.Cells["���_�����"].Value.ToString();
                            td[2, i] = dgvr.Cells["���"].Value.ToString();
                            td[3, i] = dgvr.Cells["��� ��������"].Value.ToString();
                            td.AddRow(1);
                            i++;
                        }
                    }
                    else
                    {
                        td[0, i] = i.ToString();
                        td[1, i] = dgvr.Cells["���_�����"].Value.ToString();
                        td[2, i] = dgvr.Cells["���"].Value.ToString();
                        td[3, i] = dgvr.Cells["��� ��������"].Value.ToString();
                        td.AddRow(1);
                        i++;
                    }                    
                }
            }
            catch (Exception exc)
            {
                WinFormsServ.Error("������ ������ � Word: \n" + exc.Message);
            }
        }

        private void btnLists_Click(object sender, EventArgs e)
        {
            if (MainClass.RightsFacMain())
            {
                foreach (DataGridViewRow dgvr in dgvVed.Rows)
                {
                    if (dgvr.Cells["������"].Value.ToString() == "True")
                    {
                        Guid abitId = new Guid(dgvr.Cells["Id"].Value.ToString());
                        Print.PrintExamListWord(abitId, false/*true*/);
                    }
                }
            }
            else
                WinFormsServ.Error("���������� �������� ��������������� ������, ������������ ����");            

        }        

        private void chbAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dgvr in dgvVed.Rows)
            {
                if (chbAll.Checked)
                    dgvr.Cells["������"].Value = true;
                else
                    dgvr.Cells["������"].Value = false;                   
            }               
        }          
    }  
}
