using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using WordOut;
using EducServLib;
using BDClassLib;
using BaseFormsLib;
using PriemLib; 

//namespace Priem
//{
//    public partial class ExamResults : BookList
//    {       
//        private SortedList examsId;
//        private bool muchExams;

//        public ExamResults()
//        {
//            InitializeComponent();
//            examsId = new SortedList();
//            Dgv = dgvMarks;
//            _tableName = "ed.qMark";
//            _title = "���������� ���������";

//            InitControls();             
//        }

//        //�������������� ������������� ���������
//        protected override void ExtraInit()
//        {
//            base.ExtraInit();         
//            this.Width = 840;

//            muchExams = false;

//            btnRemove.Visible = btnAdd.Visible = false;

//            try
//            {
//                using (PriemEntities context = new PriemEntities())
//                {
//                    ComboServ.FillCombo(cbFaculty, HelpClass.GetComboListByTable("ed.qFaculty", "ORDER BY Acronym"), false, false);
//                    ComboServ.FillCombo(cbStudyBasis, HelpClass.GetComboListByTable("ed.StudyBasis", "ORDER BY Name"), false, true);

//                    cbStudyBasis.SelectedIndex = 0;
//                    FillStudyForm();
//                    FillLicenseProgram();
//                    FillObrazProgram();
//                    FillProfile();
                    
//                    ComboServ.FillCombo(cbCompetition, HelpClass.GetComboListByTable("ed.Competition", "ORDER BY Name"), false, true);
//                    FillExams();

//                    chbEGE.Checked = false;
//                    chbOlymps.Checked = false;

//                    if (MainClass.dbType == PriemType.PriemMag)
//                    {
//                        chbEGE.Visible = false;
//                        chbOlymps.Visible = false;
//                    }
//                }
//            }
//            catch (Exception exc)
//            {
//                WinFormsServ.Error("������ ��� ������������� ����� " + exc.Message);
//            }           
//        }

//        #region Handlers

//        public int? FacultyId
//        {
//            get { return ComboServ.GetComboIdInt(cbFaculty); }
//            set { ComboServ.SetComboId(cbFaculty, value); }
//        }
//        public int? LicenseProgramId
//        {
//            get { return ComboServ.GetComboIdInt(cbLicenseProgram); }
//            set { ComboServ.SetComboId(cbLicenseProgram, value); }
//        }
//        public int? ObrazProgramId
//        {
//            get { return ComboServ.GetComboIdInt(cbObrazProgram); }
//            set { ComboServ.SetComboId(cbObrazProgram, value); }
//        }
//        public Guid? ProfileId
//        {
//            get
//            {
//                string prId = ComboServ.GetComboId(cbProfile);
//                if (string.IsNullOrEmpty(prId))
//                    return null;
//                else
//                    return new Guid(prId);
//            }
//            set
//            {
//                if (value == null)
//                    ComboServ.SetComboId(cbProfile, (string)null);
//                else
//                    ComboServ.SetComboId(cbProfile, value.ToString());
//            }
//        }
//        public int? StudyBasisId
//        {
//            get { return ComboServ.GetComboIdInt(cbStudyBasis); }
//            set { ComboServ.SetComboId(cbStudyBasis, value); }
//        }
//        public int? StudyFormId
//        {
//            get { return ComboServ.GetComboIdInt(cbStudyForm); }
//            set { ComboServ.SetComboId(cbStudyForm, value); }
//        }
//        public int? CompetitionId
//        {
//            get { return ComboServ.GetComboIdInt(cbCompetition); }
//            set { ComboServ.SetComboId(cbCompetition, value); }
//        }
//        public int? ExamId
//        {
//            get { return ComboServ.GetComboIdInt(cbExam); }
//            set { ComboServ.SetComboId(cbExam, value); }
//        }

//        private void FillStudyForm()
//        {            
//            using (PriemEntities context = new PriemEntities())
//            {
//                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId);

//                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.StudyFormId.ToString(), u.StudyFormName)).Distinct().ToList();

//                ComboServ.FillCombo(cbStudyForm, lst, false, true);
//                cbStudyForm.SelectedIndex = 0;
//            }
//        }
//        private void FillLicenseProgram()
//        {            
//            using (PriemEntities context = new PriemEntities())
//            {
//                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId);

//                if(StudyFormId != null)
//                    ent = ent.Where(c => c.StudyFormId == StudyFormId);

