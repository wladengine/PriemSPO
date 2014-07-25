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

namespace Priem
{
    public partial class OlympAbitList : BookList
    {
        //�����������
        public OlympAbitList()
        {
            InitializeComponent(); 
            
            Dgv = dgvAbitList;
            _title = "������ ������������ � �����������";
           
            InitControls();
            InitHandlers(); 
        }

        protected override void ExtraInit()             
        {
            base.ExtraInit();

            btnAdd.Visible = btnRemove.Visible = false;

            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    ComboServ.FillCombo(cbFaculty, HelpClass.GetComboListByTable("ed.qFaculty", "ORDER BY Acronym"), false, false);
                    ComboServ.FillCombo(cbStudyForm, HelpClass.GetComboListByTable("ed.StudyForm", "ORDER BY Id"), false, false);

                    FillLicenseProgram();
                    FillObrazProgram();    

                    ComboServ.FillCombo(cbLanguage, HelpClass.GetComboListByTable("ed.Language"), false, true);
                    ComboServ.FillCombo(cbOlympType, HelpClass.GetComboListByTable("ed.OlympType", " ORDER BY Id"), false, true);
                    ComboServ.FillCombo(cbOlympName, HelpClass.GetComboListByTable("ed.OlympName"), false, true);
                    ComboServ.FillCombo(cbOlympSubject, HelpClass.GetComboListByTable("ed.OlympSubject"), false, true);
                    ComboServ.FillCombo(cbOlympValue, HelpClass.GetComboListByTable("ed.OlympValue"), false, true);
                    ComboServ.FillCombo(cbOlympLevel, HelpClass.GetComboListByTable("ed.OlympLevel"), false, true);

                    UpdateDataGrid();                    
                }
            }
            catch (Exception exc)
            {
                WinFormsServ.Error("������ ��� ������������� ����� " + exc.Message);
            }           
        }

        //������������� ������������ ����������
        public override void  InitHandlers()
        {
            cbFaculty.SelectedIndexChanged += new EventHandler(cbFaculty_SelectedIndexChanged);
            cbLicenseProgram.SelectedIndexChanged += new EventHandler(cbLicenseProgram_SelectedIndexChanged);
            cbObrazProgram.SelectedIndexChanged += new EventHandler(cbObrazProgram_SelectedIndexChanged);           
            cbStudyForm.SelectedIndexChanged += new EventHandler(cbStudyForm_SelectedIndexChanged);

            cbLanguage.SelectedIndexChanged += new EventHandler(cbLanguage_SelectedIndexChanged);
            cbOlympType.SelectedIndexChanged += new EventHandler(cbOlympType_SelectedIndexChanged);
            cbOlympName.SelectedIndexChanged += new EventHandler(cbOlympName_SelectedIndexChanged);
            cbOlympSubject.SelectedIndexChanged += new EventHandler(cbOlympSubject_SelectedIndexChanged);
            cbOlympValue.SelectedIndexChanged += new EventHandler(cbOlympValue_SelectedIndexChanged);
            cbOlympLevel.SelectedIndexChanged += new EventHandler(cbOlympLevel_SelectedIndexChanged);
        }

        void cbOlympLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbOlympValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbOlympSubject_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbOlympName_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbOlympType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbStudyForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        void cbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillLicenseProgram();
            UpdateDataGrid();
        }

        void cbLicenseProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillObrazProgram();
            UpdateDataGrid();
        }

        void cbObrazProgram_SelectedIndexChanged(object sender, EventArgs e)
        {           
            UpdateDataGrid();
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

        public int? ObrazProgramId
        {
            get { return ComboServ.GetComboIdInt(cbObrazProgram); }
            set { ComboServ.SetComboId(cbObrazProgram, value); }
        }

        public int? StudyFormId
        {
            get { return ComboServ.GetComboIdInt(cbStudyForm); }
            set { ComboServ.SetComboId(cbStudyForm, value); }
        }

        public int? OlympTypeId
        {
            get { return ComboServ.GetComboIdInt(cbOlympType); }
            set { ComboServ.SetComboId(cbOlympType, value); }
        }

        public int? OlympSubjectId
        {
            get { return ComboServ.GetComboIdInt(cbOlympSubject); }
            set { ComboServ.SetComboId(cbOlympSubject, value); }
        }

        public int? OlympLevelId
        {
            get { return ComboServ.GetComboIdInt(cbOlympLevel); }
            set { ComboServ.SetComboId(cbOlympLevel, value); }
        }

        public int? OlympValueId
        {
            get { return ComboServ.GetComboIdInt(cbOlympValue); }
            set { ComboServ.SetComboId(cbOlympValue, value); }
        }

        public int? OlympNameId
        {
            get { return ComboServ.GetComboIdInt(cbOlympName); }
            set { ComboServ.SetComboId(cbOlympName, value); }
        }

        public int? LanguageId
        {
            get { return ComboServ.GetComboIdInt(cbLanguage); }
            set { ComboServ.SetComboId(cbLanguage, value); }
        }

        private void FillLicenseProgram()
        {
            using (PriemEntities context = new PriemEntities())
            {
                List<KeyValuePair<string, string>> lst = ((from ent in MainClass.GetEntry(context)
                                                           where ent.FacultyId == FacultyId
                                                           select new
                                                           {
                                                               Id = ent.LicenseProgramId,
                                                               Name = ent.LicenseProgramName
                                                           }).Distinct()).ToList().Select(u => new KeyValuePair<string, string>(u.Id.ToString(), u.Name)).ToList();

                ComboServ.FillCombo(cbLicenseProgram, lst, false, false);
                cbLicenseProgram.SelectedIndex = 0;
            }
        }

        private void FillObrazProgram()
        {
            using (PriemEntities context = new PriemEntities())
            {
                List<KeyValuePair<string, string>> lst = ((from ent in MainClass.GetEntry(context)
                                                           where ent.FacultyId == FacultyId && ent.LicenseProgramId == LicenseProgramId
                                                           select new
                                                           {
                                                               Id = ent.ObrazProgramId,
                                                               Name = ent.ObrazProgramName,
                                                               Crypt = ent.ObrazProgramCrypt
                                                           }).Distinct()).ToList().Select(u => new KeyValuePair<string, string>(u.Id.ToString(), u.Name + ' ' + u.Crypt)).ToList();

                ComboServ.FillCombo(cbObrazProgram, lst, false, false);
            }
        }

        protected override void GetSource()
        {
            _sQuery = string.Format(@"SELECT ed.extAbit.Id as Id, RegNum as ���_�����, ed.Person.Surname + ' ' + ed.Person.Name + ' ' + ed.Person.SecondName as ���,
                     ed.Competition.Name as ���_��������, OlympTypeName as ���, OlympName AS ��������, OlympLevelName AS �������, OlympSubjectName as �������,
                     OlympValueName as �������, StudyFormName as �����_��������, ed.Person.BirthDate as ����_��������, PassportData as �������                      
                     FROM ed.extAbit 
                     INNER JOIN ed.Person ON ed.extAbit.PersonId = ed.Person.Id
                     INNER JOIN ed.extOlympiads ON ed.extOlympiads.AbiturientId = ed.extAbit.Id                     
                     LEFT JOIN ed.Competition ON ed.Competition.Id = ed.extAbit.CompetitionId ");

            string sFilters = GetFilterString();

            HelpClass.FillDataGrid(Dgv, _bdc, _sQuery, sFilters, " ORDER BY FIO");            
        }

        //���� �������� 
        private string GetFilterString()
        {
            string s = " WHERE 1=1 ";

            //���������� ����� ��������           
            if (StudyFormId != null)
                s += " AND ed.extAbit.StudyFormId = " + StudyFormId;

            //���������� ���������            
            if (FacultyId != null)
                s += " AND ed.extAbit.FacultyId = " + FacultyId;

            //���������� ���            
            if (OlympTypeId != null)
                s += " AND ed.extOlympiads.OlympTypeId = " + OlympTypeId;

            //���������� �������            
            if (OlympLevelId != null)
                s += " AND ed.extOlympiads.OlympLevelId = " + OlympLevelId;

            //���������� �������            
            if (OlympSubjectId != null)
                s += " AND ed.extOlympiads.OlympSubjectId = " + OlympSubjectId;

            //���������� ��������            
            if (OlympValueId != null)
                s += " AND ed.extOlympiads.OlympValueId  = " + OlympValueId;

            //���������� ��������            
            if (OlympNameId != null)
                s += " AND ed.extOlympiads.OlympNameId  = " + OlympNameId;

            //���������� �����������            
            if (LicenseProgramId != null)
                s += " AND ed.extAbit.LicenseProgramId = " + LicenseProgramId;

            //���������� ����� ���������             
            if (ObrazProgramId != null)
                s += " AND ed.extAbit.ObrazProgramId = " + ObrazProgramId;

            //���������� ����          
            if (LanguageId != null)
                s += " AND ed.extAbit.LanguageId = " + LanguageId;   
            
            return s;
        }

        //����� �� ������
        private void tbNumber_TextChanged(object sender, EventArgs e)
        {
            WinFormsServ.Search(this.dgvAbitList, "���_�����", tbNumber.Text);
        }

        //����� �� ���
        private void tbFIO_TextChanged(object sender, EventArgs e)
        {
            WinFormsServ.Search(this.dgvAbitList, "���", tbFIO.Text);
        }

        //������
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                int k = 7;
                WordDoc wd = new WordDoc(string.Format(@"{0}\ListAbit.dot", MainClass.dirTemplates));

                wd.AddNewTable(dgvAbitList.Rows.Count + 1, k+1);
                TableDoc td = wd.Tables[0];

                //����������
                string sFac = cbFaculty.Text.ToLower();
                if (sFac.CompareTo("���") == 0)
                    sFac = "���� ����������� ";
                else
                { 
                    if (FacultyId == 3)
                        sFac = "������ ����� ����������� ";
                    else
                        sFac = sFac.Replace("���", "���� ").Replace("��", "��� ") + " ���������� ";
                }

                string sForm = cbStudyForm.Text.ToLower();
                if (sForm.CompareTo("���") == 0)
                    sForm = " ���� ���� �������� ";
                else
                    sForm = sForm.Replace("��", "��").Replace("��", "��") + " ����� �������� ";
                wd.Fields["Faculty"].Text = sFac;
                wd.Fields["Section"].Text = sForm;

                int i = 0;

                td[0, 0] = "� �/�";
                for (int j = 0; j < k; j++)
                    td[j + 1, 0] = dgvAbitList.Columns[j+1].HeaderText;

                // ������ �� �����
                foreach (DataGridViewRow dgvr in dgvAbitList.Rows)
                {
                    td[0, i + 1] = (i + 1).ToString();
                    for (int j = 0; j < k; j++)
                        td[j + 1, i + 1] = dgvAbitList.Rows[i].Cells[j+1].Value.ToString();

                    i++;
                }
            }
            catch (Exception exc)
            {
                WinFormsServ.Error("������ ������ � Word: \n" + exc.Message);
            }
        }        

        protected override void OpenCard(string id, BaseFormEx formOwner, int? index)
        {
            MainClass.OpenCardAbit(id, this, dgvAbitList.CurrentRow.Index);
        }        
    }
}