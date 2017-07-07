﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Transactions;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Entity.Core.Objects;

using EducServLib;
using BDClassLib;
using WordOut;
using PriemLib;
using RtfWriter;

namespace Priem
{
    public partial class RatingList : BookList
    {
        string _queryFrom;
        string _queryBody;
        string _queryOrange;

        //constructor
        public RatingList(bool fromFixieren)
        {
            InitializeComponent();

            _queryBody = @"SELECT DISTINCT Abiturient.Id as Id, Abiturient.RegNum as Рег_Номер, 
                    extPersonAll.PersonNum as 'Ид. номер', extPersonAll.FIO as ФИО, 
                    case when Abiturient.HasOriginals > 0 then 'Да' else 'Нет' end as 'Подлинники документов', 
Abiturient.Coefficient as 'Рейтинговый коэффициент', 
qAbiturientFizkultMark.Value AS 'Оценка Физ.Культ', 
qPersonAttMarkBiology.Value AS 'Атт. биология', 
qPersonAttMarkFizkult.Value AS 'Атт. Физ.Культ', 
qPersonAttMarkChem.Value AS 'Атт. хим', 
qPersonAttMarkRussian.Value AS 'Атт. Русский Язык', 
                    Competition.Name as Конкурс, 
                    CASE WHEN EXISTS(SELECT Id FROM ed.Olympiads WHERE OlympTypeId = 3 AND OlympValueId = 6 AND AbiturientId = Abiturient.Id) then 1 else CASE WHEN EXISTS(SELECT Id FROM ed.Olympiads WHERE OlympTypeId = 3 AND OlympValueId = 5 AND AbiturientId = Abiturient.Id) then 2 else CASE WHEN EXISTS(SELECT Id FROM ed.Olympiads WHERE OlympTypeId = 3 AND OlympValueId = 7 AND AbiturientId = Abiturient.Id) then 3 else 4 end end end as olymp,
                    CASE WHEN extPersonAll.AttestatSeries IN ('ЗА','ЗБ','ЗВ') then 1 else CASE WHEN extPersonAll.AttestatSeries IN ('СА','СБ','СВ') then 2 else 3 end end as attestat,
                    ISNULL(MRKAVG.Value, extPersonAll.SchoolAVG) as attAvg, 
                    CASE WHEN (CompetitionId=1  OR CompetitionId=8) then 1 else case when (CompetitionId=2 OR CompetitionId=7) AND extPersonAll.Privileges>0 then 2 else 3 end end as comp, 
                    CASE WHEN (CompetitionId=1 OR CompetitionId=8) then Abiturient.Coefficient else 10000 end as noexamssort, 
                    CASE WHEN (CompetitionId=5 OR CompetitionId=9) then 1 else 0 end as preimsort,
                    case when extPersonAll.IsExcellent>0 then 'Да' else 'Нет' end as 'Медалист', 
                    extPersonAll.AttestatSeries as 'Серия аттестата', 
                    extPersonAll.DiplomSeries as 'Серия диплома', 
                    ISNULL(MRKAVG.Value, extPersonAll.SchoolAVG) as 'Средний балл', 
                    extPersonAll.Email + ', '+ extPersonAll.Phone + ', ' + extPersonAll.Mobiles AS 'Контакты'";

            _queryFrom = @" FROM ed.Abiturient
INNER JOIN ed.extEntry ON extEntry.Id = Abiturient.EntryId
INNER JOIN ed.extPersonAll ON extPersonAll.Id = Abiturient.PersonId
INNER JOIN ed.Competition ON Competition.Id = Abiturient.CompetitionId
INNER JOIN ed.extEnableProtocol ON extEnableProtocol.AbiturientId = Abiturient.Id
LEFT JOIN ed.qAbiturientMarkAsSchoolAVG AS MRKAVG ON MRKAVG.AbiturientId = Abiturient.Id
LEFT JOIN ed.qAbiturientFizkultMark ON qAbiturientFizkultMark.Id = Abiturient.Id
LEFT JOIN ed.qPersonAttMarkBiology ON qPersonAttMarkBiology.PersonId = Abiturient.PersonId
LEFT JOIN ed.qPersonAttMarkFizkult ON qPersonAttMarkFizkult.PersonId = Abiturient.PersonId
LEFT JOIN ed.qPersonAttMarkChem ON qPersonAttMarkChem.PersonId = Abiturient.PersonId
LEFT JOIN ed.qPersonAttMarkRussian ON qPersonAttMarkRussian.PersonId = Abiturient.PersonId
LEFT JOIN ed.hlpEntryWithAddExams ON hlpEntryWithAddExams.EntryId = Abiturient.EntryId
LEFT JOIN ed.extAbitMarksSum ON Abiturient.Id = extAbitMarksSum.Id
LEFT JOIN ed.hlpMinMarkAbiturient ON hlpMinMarkAbiturient.Id = Abiturient.Id";

            Dgv = dgvAbits;
            _title = "Рейтинговый список";

            chbFix.Checked = fromFixieren;

            InitControls();            

            btnAdd.Visible = btnCard.Visible = btnRemove.Visible = false;
        }

        #region Init
        