//                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.LicenseProgramId.ToString(), u.LicenseProgramName)).Distinct().ToList();

//                ComboServ.FillCombo(cbLicenseProgram, lst, false, true);
//                cbLicenseProgram.SelectedIndex = 0;
//            }
//        }
//        private void FillObrazProgram()
//        {          
//            using (PriemEntities context = new PriemEntities())
//            {
//                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId);

//                if (StudyFormId != null)
//                    ent = ent.Where(c => c.StudyFormId == StudyFormId);
//                if (LicenseProgramId != null)
//                    ent = ent.Where(c => c.LicenseProgramId == LicenseProgramId);

//                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.ObrazProgramId.ToString(), u.ObrazProgramName + ' ' + u.ObrazProgramCrypt)).Distinct().ToList();

//                ComboServ.FillCombo(cbObrazProgram, lst, false, true);
//            }
//        }
//        private void FillProfile()
//        {           
//            using (PriemEntities context = new PriemEntities())
//            {
//                if (ObrazProgramId == null)
//                {
//                    ComboServ.FillCombo(cbProfile, new List<KeyValuePair<string, string>>(), false, false);
//                    cbProfile.Enabled = false;
//                    return;
//                }

//                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId).Where(c => c.ProfileId != null);

//                if (StudyFormId != null)
//                    ent = ent.Where(c => c.StudyFormId == StudyFormId);
//                if (LicenseProgramId != null)
//                    ent = ent.Where(c => c.LicenseProgramId == LicenseProgramId);
//                if (ObrazProgramId != null)
//                    ent = ent.Where(c => c.ObrazProgramId == ObrazProgramId);

//                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.ProfileId.ToString(), u.ProfileName)).Distinct().ToList();

//                if (lst.Count() > 0)
//                {
//                    ComboServ.FillCombo(cbProfile, lst, false, true);
//                    cbProfile.Enabled = true;
//                }
//                else
//                {
//                    ComboServ.FillCombo(cbProfile, new List<KeyValuePair<string, string>>(), false, false);
//                    cbProfile.Enabled = false;
//                }
//            }
//        }
//        private void FillExams()
//        {            
//            using (PriemEntities context = new PriemEntities())
//            {
//                var ent = Exams.GetExamsWithFilters(context, FacultyId, LicenseProgramId, ObrazProgramId, ProfileId, StudyFormId, StudyBasisId, null, null, null);
//                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.ExamId.ToString(), u.ExamName)).Distinct().ToList();
//                ComboServ.FillCombo(cbExam, lst, false, true);
//            }            
//        }

//        //������������� ������������ ����������
//        public override void InitHandlers()
//        {
//            cbFaculty.SelectedIndexChanged += new EventHandler(cbFaculty_SelectedIndexChanged);
//            cbLicenseProgram.SelectedIndexChanged += new EventHandler(cbLicenseProgram_SelectedIndexChanged);
//            cbObrazProgram.SelectedIndexChanged += new EventHandler(cbObrazProgram_SelectedIndexChanged);
//            cbProfile.SelectedIndexChanged += new EventHandler(cbProfile_SelectedIndexChanged);
//            cbStudyForm.SelectedIndexChanged += new EventHandler(cbStudyForm_SelectedIndexChanged);
//            cbStudyBasis.SelectedIndexChanged += new EventHandler(cbStudyBasis_SelectedIndexChanged);
//            cbCompetition.SelectedIndexChanged += new EventHandler(cbCompetition_SelectedIndexChanged);
//            cbExam.SelectedIndexChanged += new EventHandler(cbExam_SelectedIndexChanged);

//            chbEGE.CheckStateChanged += new System.EventHandler(this.chbEGE_CheckStateChanged);
//            chbOlymps.CheckStateChanged += new System.EventHandler(this.chbOlymps_CheckStateChanged);
//        }

//        void cbFaculty_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            //if (dgvMarks.Rows.Count > 0)
//            //{
//            //    cbStudyForm.SelectedIndexChanged -= new EventHandler(cbStudyForm_SelectedIndexChanged);
//            //    cbLicenseProgram.SelectedIndexChanged -= new EventHandler(cbLicenseProgram_SelectedIndexChanged);
//            //    cbObrazProgram.SelectedIndexChanged -= new EventHandler(cbObrazProgram_SelectedIndexChanged);
//            //    cbProfile.SelectedIndexChanged -= new EventHandler(cbProfile_SelectedIndexChanged);
//            //    cbStudyForm.SelectedIndexChanged -= new EventHandler(cbStudyForm_SelectedIndexChanged);
//            //    cbStudyBasis.SelectedIndexChanged -= new EventHandler(cbStudyBasis_SelectedIndexChanged);
//            //    cbCompetition.SelectedIndexChanged -= new EventHandler(cbCompetition_SelectedIndexChanged);
//            //}

//            FillStudyForm();
//            //FillLicenseProgram();
//            //FillObrazProgram();
//            //FillProfile();
//            //FillExams();

//            //cbStudyForm.SelectedIndexChanged += new EventHandler(cbStudyForm_SelectedIndexChanged);
//            //cbLicenseProgram.SelectedIndexChanged += new EventHandler(cbLicenseProgram_SelectedIndexChanged);
//            //cbObrazProgram.SelectedIndexChanged += new EventHandler(cbObrazProgram_SelectedIndexChanged);
//            //cbProfile.SelectedIndexChanged += new EventHandler(cbProfile_SelectedIndexChanged);
//            //cbStudyForm.SelectedIndexChanged += new EventHandler(cbStudyForm_SelectedIndexChanged);
//            //cbStudyBasis.SelectedIndexChanged += new EventHandler(cbStudyBasis_SelectedIndexChanged);
//            //cbCompetition.SelectedIndexChanged += new EventHandler(cbCompetition_SelectedIndexChanged);
//        }
//        void cbStudyForm_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            FillLicenseProgram();            
//        }
//        void cbLicenseProgram_SelectedIndexChanged(object sender, EventArgs e)
//        {        
//            FillObrazProgram();            
//        }
//        void cbObrazProgram_SelectedIndexChanged(object sender, EventArgs e)
//        {           
//            FillProfile();
//            //if (cbProfile.Items.Count < 2)
//            //    FillExams();
//        }
//        void cbProfile_SelectedIndexChanged(object sender, EventArgs e)
//        {           
//            FillExams();
//        }
//        void cbStudyBasis_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            FillExams();
//        }
//        void cbCompetition_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            UpdateDataGrid();
//        }
//        void cbExam_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            UpdateDataGrid();
//        }

//        #endregion

//        public override void UpdateDataGrid()
//        {
//            FillGridMarks(GetAbitFilterString(), GetExamFilterString(), chbEGE.Checked, chbOlymps.Checked);          

//            lblCount.Text = "�����: " + Dgv.Rows.Count.ToString();
//            btnCard.Enabled = !(Dgv.RowCount == 0);
//        }
        
//        private void FillGridMarks(string abitFilters, string examFilters, bool isEge, bool isOlymp)
//        {
//            DataTable examTable = new DataTable();
//            DataSet ds;
//            string sQueryAbit;  
//            List<ListItem> egeList = new List<ListItem>();
//            List<ListItem> olympList = new List<ListItem>();

//            DataColumn clm;
//            clm = new DataColumn();
//            clm.ColumnName = "��_�����";
//            examTable.Columns.Add(clm);

//            clm = new DataColumn();
//            clm.ColumnName = "���";
//            examTable.Columns.Add(clm);

//            clm = new DataColumn();
//            clm.ColumnName = "���_�����";
//            examTable.Columns.Add(clm); 
            
//            clm = new DataColumn();
//            clm.ColumnName = "�����������";
//            examTable.Columns.Add(clm);            
            
//            clm = new DataColumn();
//            clm.ColumnName = "������ ��������";
//            examTable.Columns.Add(clm);
            
//            clm = new DataColumn();
//            clm.ColumnName = "����� ��������";
//            examTable.Columns.Add(clm);            

//            clm = new DataColumn();
//            clm.ColumnName = "Id";
//            examTable.Columns.Add(clm);

//            clm = new DataColumn();
//            clm.ColumnName = "�������";
//            examTable.Columns.Add(clm);

//            clm = new DataColumn();
//            clm.ColumnName = "����� ������";
//            clm.DataType = typeof(int);
//            examTable.Columns.Add(clm);

//            IList lst = examsId.GetKeyList();

//            string examsFields = string.Empty;

//            if (examsId.Count > 50)
//            {
//                if(!muchExams) 
//                    WinFormsServ.Error("������� ����� ���������, �������������� ���������!");
                