        protected override void ExtraInit()
        {
            base.ExtraInit();

            btnFixieren.Visible = btnFixieren.Enabled = false;
            gbPasha.Visible = gbPasha.Enabled = false;
            chbFix.Visible = false;  

            if (MainClass.RightsFacMain() || MainClass.IsPasha())
                btnFixieren.Visible = btnFixieren.Enabled = true;

            if (MainClass.IsPasha())
            {
                gbPasha.Visible = gbPasha.Enabled = true;
                chbFix.Visible = true;  
            }

            if (!chbFix.Checked)
                gbPasha.Visible = gbPasha.Enabled = false;

            ComboServ.FillCombo(cbFaculty, HelpClass.GetComboListByTable("ed.qFaculty_SPO", "ORDER BY Acronym"), false, false);
            ComboServ.FillCombo(cbStudyBasis, HelpClass.GetComboListByTable("ed.StudyBasis", "ORDER BY Name"), false, false);

            cbStudyBasis.SelectedIndex = 0;
            FillStudyLevelGroup();
            FillStudyForm();
            FillLicenseProgram();
            FillObrazProgram();
            FillProfile();

            chbCel.Visible = false;

            if (MainClass.dbType == PriemType.PriemMag)
                chbWithOlymps.Visible = false;
        }

        public int? StudyLevelGroupId
        {
            get { return ComboServ.GetComboIdInt(cbStudyLevelGroup); }
            set { ComboServ.SetComboId(cbStudyLevelGroup, value); }
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
        public int? ProfileId
        {
            get
            {
                return ComboServ.GetComboIdInt(cbProfile);
            }
            set
            {
                if (value == null)
                    ComboServ.SetComboId(cbProfile, (string)null);
                else
                    ComboServ.SetComboId(cbProfile, value.ToString());
            }
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
        public bool IsSecond
        {
            get { return chbIsSecond.Checked; }
            set { chbIsSecond.Checked = value; }
        }
        public bool IsReduced
        {
            get { return chbIsReduced.Checked; }
            set { chbIsReduced.Checked = value; }
        }
        public bool IsParallel
        {
            get { return chbIsParallel.Checked; }
            set { chbIsParallel.Checked = value; }
        }
        public bool IsQuota
        {
            get { return chbIsQuota.Checked; }
            set { chbIsQuota.Checked = value; }
        }
        public bool IsCel
        {
            get { return chbCel.Checked; }
            set { chbCel.Checked = value; }
        }
        public Guid? EntryId
        {
            get
            {
                try
                {
                    using (PriemEntities context = new PriemEntities())
                    {
                        Guid? entId = (from ent in MainClass.GetEntry(context)
                                       where ent.IsSecond == IsSecond && ent.IsParallel == IsParallel && ent.IsReduced == IsReduced
                                       && ent.LicenseProgramId == LicenseProgramId
                                       && ent.ObrazProgramId == ObrazProgramId
                                       && (ProfileId == null ? ent.ProfileId == null : ent.ProfileId == ProfileId)
                                       && ent.StudyFormId == StudyFormId
                                       && ent.StudyBasisId == StudyBasisId
                                       select ent.Id).FirstOrDefault();
                        return entId;
                    }
                }
                catch
                {
                    return null;
                }
            }            
        }

        private void FillStudyLevelGroup()
        {
            using (PriemEntities context = new PriemEntities())
            {
                var ent = MainClass.GetEntry(context).Select(x => new { x.StudyLevelGroupId, x.StudyLevelGroupName });

                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.StudyLevelGroupId.ToString(), u.StudyLevelGroupName)).Distinct().ToList();

                ComboServ.FillCombo(cbStudyLevelGroup, lst, false, false);
            }
        }
        private void FillStudyForm()
        {
            using (PriemEntities context = new PriemEntities())
            {
                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId).Where(c=>c.StudyBasisId == StudyBasisId);
                
                ent = ent.Where(c => c.IsSecond == IsSecond && c.IsReduced == IsReduced && c.IsParallel == IsParallel);

                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.StudyFormId.ToString(), u.StudyFormName)).Distinct().ToList();