//                muchExams = true;
//                dgvMarks.DataSource = null;
//                return;
//            }

//            muchExams = false;
//            foreach (DictionaryEntry de in examsId)
//            {
//                clm = new DataColumn();
//                clm.ColumnName = de.Value.ToString();                                              
//                examTable.Columns.Add(clm);

//                clm = new DataColumn();
//                clm.ColumnName = de.Value.ToString()+"IsEge";                
//                examTable.Columns.Add(clm);

//                clm = new DataColumn();
//                clm.ColumnName = de.Value.ToString() + "IsOlymp";
//                examTable.Columns.Add(clm);

////                examsFields += string.Format(@"
////                        , (SELECT SUM(Value) FROM ed.qMark INNER JOIN ed.extExamInEntry ON qMark.ExamInEntryId = ed.extExamInEntry.Id WHERE ed.extExamInEntry.ExamId={0} AND ed.qMark.AbiturientId=ed.extAbitSPO.Id AND ed.extExamInEntry.FacultyId = {1}) as '{2}'
////                        , (SELECT Top 1 IsFromEge FROM ed.qMark INNER JOIN ed.extExamInEntry ON qMark.ExamInEntryId = ed.extExamInEntry.Id WHERE ed.extExamInEntry.ExamId={0} AND ed.qMark.AbiturientId=ed.extAbitSPO.Id AND ed.extExamInEntry.FacultyId = {1}) as '{2}IsEge'
////                        , (SELECT Top 1 IsFromOlymp FROM ed.qMark INNER JOIN ed.extExamInEntry ON qMark.ExamInEntryId = ed.extExamInEntry.Id WHERE ed.extExamInEntry.ExamId={0} AND ed.qMark.AbiturientId=ed.extAbitSPO.Id AND ed.extExamInEntry.FacultyId = {1}) as '{2}IsOlymp'",
////                    de.Key, FacultyId, de.Key);
//            }

////            sQueryAbit = string.Format(@"SELECT DISTINCT ed.extAbitSPO.Id as Id, extPersonSPO.PersonNum as ��_�����, ed.extAbitSPO.RegNum as ���_�����, ed.extAbitMarksSum.TotalSum AS Sum, ed.extPersonSPO.FIO as ���, 
////                        ed.extAbitSPO.StudyFormName AS StudyForm, ed.extAbitSPO.StudyBasisName AS StudyBasis, ed.extAbitSPO.ObrazProgramCrypt + ' ' +(Case when NOT ed.extAbitSPO.ProfileId IS NULL then ed.extAbitSPO.ProfileName else ed.extAbitSPO.ObrazProgramName end) as Spec, 
////                        Competition.Name AS CompName {0} 
////                        FROM ed.extAbitSPO LEFT JOIN ed.extPersonSPO ON ed.extAbitSPO.PersonId = ed.extPersonSPO.Id 
////                        LEFT JOIN ed.Competition ON ed.extAbitSPO.CompetitionId = ed.Competition.Id LEFT JOIN ed.extAbitMarksSum ON ed.extAbitMarksSum.Id = ed.extAbitSPO.Id 
////                        {1} ORDER BY ��� ", examsFields, abitFilters);
            
//            NewWatch wc = new NewWatch();
//            wc.Show();
//            wc.SetText("��������� ������ �� ������������...");
//            sQueryAbit = string.Format(@"SELECT DISTINCT 
//ed.extAbitSPO.Id as Id, 
//extPersonSPO.PersonNum as ��_�����, 
//ed.extAbitSPO.RegNum as ���_�����, 
//ed.extAbitMarksSum.TotalSum AS Sum, 
//ed.extPersonSPO.FIO as ���, 
//ed.extAbitSPO.ObrazProgramCrypt + ' ' +(Case when NOT ed.extAbitSPO.ProfileId IS NULL then ed.extAbitSPO.ProfileName else ed.extAbitSPO.ObrazProgramName end) as Spec, 
//ed.extAbitSPO.StudyFormName AS StudyForm, 
//ed.extAbitSPO.StudyBasisName AS StudyBasis,
//Competition.Name AS CompName
//FROM ed.extAbitSPO 
//LEFT JOIN ed.extPersonSPO ON ed.extAbitSPO.PersonId = ed.extPersonSPO.Id 
//LEFT JOIN ed.Competition ON ed.extAbitSPO.CompetitionId = ed.Competition.Id 
//LEFT JOIN ed.extAbitMarksSum ON ed.extAbitMarksSum.Id = ed.extAbitSPO.Id 
//--LEFT JOIN ed.qMark ON qMark.AbiturientId = extAbitSPO.Id
//{0}
//ORDER BY ���", abitFilters);
                        
//            ds = _bdc.GetDataSet(sQueryAbit);

//            var abit_data = from DataRow rw in ds.Tables[0].Rows
//                            select new
//                            {
//                                Id = rw.Field<Guid>("Id"),
//                                PersonNum = rw["��_�����"].ToString(),
//                                RegNum = rw["���_�����"].ToString(),
//                                Sum = rw.Field<int?>("Sum"),
//                                FIO = rw.Field<string>("���"),
//                                StudyForm = rw.Field<string>("StudyForm"),
//                                StudyBasis = rw.Field<string>("StudyBasis"),
//                                Spec = rw.Field<string>("Spec"),
//                                Competition = rw.Field<string>("CompName"),
//                            };

//            List<Guid> ids = abit_data.Select(x => x.Id).Distinct().ToList();
//            //NewWatch wc = new NewWatch(ids.Count); 
//            wc.SetText("��������� ������ �� ����...");
            

//            string query = string.Format(@"SELECT AbiturientId, 
//                qMark.ExamId,
//                qMark.Value,
//                case when qMark.IsFromEge IS NULL OR qMark.IsFromEge = 'False' then 0 else 1 end AS IsFromEge,
//                case when qMark.IsFromOlymp IS NULL OR qMark.IsFromOlymp = 'False' then 0 else 1 end AS IsFromOlymp
//                FROM ed.qMark
//                INNER JOIN ed.extAbitSPO ON extAbitSPO.Id = qMark.AbiturientId
//                {0}", abitFilters);
//            ds = _bdc.GetDataSet(query);

//            var marks = from DataRow rw in ds.Tables[0].Rows
//                        select new
//                        {
//                            AbiturientId = rw.Field<Guid>("AbiturientId"),
//                            ExamId = rw.Field<int?>("ExamId"),
//                            Value = rw.Field<byte?>("Value"),
//                            IsFromOlymp = rw.Field<int>("IsFromOlymp") == 1 ? true : false,
//                            IsFromEge = rw.Field<int>("IsFromEge") == 1 ? true : false
//                        };
//            marks.GetEnumerator();
//            wc.SetText("���������� ������...");
            
//            List<string> lstIds = new List<string>();
//            int iCntAbits = ids.Count();
//            wc.SetMax(iCntAbits);
//            //foreach (DataRow dsRow in ds.Tables[0].Rows)
//            foreach (Guid abitId in ids)
//            {
//                DataRow newRow;
//                newRow = examTable.NewRow();
//                var abit = abit_data.Where(x => x.Id == abitId).First();
//                newRow["��_�����"] = abit.PersonNum.ToString(); // dsRow["��_�����"].ToString();
//                newRow["���"] = abit.FIO; //dsRow["���"].ToString();
//                newRow["���_�����"] = abit.RegNum.ToString(); //dsRow["���_�����"].ToString();
//                newRow["�����������"] = abit.Spec; //dsRow["Spec"].ToString();
//                newRow["������ ��������"] = abit.StudyBasis; //dsRow["StudyBasis"].ToString();
//                newRow["����� ��������"] = abit.StudyForm; //dsRow["StudyForm"].ToString();
//                newRow["Id"] = abit.Id.ToString(); //dsRow["Id"].ToString();
//                newRow["�������"] = abit.Competition; //dsRow["CompName"].ToString();
//                newRow["����� ������"] = abit.Sum.HasValue ? abit.Sum.Value : 0; //dsRow["CompName"].ToString();
//                foreach (DictionaryEntry de in examsId)
//                {
//                    int iExamId = 0;
//                    if (!int.TryParse(de.Key.ToString(), out iExamId))
//                        continue;
//                    var mark_data = marks.Where(x => x.AbiturientId == abitId && x.ExamId == iExamId);
//                    int markSum = mark_data.Select(x => x.Value).DefaultIfEmpty((byte?)0).Sum(x => x.Value);
//                    bool isFromEge = mark_data.Select(x => x.IsFromEge).DefaultIfEmpty(false).First();
//                    bool isFromOlymp = mark_data.Select(x => x.IsFromOlymp).DefaultIfEmpty(false).First();
                    