                ComboServ.FillCombo(cbStudyForm, lst, false, false);                
            }
        }
        private void FillLicenseProgram()
        {
            using (PriemEntities context = new PriemEntities())
            {
                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId);

                ent = ent.Where(c => c.IsSecond == IsSecond && c.IsReduced == IsReduced && c.IsParallel == IsParallel);

                if (StudyBasisId != null)
                    ent = ent.Where(c => c.StudyBasisId == StudyBasisId);
                if (StudyFormId != null)
                    ent = ent.Where(c => c.StudyFormId == StudyFormId);

                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.LicenseProgramId.ToString(), u.LicenseProgramName)).Distinct().ToList();

                ComboServ.FillCombo(cbLicenseProgram, lst, false, false);                
            }
        }
        private void FillObrazProgram()
        {
            using (PriemEntities context = new PriemEntities())
            {
                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId);

                ent = ent.Where(c => c.IsSecond == IsSecond && c.IsReduced == IsReduced && c.IsParallel == IsParallel);

                if (StudyBasisId != null)
                    ent = ent.Where(c => c.StudyBasisId == StudyBasisId);
                if (StudyFormId != null)
                    ent = ent.Where(c => c.StudyFormId == StudyFormId);
                if (LicenseProgramId != null)
                    ent = ent.Where(c => c.LicenseProgramId == LicenseProgramId);

                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.ObrazProgramId.ToString(), u.ObrazProgramName + ' ' + u.ObrazProgramCrypt)).Distinct().ToList();

                ComboServ.FillCombo(cbObrazProgram, lst, false, false);
            }
        }
        private void FillProfile()
        {
            using (PriemEntities context = new PriemEntities())
            {
                if (ObrazProgramId == null)
                {
                    ComboServ.FillCombo(cbProfile, new List<KeyValuePair<string, string>>(), false, false);
                    cbProfile.Enabled = false;
                    return;
                }

                var ent = MainClass.GetEntry(context).Where(c => c.FacultyId == FacultyId).Where(c => c.ProfileId != null);

                ent = ent.Where(c => c.IsSecond == IsSecond && c.IsReduced == IsReduced && c.IsParallel == IsParallel);

                if (StudyBasisId != null)
                    ent = ent.Where(c => c.StudyBasisId == StudyBasisId);
                if (StudyFormId != null)
                    ent = ent.Where(c => c.StudyFormId == StudyFormId);
                if (LicenseProgramId != null)
                    ent = ent.Where(c => c.LicenseProgramId == LicenseProgramId);
                if (ObrazProgramId != null)
                    ent = ent.Where(c => c.ObrazProgramId == ObrazProgramId);

                List<KeyValuePair<string, string>> lst = ent.ToList().Select(u => new KeyValuePair<string, string>(u.ProfileId.ToString(), u.ProfileName)).Distinct().ToList();

                if (lst.Count() > 0)
                {
                    ComboServ.FillCombo(cbProfile, lst, false, false);
                    cbProfile.Enabled = true;
                }
                else
                {
                    ComboServ.FillCombo(cbProfile, new List<KeyValuePair<string, string>>(), false, false);
                    cbProfile.Enabled = false;
                }              
            }
        }
        
        //инициализация обработчиков мегакомбов
        public override void InitHandlers()
        {
            cbFaculty.SelectedIndexChanged += new EventHandler(cbFaculty_SelectedIndexChanged);
            cbStudyForm.SelectedIndexChanged += new EventHandler(cbStudyForm_SelectedIndexChanged);
            cbStudyBasis.SelectedIndexChanged += new EventHandler(cbStudyBasis_SelectedIndexChanged);
            cbLicenseProgram.SelectedIndexChanged += new EventHandler(cbLicenseProgram_SelectedIndexChanged);
            cbObrazProgram.SelectedIndexChanged += new EventHandler(cbObrazProgram_SelectedIndexChanged);
            cbStudyLevelGroup.SelectedIndexChanged += cbFaculty_SelectedIndexChanged;

            chbFix.CheckedChanged += new EventHandler(chbFix_CheckedChanged);
        }

        void cbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {            
            FillStudyForm();
            NullDataGrid();
        }
        void cbStudyBasis_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillStudyForm();
            NullDataGrid();
        }
        void cbStudyForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillLicenseProgram();
            NullDataGrid();
        }        
        void cbLicenseProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillObrazProgram();
            NullDataGrid();
        }
        void cbObrazProgram_SelectedIndexChanged(object sender, EventArgs e)
        {           
            FillProfile();
            NullDataGrid();
        }       
        
        private void chbFix_CheckedChanged(object sender, EventArgs e)
        {
            if (chbFix.Checked)
                gbPasha.Visible = gbPasha.Enabled = true;
            else
                gbPasha.Visible = gbPasha.Enabled = false;

            UpdateDataGrid();
        }
 
        #endregion

        protected override void OpenCard(string id, BaseFormsLib.BaseFormEx formOwner, int? index)
        {
            MainClass.OpenCardAbit(id, this, dgvAbits.CurrentRow.Index);
        }

        int GetPlanValueAndCheckLock()
        {
            using (PriemEntities context = new PriemEntities())
            {
                int plan = 0, planCel = 0, planCrimea = 0, planQuota = 0, entered = 0, enteredCel = 0, enteredCrimea = 0, enteredQuota = 0;        

                qEntry entry = (from ent in MainClass.GetEntry(context)
                       where ent.IsReduced == IsReduced && ent.IsParallel == IsParallel && ent.IsSecond == IsSecond 
                       && ent.FacultyId == FacultyId && ent.LicenseProgramId == LicenseProgramId
                       && ent.ObrazProgramId == ObrazProgramId
                       && (ProfileId == null ? ent.ProfileId == null : ent.ProfileId == ProfileId)
                       && ent.StudyFormId == StudyFormId
                       && ent.StudyBasisId == StudyBasisId
                       select ent).FirstOrDefault();

                if (entry == null)
                    return 0;

                plan = entry.KCP ?? 0;
                planCel = entry.KCPCel ?? 0;

                Guid? entryId = entry.Id;

                entered = (from ab in context.qAbitAll
                           join ev in context.extEntryView
                           on ab.Id equals ev.AbiturientId
                           where ab.CompetitionId != 6 && ab.EntryId == entryId
                           select ab).Count();

                enteredCrimea = (from ab in context.qAbitAll
                                 join ev in context.extEntryView
                                 on ab.Id equals ev.AbiturientId
                                 where (ab.CompetitionId == 11 || ab.CompetitionId == 12) && ab.EntryId == entryId
                                 select ab).Count();

                enteredQuota = (from ab in context.qAbitAll
                                join ev in context.extEntryView
                                on ab.Id equals ev.AbiturientId
                                where (ab.CompetitionId == 2 || ab.CompetitionId == 7) && ab.EntryId == entryId
                                select ab).Count();

                enteredCel = (from ab in context.qAbitAll
                              join ev in context.extEntryView
                              on ab.Id equals ev.AbiturientId
                              where ab.CompetitionId == 6 && ab.EntryId == entryId
                              select ab).Count();
               
                CheckLockAndPasha(context);

                if (IsCel)
                    return planCel - enteredCel;
                else if (IsQuota)
                    return planQuota - enteredQuota;
                else
                    return plan - planCel - entered - enteredQuota;
            }
        }

        private void CheckLockAndPasha(PriemEntities context)
        {
            //лочит кнопку 
            FixierenView fixView = 
                (from fv in context.FixierenView
                 where fv.StudyLevelGroupId == StudyLevelGroupId
                 && fv.IsReduced == IsReduced && fv.IsParallel == IsParallel && fv.IsSecond == IsSecond
                 && fv.FacultyId == FacultyId && fv.LicenseProgramId == LicenseProgramId
                 && fv.ObrazProgramId == ObrazProgramId
                 && (ProfileId == null ? fv.ProfileId == 0 : fv.ProfileId == ProfileId)
                 && fv.StudyFormId == StudyFormId
                 && fv.StudyBasisId == StudyBasisId
                 && fv.IsCel == IsCel
                 select fv).FirstOrDefault();
            
            string DocNum = string.Empty;
            bool? locked = false;

            if (fixView != null)
            {
                DocNum = fixView.DocNum.ToString(); ;
                locked = fixView.Locked;
            }

            lblNumber.Text = DocNum.Length == 0 ? " -----" : DocNum;
            lblLocked.Text = locked.GetValueOrDefault(false) ? "ЗАЛОЧЕНА" : "НЕ залочена";

            return;
        }

        public void NullDataGrid()
        {
            if (dgvAbits.DataSource != null)
            {
                dgvAbits.DataSource = null;
                lblCount.Text = string.Empty;
            }
        }
                
        //обновление грида
        int plan = 0;
        public override void UpdateDataGrid()
        {
            try
            {
                string sOrderBy = string.Empty;
                if (LicenseProgramId == 557 || LicenseProgramId == 521)//Физическая культура
                    //1. лица, имеющие более высокий средний балл документа об образовании; 
                    //2. лица, имеющие более высокий балл по общеобразовательному предмету "Физическая культура" в документе об образовании; 
                    //3. лица, имеющие более высокий балл по общеобразовательному предмету "Биология" в документе об образовании; 
                    //4. лица, имеющие более высокий балл по общеобразовательному предмету "Русский язык" в документе об образовании; 
                    //5. лица, имеющие более высокую сумму баллов в документе об образовании".
                    sOrderBy = " ORDER BY comp, noexamssort desc, attAvg DESC, 'Атт. Физ.Культ' desc, 'Атт. биология' desc, 'Атт. Русский Язык' desc, 'Рейтинговый коэффициент' DESC, 'Оценка Физ.Культ' DESC, ФИО";
                    //sOrderBy = " ORDER BY comp, noexamssort desc, 'Оценка Физ.Культ' DESC, attAvg DESC, 'Атт. Физ.Культ' desc, 'Атт. биология' desc, 'Атт. Русский Язык' desc, ФИО";
                else //остальные
                    sOrderBy = " ORDER BY comp, noexamssort desc, attAvg DESC, 'Атт. хим' desc, 'Атт. биология' desc, 'Атт. Русский Язык' desc, 'Рейтинговый коэффициент' DESC, ФИО";
                string totalQuery = null;
                
                plan = GetPlanValueAndCheckLock();

                if (chbFix.Checked)
                {
                    if (MainClass.dbType == PriemType.PriemMag)
                        _queryOrange = @", CASE WHEN EXISTS(SELECT PersonId FROM ed.hlpPersonsWithOriginals WHERE PersonId = Abiturient.PersonId AND EntryId <> Abiturient.EntryId) then 1 else 0 end as orange ";
                    else
                        _queryOrange = @", CASE WHEN EXISTS(SELECT extEntryView.Id FROM ed.extEntryView INNER JOIN ed.Abiturient a ON ed.extEntryView.AbiturientId = a.Id WHERE a.PersonId = Abiturient.PersonId) then 1 else 0 end as orange ";

                    string queryFix = _queryBody + _queryOrange +
                    @" FROM ed.Abiturient
INNER JOIN ed.extEntry ON extEntry.Id = Abiturient.EntryId
INNER JOIN ed.extPersonAll ON extPersonAll.Id = Abiturient.PersonId                    
INNER JOIN ed.Competition ON Competition.Id = Abiturient.CompetitionId 
INNER JOIN ed.Fixieren ON Fixieren.AbiturientId = Abiturient.Id 
LEFT JOIN ed.qAbiturientMarkAsSchoolAVG AS MRKAVG ON MRKAVG.AbiturientId = Abiturient.Id
LEFT JOIN ed.qAbiturientFizkultMark ON qAbiturientFizkultMark.Id = Abiturient.Id
LEFT JOIN ed.qPersonAttMarkBiology ON qPersonAttMarkBiology.PersonId = Abiturient.PersonId
LEFT JOIN ed.qPersonAttMarkFizkult ON qPersonAttMarkFizkult.PersonId = Abiturient.PersonId
LEFT JOIN ed.qPersonAttMarkChem ON qPersonAttMarkChem.PersonId = Abiturient.PersonId
LEFT JOIN ed.qPersonAttMarkRussian ON qPersonAttMarkRussian.PersonId = Abiturient.PersonId
LEFT JOIN ed.hlpEntryWithAddExams ON hlpEntryWithAddExams.EntryId = Abiturient.EntryId
LEFT JOIN ed.FixierenView ON Fixieren.FixierenViewId= FixierenView.Id 
LEFT JOIN ed.hlpAbiturientProfAdd ON hlpAbiturientProfAdd.Id = Abiturient.Id 
LEFT JOIN ed.hlpAbiturientProf ON hlpAbiturientProf.Id = Abiturient.Id 
LEFT JOIN ed.extAbitMarksSum ON Abiturient.Id = extAbitMarksSum.Id";

                    /*string whereFix = string.Format(@" WHERE ed.FixierenView.StudyLevelGroupId = {10} AND ed.FixierenView.StudyFormId={0} AND ed.FixierenView.StudyBasisId={1} AND ed.FixierenView.FacultyId={2} 
                                                    AND ed.FixierenView.LicenseProgramId={3} AND ed.FixierenView.ObrazProgramId={4} {5} AND ed.FixierenView.IsCel = {6}
                                                    AND ed.FixierenView.IsSecond = {7} AND ed.FixierenView.IsReduced = {8} AND ed.FixierenView.IsParallel = {9} ",
                        StudyFormId, StudyBasisId, FacultyId, LicenseProgramId, ObrazProgramId, ProfileId == null ? " AND ed.FixierenView.ProfileId IS NULL" : "AND ed.FixierenView.ProfileId='" + ProfileId + "'", 
                        QueryServ.StringParseFromBool(IsCel), QueryServ.StringParseFromBool(IsSecond), QueryServ.StringParseFromBool(IsReduced), QueryServ.StringParseFromBool(IsParallel), MainClass.studyLevelGroupId);
                    */
                    string whereFix = string.Format(
@" WHERE FixierenView.StudyLevelGroupId = {10} AND FixierenView.StudyFormId={0} AND FixierenView.StudyBasisId={1} AND FixierenView.FacultyId={2} 
AND FixierenView.LicenseProgramId={3} AND FixierenView.ObrazProgramId={4} {5} AND FixierenView.IsCel = {6}
AND FixierenView.IsSecond = {7} AND FixierenView.IsReduced = {8} AND FixierenView.IsParallel = {9} AND FixierenView.IsQuota = {11}",
                        StudyFormId, StudyBasisId, FacultyId, LicenseProgramId, ObrazProgramId, ProfileId == null ? " AND FixierenView.ProfileId IS NULL" : "AND FixierenView.ProfileId='" + ProfileId + "'",
                        QueryServ.StringParseFromBool(IsCel), QueryServ.StringParseFromBool(IsSecond), QueryServ.StringParseFromBool(IsReduced), QueryServ.StringParseFromBool(IsParallel),
                        StudyLevelGroupId, QueryServ.StringParseFromBool(IsQuota));
                    //sOrderBy = " ORDER BY Fixieren.Number ";

                    totalQuery = queryFix + whereFix + sOrderBy;
                }
                else
                {
                    string sFilters = GetFilterString();
                    
                    //целевики?
                    //if (chbCel.Checked)
                    //    sFilters += " AND Abiturient.CompetitionId IN (6) ";
                    // в общем списке выводить всех 
                    //else
                    //    sFilters += " AND Abiturient.CompetitionId NOT IN (6) ";

                    //квотники?
                    if (IsQuota)
                        sFilters += " AND Abiturient.CompetitionId IN (2, 7) ";
                    //else
                    //    sFilters += " AND Abiturient.CompetitionId NOT IN (2, 7) ";

                    //не забрали доки
                    sFilters += " AND (Abiturient.BackDoc=0) ";
                    sFilters += " AND Abiturient.Id NOT IN (select abiturientid from ed.extentryview) ";                    
                      
                    // кроме бэ преодолены мин планки                       
                    sFilters += " AND ((CompetitionId=1  OR CompetitionId=8) OR hlpMinMarkAbiturient.Id IS NULL )";                    

                    string examsCnt = _bdc.GetStringValue(string.Format(" SELECT Count(Id) FROM ed.extExamInEntry WHERE EntryId='{0}' AND ParentExamInEntryBlockId IS NULL", EntryId.ToString()));
                   
                    _queryOrange = @", CASE WHEN EXISTS(SELECT PersonId FROM ed.hlpPersonsWithOriginals WHERE PersonId = Abiturient.PersonId AND EntryId <> Abiturient.EntryId) then 1 else 0 end as orange ";
                        
                    // кроме бэ нужное кол-во оценок есть
                    sFilters += " AND ((CompetitionId=1  OR CompetitionId=8) OR ed.extAbitMarksSum.TotalCount = " + examsCnt + " ) ";

                    totalQuery = _queryBody + _queryOrange + _queryFrom + sFilters + sOrderBy;

                }

                if (!dgvAbits.Columns.Contains("Number"))
                    dgvAbits.Columns.Add("Number", "№ п/п");

                HelpClass.FillDataGrid(dgvAbits, _bdc, totalQuery, "");

                dgvAbits.Columns["Id"].Visible = false;
                dgvAbits.Columns["comp"].Visible = false;
                dgvAbits.Columns["noexamssort"].Visible = false;
                dgvAbits.Columns["preimsort"].Visible = false;
                dgvAbits.Columns["olymp"].Visible = false;
                dgvAbits.Columns["attestat"].Visible = false;
                dgvAbits.Columns["attAvg"].Visible = false;
                dgvAbits.Columns["orange"].Visible = false;

                if (MainClass.dbType == PriemType.PriemMag)
                {
                    dgvAbits.Columns["Серия аттестата"].Visible = false;
                    dgvAbits.Columns["Медалист"].HeaderText = "Красный диплом";
                }
                else
                    dgvAbits.Columns["Серия диплома"].Visible = false;
                
                foreach (DataGridViewColumn column in dgvAbits.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                lblCount.Text = dgvAbits.RowCount.ToString() + "             Cвободных мест: "+plan;
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при обновлении списка.", ex);
            }
        }
      
        private string GetFilterString()
        {
            string s = " WHERE 1=1 ";
            s += " AND extEntry.StudyLevelGroupId = " + StudyLevelGroupId;  
            
            //s += " AND Abiturient.DocDate>='20120813'"; 

            //обработали факультет
            if (FacultyId != null)
                s += " AND extEntry.FacultyId = " + FacultyId;      
            
            //обработали форму обучения  
            if (StudyFormId != null)
                s += " AND extEntry.StudyFormId = " + StudyFormId;

            //обработали основу обучения  
            if (StudyBasisId != null)
                s += " AND extEntry.StudyBasisId = " + StudyBasisId;               

            //обработали Направление
            if (LicenseProgramId != null)
                s += " AND extEntry.LicenseProgramId = " + LicenseProgramId;

            //обработали Образ программу
            if (ObrazProgramId != null)
                s += " AND extEntry.ObrazProgramId = " + ObrazProgramId;

            //обработали профиль
            if (ProfileId != null)
                s += string.Format(" AND extEntry.ProfileId = '{0}'", ProfileId);
            else
                s += " AND extEntry.ProfileId IS NULL";


            s += " AND extEntry.IsSecond = " + (IsSecond ? " 1 " : " 0 ");
            s += " AND extEntry.IsReduced = " + (IsReduced ? " 1 " : " 0 ");
            s += " AND extEntry.IsParallel = " + (IsParallel ? " 1 " : " 0 ");

            return s;
        }

        private void dgvAbits_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dgvAbits.Columns["Number"].Index)
            {
                e.Value = string.Format("{0}", e.RowIndex + 1);
            }

            if (e.RowIndex < plan)
            {
                if (e.ColumnIndex != dgvAbits.Columns["ФИО"].Index)//сперва подкрасим не-фио
                    dgvAbits[e.ColumnIndex, e.RowIndex].Style.BackColor = System.Drawing.Color.LightGreen;
                //потом докрашиваем не-оранжевые фио
                if (e.ColumnIndex == dgvAbits.Columns["ФИО"].Index && dgvAbits["orange", e.RowIndex].Value.ToString() != "1")
                    dgvAbits[e.ColumnIndex, e.RowIndex].Style.BackColor = System.Drawing.Color.LightGreen;
            }
            //и в последнюю очередь - оранжевых
            //это позволяет избежать рекурсивного вызова "перекраски" (сперва ячейка зелёная, а потом оранжевая)
            if (e.ColumnIndex == dgvAbits.Columns["ФИО"].Index && dgvAbits["orange", e.RowIndex].Value.ToString() == "1")
            {
                dgvAbits["ФИО", e.RowIndex].Style.BackColor = System.Drawing.Color.Orange;
            }            
        }

        private void tbNumber_TextChanged(object sender, EventArgs e)
        {
            WinFormsServ.Search(this.dgvAbits, "Рег_номер", tbNumber.Text);
        }

        private void tbFIO_TextChanged(object sender, EventArgs e)
        {
            WinFormsServ.Search(this.dgvAbits, "ФИО", tbFIO.Text);
        }

        private void btnFixieren_Click(object sender, EventArgs e)
        {
            Fixieren();
        }        

        private void Fixieren()
        {
            if (dgvAbits.DataSource == null || dgvAbits.Rows.Count == 0)
                return;

            using (PriemEntities context = new PriemEntities())
            {
                using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    try
                    {
                        Guid? fixViewId = (from fv in context.FixierenView
                                           where fv.StudyLevelGroupId == StudyLevelGroupId && fv.IsReduced == IsReduced && fv.IsParallel == IsParallel && fv.IsSecond == IsSecond
                                           && fv.FacultyId == FacultyId && fv.LicenseProgramId == LicenseProgramId
                                           && fv.ObrazProgramId == ObrazProgramId
                                           && (ProfileId == null ? fv.ProfileId == 0 : fv.ProfileId == ProfileId)
                                           && fv.StudyFormId == StudyFormId
                                           && fv.StudyBasisId == StudyBasisId
                                           && fv.IsCel == IsCel 
                                           && fv.IsQuota == IsQuota
                                           select fv.Id).FirstOrDefault();

                        if (fixViewId != null)
                        {
                            bool? locked = (from fv in context.FixierenView
                                            where fv.Id == fixViewId
                                            select fv.Locked).FirstOrDefault();

                            if (locked.HasValue && locked.Value)
                            {
                                WinFormsServ.Error("Создание представления заблокировано, т.к. уже утверждена предыдущая версия");
                                return;
                            }

                            context.Fixieren_DeleteByFVId(fixViewId);
                            context.FixierenView_Delete(fixViewId);
                        }

                        int rand = new Random().Next(10000, 99999);

                        ObjectParameter fvId = new ObjectParameter("id", typeof(Guid));
                        context.FixierenView_Insert(StudyLevelGroupId, FacultyId, LicenseProgramId, ObrazProgramId, ProfileId, StudyBasisId, StudyFormId, IsSecond, IsReduced, IsParallel, IsCel, rand, false, false, IsQuota, fvId);
                        Guid? viewId = (Guid?)fvId.Value;

                        int counter = 0;
                        foreach (DataGridViewRow row in dgvAbits.Rows)
                        {
                            counter++;
                            Guid? abId = new Guid(row.Cells["Id"].Value.ToString());
                            context.Fixieren_Insert(counter, abId, viewId);
                        }

                        transaction.Complete();                        
                    }
                    catch (Exception ex)
                    {
                        WinFormsServ.Error("Ошибка при сохранении списка", ex);
                        return;
                    }                   
                }

                //ПЕЧАТЬ!
                PrintProtocol();
            }             
        }

        private void PrintProtocol()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ADOBE Pdf files|*.pdf";
            if (sfd.ShowDialog() == DialogResult.OK)
                PriemLib.Print.PrintRatingProtocol(StudyFormId, StudyBasisId, FacultyId, LicenseProgramId, ObrazProgramId, ProfileId, IsCel, 
                    plan, sfd.FileName, IsSecond, IsReduced, IsParallel, false);
        }        

        private void btnWord_Click(object sender, EventArgs e)
        {
            ToWord();
        }

        private void ToWord()
        {
            int rowCount = dgvAbits.Rows.Count;
            if (rowCount == 0)
                return;
            try
            {
                float margin = (float)(20.0m * RtfConstants.MILLIMETERS_TO_POINTS);
                RtfDocument doc = new RtfDocument(PaperSize.A4, PaperOrientation.Landscape, Lcid.Russian, margin, margin, margin, margin);

                RtfTable table = doc.addTable(rowCount + 1, 14, (float)(276.1m * RtfConstants.MILLIMETERS_TO_POINTS));

                // Устанавливаем ширину столбцов таблицы (в миллиметрах)
                table.setColWidth(0, (float)(8.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(1, (float)(18.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(2, (float)(40.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(3, (float)(15.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(4, (float)(15.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(5, (float)(15.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(6, (float)(15.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(7, (float)(18.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(8, (float)(45.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(9, (float)(15.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(10, (float)(15.1m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(11, (float)(15.1m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(12, (float)(15.1m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(13, (float)(15.1m * RtfConstants.MILLIMETERS_TO_POINTS));

                table.cell(0, 0).addParagraph().Text = "№ п/п";
                table.cell(0, 1).addParagraph().Text = "Рег.номер";
                table.cell(0, 2).addParagraph().Text = "ФИО";
                table.cell(0, 3).addParagraph().Text = "Сумма баллов";
                table.cell(0, 4).addParagraph().Text = "Проф. экзамен";
                table.cell(0, 5).addParagraph().Text = "Доп. экзамен";
                table.cell(0, 6).addParagraph().Text = "Конкурс";
                table.cell(0, 7).addParagraph().Text = "Подлинники";
                table.cell(0, 8).addParagraph().Text = "Контакты";
                table.cell(0, 9).addParagraph().Text = "Медалист";
                table.cell(0, 10).addParagraph().Text = "Серия док. об обр.";                
                table.cell(0, 11).addParagraph().Text = "ср. балл";
                table.cell(0, 12).addParagraph().Text = "Ретинг. коэфф.";
                if (dgvAbits.Columns.Contains("Олимпиада"))
                    table.cell(0, 13).addParagraph().Text = "Олимпиада";

                for (int j = 0; j < 14; j++)
                {
                    // Устанавливаем горизонтальное и вертикальное выравнивание текста "по центру" в каждой ячейке таблицы
                    table.cell(0, j).Alignment = Align.Center;
                    table.cell(0, j).AlignmentVertical = AlignVertical.Middle;
                }

                int r = 0;
                foreach (DataGridViewRow row in dgvAbits.Rows)
                {
                    ++r;
                    table.cell(r, 0).addParagraph().Text = r.ToString();
                    table.cell(r, 1).addParagraph().Text = row.Cells["Рег_Номер"].Value.ToString();
                    table.cell(r, 2).addParagraph().Text = row.Cells["ФИО"].Value.ToString();
                    table.cell(r, 3).addParagraph().Text = row.Cells["Сумма баллов"].Value.ToString();
                    table.cell(r, 6).addParagraph().Text = row.Cells["Конкурс"].Value.ToString();
                    table.cell(r, 7).addParagraph().Text = row.Cells["Подлинники документов"].Value.ToString();
                    table.cell(r, 8).addParagraph().Text = row.Cells["Контакты"].Value.ToString();
                    table.cell(r, 9).addParagraph().Text = row.Cells["Медалист"].Value.ToString();
                    table.cell(r, 10).addParagraph().Text = MainClass.dbType == PriemType.PriemMag ? row.Cells["Серия диплома"].Value.ToString() : row.Cells["Серия аттестата"].Value.ToString();
                    table.cell(r, 11).addParagraph().Text = row.Cells["Средний балл"].Value.ToString();
                    table.cell(r, 12).addParagraph().Text = row.Cells["Рейтинговый коэффициент"].Value.ToString();
                    if(dgvAbits.Columns.Contains("Олимпиада"))
                        table.cell(r, 13).addParagraph().Text = row.Cells["Олимпиада"].Value.ToString(); 

                    for (int j = 0; j < 14; j++)
                    {
                        // Устанавливаем горизонтальное и вертикальное выравнивание текста "по центру" в каждой ячейке таблицы
                        table.cell(r, j).Alignment = Align.Center;
                        table.cell(r, j).AlignmentVertical = AlignVertical.Middle;
                    }
                }

                // Задаём толщину внутренних границ таблицы
                table.setInnerBorder(RtfWriter.BorderStyle.Single, 0.5f);
                // Задаём толщину внешних границ таблицы
                table.setOuterBorder(RtfWriter.BorderStyle.Single, 0.5f);
                
                doc.save(MainClass.saveTempFolder + "\\RatingList.rtf");

                // ==========================================================================
                // Открываем сохранённый RTF файл
                // ==========================================================================
                WordDoc wd = new WordDoc(string.Format(@"{0}\RatingList.rtf", MainClass.saveTempFolder));
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при составлении списка:\n" + ex.Message +
                    ex.InnerException == null ? "" : ("\nВнутреннее исключение:\n" + ex.InnerException.Message));
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            LockUnlock(true);
        }

        private void LockUnlock(bool locked)
        {
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    context.FixierenView_UpdateLocked(StudyLevelGroupId, FacultyId, LicenseProgramId, ObrazProgramId, ProfileId, StudyBasisId, StudyFormId, IsSecond, IsReduced, IsParallel, IsCel, false, locked);
                    
                    lblLocked.Text = locked ? "ЗАЛОЧЕНА" : "НЕ залочена";
                }
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при локе/анлоке", ex);
            }
            return;            
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            LockUnlock(false);
        }

        private void btnFixierenWeb_Click(object sender, EventArgs e)
        {
            WebFixieren();
        }

        private void WebFixieren()
        {
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        Guid? fixViewId = (from fv in context.FixierenView
                                           where fv.StudyLevelGroupId == StudyLevelGroupId && fv.IsReduced == IsReduced && fv.IsParallel == IsParallel && fv.IsSecond == IsSecond
                                           && fv.FacultyId == FacultyId && fv.LicenseProgramId == LicenseProgramId
                                           && fv.ObrazProgramId == ObrazProgramId
                                           && (ProfileId == null ? fv.ProfileId == 0 : fv.ProfileId == ProfileId)
                                           && fv.StudyFormId == StudyFormId
                                           && fv.StudyBasisId == StudyBasisId
                                           && fv.IsCel == IsCel
                                           select fv.Id).FirstOrDefault();
                        bool isForeign = MainClass.dbType == PriemType.PriemForeigners;
                        Guid? entryId = (from fv in context.qEntry
                                           where fv.StudyLevelGroupId == StudyLevelGroupId && fv.IsReduced == IsReduced 
                                           && fv.IsParallel == IsParallel && fv.IsSecond == IsSecond
                                           && fv.FacultyId == FacultyId && fv.LicenseProgramId == LicenseProgramId
                                           && fv.ObrazProgramId == ObrazProgramId
                                           && (ProfileId == null ? fv.ProfileId == 0 : fv.ProfileId == ProfileId)
                                           && fv.StudyFormId == StudyFormId
                                           && fv.StudyBasisId == StudyBasisId   
                                           && fv.IsForeign == isForeign
                                           select fv.Id).FirstOrDefault();
                        
                        //удалили старое
                        context.FirstWave_DELETE(entryId, IsCel, false, IsQuota);

                        var fix = from fx in context.Fixieren
                                  where fx.FixierenViewId == fixViewId
                                  select fx;                          
                        
                        //foreach(Fixieren f in fix)
                        int cnt = 0;                    
                        foreach (DataGridViewRow row in dgvAbits.Rows)                        
                        {
                            //cnt++;
                            //Guid? abId = new Guid(row.Cells["Id"].Value.ToString());
                            //context.FirstWave_INSERT(abId, cnt);
                            ////context.FirstWave_INSERT(f.AbiturientId, f.Number);
                            cnt++;
                            Guid? abId = new Guid(row.Cells["Id"].Value.ToString());
                            if (!chbCel.Checked)
                            {
                                if (!IsQuota)
                                    context.FirstWave_INSERT(abId, cnt);
                                else
                                    context.FirstWave_INSERTQUOTA(abId, cnt);

                            }
                            else
                                context.FirstWave_INSERTCEL(abId, cnt);
                            //context.FirstWave_INSERT(f.AbiturientId, f.Number);
                        }
                        transaction.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при WEB FIXIEREN !", ex);
            }
            MessageBox.Show("DONE!");
        }        

        private void btnUnfix_Click(object sender, EventArgs e)
        {
            Unfixieren();
        }

        private void Unfixieren()
        {
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    Guid? entryId = (from fv in context.qEntry
                                     where fv.StudyLevelGroupId == StudyLevelGroupId && fv.IsReduced == IsReduced && fv.IsParallel == IsParallel && fv.IsSecond == IsSecond
                                     && fv.FacultyId == FacultyId && fv.LicenseProgramId == LicenseProgramId
                                     && fv.ObrazProgramId == ObrazProgramId
                                     && (ProfileId == null ? fv.ProfileId == 0 : fv.ProfileId == ProfileId)
                                     && fv.StudyFormId == StudyFormId
                                     && fv.StudyBasisId == StudyBasisId
                                     select fv.Id).FirstOrDefault();
                    
                    //удалили
                    context.FirstWave_DELETE(entryId, IsCel, false, IsQuota);
                }
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при WEB FIXIEREN !", ex);
            }
            MessageBox.Show("DONE!");
        }

        private void btnDeleteAb_Click(object sender, EventArgs e)
        {
            if (MainClass.IsPasha())
            {
                using (PriemEntities context = new PriemEntities())
                {
                    if (MessageBox.Show("Удалить из рейтингового списка?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            foreach (DataGridViewRow dgvr in dgvAbits.SelectedRows)
                            {
                                Guid abId = new Guid(dgvr.Cells["Id"].Value.ToString());
                                try
                                {
                                    context.Fixieren_DELETE(abId);
                                    context.FirstWave_DeleteByAbId(abId);
                                }
                                catch (Exception ex)
                                {
                                    WinFormsServ.Error("Ошибка удаления данных" + ex.Message);
                                }
                            }

                            transaction.Complete();
                        }   
                        UpdateDataGrid();
                    }
                }
            }
        }

        private void btnUpdateGrid_Click(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        private void chbIsReduced_CheckedChanged(object sender, EventArgs e)
        {
            FillStudyForm();
            NullDataGrid();
        }

        private void chbIsParallel_CheckedChanged(object sender, EventArgs e)
        {
            FillStudyForm();
            NullDataGrid();
        }

        private void chbIsSecond_CheckedChanged(object sender, EventArgs e)
        {
            FillStudyForm();
            NullDataGrid();
        }

        private void chbCel_CheckedChanged(object sender, EventArgs e)
        {
            NullDataGrid();

            if (IsQuota)
                chbIsQuota.Checked = false;

            /*if (IsCel)
                btnFixierenWeb.Enabled = false;
            else
                btnFixierenWeb.Enabled = true;*/
        }             
    }
}