//                    newRow[de.Value.ToString()] = markSum == 0 ? "" : markSum.ToString(); //dsRow[de.Key.ToString()].ToString();
//                    newRow[de.Value.ToString() + "IsEge"] = isFromEge.ToString(); //dsRow[de.Key.ToString() + "IsEge"].ToString();
//                    newRow[de.Value.ToString() + "IsOlymp"] = isFromOlymp.ToString(); //dsRow[de.Key.ToString() + "IsOlymp"].ToString();
//                }

//                examTable.Rows.Add(newRow);
//                wc.PerformStep();
                
//                lstIds.Add(string.Format("'{0}'", abitId.ToString()));
//                //lstIds.Add(string.Format("'{0}'", dsRow["Id"].ToString()));
//            }

//            wc.Close();
//            /*
//            if (ds.Tables[0].Rows.Count > 0)
//            {
//                string colName;
//                sQuery = string.Format("SELECT qMark.Id, qMark.AbiturientId AS AbitId, qMark.Value AS Mark, extExamInProgram.ExamNameId AS ExamId, qMark.IsFromEge, qMark.IsFromOlymp FROM qMark LEFT JOIN extExamInProgram ON qMark.ExamInProgramId = extExamInProgram.Id WHERE qMark.AbiturientId IN ({0}) {1}", Util.BuildStringWithCollection(lstIds), examFilters);
//                ds = _bdc.GetDataSet(sQuery);

//                foreach (DataRow dsRow in ds.Tables[0].Rows)
//                {
//                    for (int i = 0; i < examTable.Rows.Count; i++)
//                    {
//                        if (examTable.Rows[i]["Id"].ToString() == dsRow["AbitId"].ToString())
//                        {
//                            colName = dsRow["ExamId"].ToString();
//                            if (dsRow["Mark"] != null && dsRow["Mark"].ToString() != "")
//                                examTable.Rows[i][colName] = dsRow["Mark"].ToString();
//                            if (bool.Parse(dsRow["IsFromEge"].ToString()))
//                                egeList.Add(new ListItem(i, colName));
//                            if (bool.Parse(dsRow["IsFromOlymp"].ToString()))
//                                olympList.Add(new ListItem(i, colName));
//                        }
//                    }
//                }
//            } */      

//            DataView dv = new DataView(examTable);
//            dv.AllowNew = false;

//            dgvMarks.DataSource = dv;
//            dgvMarks.ReadOnly = true;

//            dgvMarks.Columns["Id"].Visible = false;

//            foreach (DictionaryEntry de in examsId)
//            {                
//                dgvMarks.Columns[de.Value.ToString() + "IsEge"].Visible=false;
//                dgvMarks.Columns[de.Value.ToString() + "IsOlymp"].Visible=false;                
//            }

//            dgvMarks.Columns["��_�����"].Width = 65;
//            dgvMarks.Columns["���_�����"].Width = 66;
//            dgvMarks.Columns["���"].Width = 203;
//            dgvMarks.Columns["����� ��������"].Width = 56;
//            dgvMarks.Columns["������ ��������"].Width = 56;
//            dgvMarks.Columns["�����������"].Width = 100;
//            dgvMarks.Columns["�������"].Width = 54;
//            dgvMarks.Columns["����� ������"].Width = 49;
           
//            for (int i = 9; i < dgvMarks.Columns.Count; i = i + 1)
//                dgvMarks.Columns[i].Width = 43;

//            dgvMarks.Update(); 
//        }        

//        //������ ������ �������� ��� ������������
//        private string GetAbitFilterString()
//        {
//            string s = " WHERE 1=1 ";

//            s += " AND ed.extAbitSPO.StudyLevelGroupId = " + MainClass.studyLevelGroupId;

//            //���������� ����� ��������  
//            if (StudyFormId != null)
//                s += " AND ed.extAbitSPO.StudyFormId = " + StudyFormId;

//            //���������� ������ ��������  
//            if (StudyBasisId != null)
//                s += " AND ed.extAbitSPO.StudyBasisId = " + StudyBasisId;   

//            //���������� ���������
//            if (FacultyId != null)
//                s += " AND ed.extAbitSPO.FacultyId = " + FacultyId;

//            //���������� ��� ��������          
//            if (CompetitionId != null)
//                s += " AND ed.extAbitSPO.CompetitionId = " + CompetitionId;
            
//            //���������� �����������
//            if (LicenseProgramId != null)
//                s += " AND ed.extAbitSPO.LicenseProgramId = " + LicenseProgramId;

//            //���������� ����� ���������
//            if (ObrazProgramId != null)
//                s += " AND ed.extAbitSPO.ObrazProgramId = " + ObrazProgramId;

//            //���������� ������������� 
//            if (ProfileId != null)
//                s += string.Format(" AND ed.extAbitSPO.ProfileId = '{0}'", ProfileId);
          
//            return s;
//        }

//        // ������ ������ �������� ��� ���������
//        private string GetExamFilterString()
//        {
//            string s = "";
//            string defQuery = "SELECT DISTINCT ed.extExamInEntry.ExamId AS Id, ed.extExamInEntry.ExamName AS Name FROM ed.extExamInEntry WHERE 1=1";

//            s += " AND ed.extExamInEntry.StudyLevelGroupId = " + MainClass.studyLevelGroupId;
//            defQuery += " AND ed.extExamInEntry.StudyLevelGroupId = " + MainClass.studyLevelGroupId;

//            if (ExamId != null)
//            {
//                s += " AND ed.extExamInEntry.ExamId = " + ExamId;
//                defQuery += " AND ed.extExamInEntry.ExamId = " + ExamId;
//            }

//            //���������� ����� ��������  
//            if (StudyFormId != null)
//            {
//                s += " AND ed.extExamInEntry.StudyFormId = " + StudyFormId;
//                defQuery += " AND ed.extExamInEntry.StudyFormId = " + StudyFormId;
//            }
            
//            //���������� ������ ��������  
//            if (StudyBasisId != null)
//            {
//                s += " AND ed.extExamInEntry.StudyBasisId = " + StudyBasisId;
//                defQuery += " AND ed.extExamInEntry.StudyBasisId = " + StudyBasisId;
//            }

//            //���������� ���������
//            if (FacultyId != null)
//            {
//                s += " AND ed.extExamInEntry.FacultyId = " + FacultyId;
//                defQuery += " AND ed.extExamInEntry.FacultyId = " + FacultyId;
//            }

//            //���������� �����������
//            if (LicenseProgramId != null)
//            {
//                s += " AND ed.extExamInEntry.LicenseProgramId = " + LicenseProgramId;
//                defQuery += " AND ed.extExamInEntry.LicenseProgramId = " + LicenseProgramId;
//            }

//            //���������� ����� ���������             
//            if (ObrazProgramId != null)            
//            {
//                s += " AND ed.extExamInEntry.ObrazProgramId = " + ObrazProgramId;
//                defQuery += " AND ed.extExamInEntry.ObrazProgramId = " + ObrazProgramId;
//            }

//            //���������� �������������           
//            if (ProfileId != null)
//            {
//                s += string.Format(" AND ed.extExamInEntry.ProfileId = '{0}'", ProfileId);
//                defQuery += string.Format(" AND ed.extExamInEntry.ProfileId = '{0}'", ProfileId);              
//            }
          
//            //���������� ����
//            if (dtDateExam.Checked)            
//                s += " AND qMark.PassDate = " + _bdc.BuildData(dtDateExam.Value);           
            
//            DataSet ds = _bdc.GetDataSet(defQuery);
//            examsId.Clear();
//            foreach (DataRow dsRow in ds.Tables[0].Rows)
//            {
//                string sName =dsRow["Name"].ToString();
//                examsId.Add(dsRow["Id"].ToString(), sName);
//            }

//            return s;
//        }
        
//        protected override void OpenCard(string itemId, BaseFormEx formOwner, int? index)
//        {
//            MainClass.OpenCardAbit(itemId, this, dgvMarks.CurrentRow.Index);
//        }       

//        //����� �� ������
//        private void tbNumber_TextChanged(object sender, EventArgs e)
//        {
//            WinFormsServ.Search(this.dgvMarks, "���_�����", tbNumber.Text);
//        }
//        //����� �� ���
//        private void tbFIO_TextChanged(object sender, EventArgs e)
//        {
//            WinFormsServ.Search(this.dgvMarks, "���", tbFIO.Text);
//        }
        
//        private void chbEGE_CheckStateChanged(object sender, EventArgs e)
//        {            
//            dgvMarks.Refresh();
//        }
//        private void chbOlymps_CheckStateChanged(object sender, EventArgs e)
//        {            
//            dgvMarks.Refresh();
//        }       
//        private void btnPrint_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                WordDoc wd = new WordDoc(string.Format(@"{0}\Marks.dot", MainClass.dirTemplates));

//                //����������
//                string sFac = cbFaculty.Text.ToLower();
//                if (sFac.CompareTo("���") == 0)
//                    sFac = "��� ���������� ";
//                else
//                {                   
//                    if (FacultyId == 3)
//                        sFac = "������ ����� ����������� ";
//                    else
//                        sFac = sFac + " ��������� ";
//                }

//                string sForm = cbStudyForm.Text.ToLower();
//                if (sForm.CompareTo("���") == 0)
//                    sForm = " ��� ����� �������� ";
//                else
//                    sForm = sForm + " ����� �������� ";

//                string sCom = cbCompetition.Text.ToLower();
//                if (sCom.CompareTo("���") == 0)
//                    sCom = "��� ���� �������� ";
//                else
//                    sCom = "��� ��������: " + sCom;

//                string sSpec = cbLicenseProgram.Text.ToLower();
//                if (sSpec.CompareTo("���") == 0)
//                    sSpec = " ��� �����������";
//                else
//                    sSpec = " �����������: " + sSpec;

//                wd.Fields["Faculty"].Text = sFac;
//                wd.Fields["Section"].Text = sForm;
//                wd.Fields["Competition"].Text = sCom;
//                wd.Fields["Profession"].Text = sSpec;

//                int colCount = 0;
//                foreach (DataGridViewColumn clm in dgvMarks.Columns)
//                {
//                    if (clm.Visible)
//                        colCount++;
//                }

//                wd.AddNewTable(2, colCount);
//                TableDoc td = wd.Tables[0];

//                int i = 0;
//                foreach (DataGridViewColumn clm in dgvMarks.Columns)
//                {
//                    if (clm.Visible)
//                    {
//                        td[i, 0] = clm.HeaderText;
//                        i++;
//                    }
//                }

//                i = 1;
//                int j;
//                foreach (DataGridViewRow dgvr in dgvMarks.Rows)
//                {
//                    j = 0;
//                    foreach (DataGridViewColumn clm in dgvMarks.Columns)
//                    {
//                        if (clm.Visible)
//                        {
//                            td[j, i] = dgvr.Cells[clm.Index].Value.ToString();
//                            j++;
//                        }
//                    }
//                    i++;
//                    td.AddRow(1);
//                }
//            }
//            catch (WordException we)
//            {
//                WinFormsServ.Error(we.Message);
//            }
//            catch (Exception exc)
//            {
//                WinFormsServ.Error(exc.Message);
//            }              
//        }
//        private void btnUpdate_Click(object sender, EventArgs e)
//        {            
//            MainClass.DataRefresh();
//        }
//        private void dtDateExam_ValueChanged(object sender, EventArgs e)
//        {
//            UpdateDataGrid();
//        }
//        private void dgvMarks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
//        {
//            try
//            {
//                foreach (DictionaryEntry de in examsId)
//                {
//                    if (e.ColumnIndex == dgvMarks.Columns[de.Value.ToString()].Index)
//                    {
//                        if (chbEGE.Checked)
//                        {
//                            if (dgvMarks[de.Value.ToString() + "IsEge", e.RowIndex].Value.ToString() == "True")
//                                dgvMarks[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.LightGreen;
//                            else
//                                dgvMarks[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.White;
//                        }
//                        else if (chbOlymps.Checked)
//                        {
//                            if (dgvMarks[de.Value.ToString() + "IsOlymp", e.RowIndex].Value.ToString() == "True")
//                                dgvMarks[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.LightBlue;
//                            else
//                                dgvMarks[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.White;
//                        }
//                        else
//                            dgvMarks[e.ColumnIndex, e.RowIndex].Style.BackColor = Color.White;
//                        break;
//                    }

//                }
//            }
//            catch
//            {
//            }
//        }
//        protected override void Dgv_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
//        {
//            if (e.ColumnIndex <= 0)
//                _orderBy = null;
//            else
//                _orderBy = "it." + Dgv.Columns[e.ColumnIndex].Name;
//        }
//    }
//}