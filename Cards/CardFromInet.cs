using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data.Objects;
using System.Transactions;

using BaseFormsLib;
using EducServLib;
using PriemLib;


namespace Priem
{
    public partial class CardFromInet : CardFromList
    {        
        private DBPriem _bdcInet;    
        private int? _abitBarc;
        private int? _personBarc;

        private Guid? personId;
        private bool _closePerson;
        private bool _closeAbit;

        LoadFromInet load;
        private List<ShortCompetition> LstCompetitions;

        private DocsClass _docs;   

        // конструктор формы
        public CardFromInet(int? personBarcode, int? abitBarcode, bool closeAbit)
        {
            InitializeComponent();
            _Id = null;
           
            _abitBarc = abitBarcode;
            _personBarc = personBarcode;
            _closeAbit = closeAbit;
            tcCard = tabCard;

            if (_abitBarc == null)
                _closeAbit = true;

            InitControls();     
        }      

        protected override void ExtraInit()
        { 
            base.ExtraInit();

            load = new LoadFromInet();
            _bdcInet = load.BDCInet;
            
            _bdc = MainClass.Bdc;
            _isModified = true;

            if (_personBarc == null)
            {
                _personBarc = (int?)_bdcInet.GetValue(string.Format("SELECT Person.Barcode FROM qAbiturient INNER JOIN Person ON qAbiturient.PersonId = Person.Id WHERE qAbiturient.CommitNumber = {0}", _abitBarc));
                if (!_personBarc.HasValue)
                {
                    WinFormsServ.Error("Не удалось найти запись человека. Вероятно, заявление было удалено.");
                    return;
                }
            }

            lblBarcode.Text = _personBarc.ToString();
            if (_abitBarc != null)
                lblBarcode.Text += @"\" + _abitBarc.ToString();

            _docs = new DocsClass(_personBarc.Value, _abitBarc);

            tbNum.Enabled = false;

            rbMale.Checked = true;
            chbIsEqual.Visible = false;

            chbHostelAbitYes.Checked = false;
            chbHostelAbitNo.Checked = false;
            chbHostelEducYes.Checked = false;
            chbHostelEducNo.Checked = false;
            cbHEQualification.DropDownStyle = ComboBoxStyle.DropDown;
            
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    ComboServ.FillCombo(cbPassportType, HelpClass.GetComboListByTable("ed.PassportType"), true, false);
                    ComboServ.FillCombo(cbCountry, HelpClass.GetComboListByTable("ed.Country", "ORDER BY Distance, Name"), true, false);
                    ComboServ.FillCombo(cbNationality, HelpClass.GetComboListByTable("ed.Country", "ORDER BY Distance, Name"), true, false);
                    ComboServ.FillCombo(cbRegion, HelpClass.GetComboListByTable("ed.Region", "ORDER BY Distance, Name"), true, false);
                    ComboServ.FillCombo(cbLanguage, HelpClass.GetComboListByTable("ed.Language"), true, false);
                    ComboServ.FillCombo(cbCountryEduc, HelpClass.GetComboListByTable("ed.Country", "ORDER BY Distance, Name"), true, false);
                    ComboServ.FillCombo(cbRegionEduc, HelpClass.GetComboListByTable("ed.Region", "ORDER BY Distance, Name"), true, false);
                    
                    ComboServ.FillCombo(cbHEStudyForm, HelpClass.GetComboListByTable("ed.StudyForm"), true, false);
                    ComboServ.FillCombo(cbSportQualification, HelpClass.GetComboListByTable("ed.SportQualification"), false, false);

                    //DataTable tbl = _bdcInet.GetDataSet("SELECT DISTINCT SchoolCity AS Name FROM extPerson_SPO WHERE SchoolCity > '' ORDER BY 1").Tables[0];
                    //List<KeyValuePair<string, string>> kvp = new List<KeyValuePair<string, string>>();

                    //kvp = (from DataRow rw in tbl.Rows
                    //       select new KeyValuePair<string, string>(rw.Field<string>("Name"), rw.Field<string>("Name"))).ToList();
                    //ComboServ.FillCombo(cbSchoolCity, kvp, false, false);

                    //tbl = _bdcInet.GetDataSet("SELECT DISTINCT AttestatSeries AS Name FROM extPerson_SPO WHERE AttestatSeries > '' ORDER BY 1").Tables[0];
                    //kvp = (from DataRow rw in tbl.Rows
                    //       select new KeyValuePair<string, string>(rw.Field<string>("Name"), rw.Field<string>("Name"))).ToList();
                    //ComboServ.FillCombo(cbAttestatSeries, kvp, false, false);

                    //tbl = _bdcInet.GetDataSet("SELECT DISTINCT HEQualification AS Name FROM extPerson_SPO WHERE NOT HEQualification IS NULL AND HEQualification > '' ORDER BY 1").Tables[0];
                    //kvp = (from DataRow rw in tbl.Rows
                    //       select new KeyValuePair<string, string>(rw.Field<string>("Name"), rw.Field<string>("Name"))).ToList();
                    //ComboServ.FillCombo(cbHEQualification, kvp, false, false);

                    cbSchoolCity.DataSource = context.ExecuteStoreQuery<string>("SELECT DISTINCT ed.Person_EducationInfo.SchoolCity AS Name FROM ed.Person_EducationInfo WHERE ed.Person_EducationInfo.SchoolCity > '' ORDER BY 1");
                    cbAttestatSeries.DataSource = context.ExecuteStoreQuery<string>("SELECT DISTINCT ed.Person_EducationInfo.AttestatSeries AS Name FROM ed.Person_EducationInfo WHERE ed.Person_EducationInfo.AttestatSeries > '' ORDER BY 1");
                    cbHEQualification.DataSource = context.ExecuteStoreQuery<string>("SELECT DISTINCT ed.Person_EducationInfo.HEQualification AS Name FROM ed.Person_EducationInfo WHERE NOT ed.Person_EducationInfo.HEQualification IS NULL AND ed.Person_EducationInfo.HEQualification > '' ORDER BY 1");

                    cbAttestatSeries.SelectedIndex = -1;
                    cbSchoolCity.SelectedIndex = -1;
                    cbHEQualification.SelectedIndex = -1;
                 

                    ComboServ.FillCombo(cbLanguage, HelpClass.GetComboListByTable("ed.Language"), true, false); 
                     
                }

                //tpDocs.Parent = null; 
                ComboServ.FillCombo(cbSchoolType, HelpClass.GetComboListByTable("ed.SchoolType", "ORDER BY 1"), true, false);

                //if (_closeAbit)
                //    tpApplication.Parent = null;
            }
            catch (Exception exc)
            {
                WinFormsServ.Error("Ошибка при инициализации формы " + exc.Message);
            }
        }

        protected override bool IsForReadOnly()
        {
            return !MainClass.RightsToEditCards();
        }
        
        #region handlers      

        //инициализация обработчиков мегакомбов
        protected override void InitHandlers()
        {  
            cbSchoolType.SelectedIndexChanged += new EventHandler(UpdateAfterSchool);
            cbCountry.SelectedIndexChanged += new EventHandler(UpdateAfterCountry);
            cbCountryEduc.SelectedIndexChanged += new EventHandler(UpdateAfterCountryEduc);
        }

        protected override void NullHandlers()
        {
            cbSchoolType.SelectedIndexChanged -= new EventHandler(UpdateAfterSchool);
            cbCountry.SelectedIndexChanged -= new EventHandler(UpdateAfterCountry);
            cbCountryEduc.SelectedIndexChanged -= new EventHandler(UpdateAfterCountryEduc);
        }
         
        private IEnumerable<qEntry> GetEntry(PriemEntities context)
        {
            IEnumerable<qEntry> entry = MainClass.GetEntry(context);

            return entry;
        }

        private void UpdateAfterSchool(object sender, EventArgs e)
        {
            if (SchoolTypeId == MainClass.educSchoolId)
            {
                gbAtt.Visible = true;
                gbDipl.Visible = false;
                tbSchoolName.Width = 217;
            }               
            else
            {
                if (SchoolTypeId == 4)
                    tbSchoolName.Width = 281;
                else
                    tbSchoolName.Width = 217;
                gbAtt.Visible = false;
                gbDipl.Visible = true;
            }
        }
        private void UpdateAfterCountry(object sender, EventArgs e)
        {
            if (CountryId == MainClass.countryRussiaId)
            {
                cbRegion.Enabled = true;
                cbRegion.SelectedItem = "нет";
            }
            else
            {
                cbRegion.Enabled = false;
                cbRegion.SelectedItem = "нет";
            }
        }
        private void UpdateAfterCountryEduc(object sender, EventArgs e)
        {
            if (CountryEducId == MainClass.countryRussiaId)
            {
                chbIsEqual.Visible = false;
                label66.Visible = false;
                tbEqualDocumentNumber.Visible = false;
                cbRegionEduc.Enabled = true; 
            }
            else
            {
                chbIsEqual.Visible = true;
                label66.Visible = true;
                tbEqualDocumentNumber.Visible = true;
                cbRegionEduc.Enabled = false;
                cbRegionEduc.SelectedIndex = 0; 
            }
        }

        private void chbHostelAbitYes_CheckedChanged(object sender, EventArgs e)
        {
            chbHostelAbitNo.Checked = !chbHostelAbitYes.Checked;           
        }
        private void chbHostelAbitNo_CheckedChanged(object sender, EventArgs e)
        {
            chbHostelAbitYes.Checked = !chbHostelAbitNo.Checked;
        }

        #endregion

        private void UpdateCommitCompetition(ShortCompetition comp)
        {
            int ind = LstCompetitions.FindIndex(x => comp.Id == x.Id);
            if (ind > -1)
            {
                LstCompetitions[ind].HasCompetition = true;
                LstCompetitions[ind].CompetitionId = comp.CompetitionId;
                LstCompetitions[ind].CompetitionName = comp.CompetitionName;
                LstCompetitions[ind].FacultyId = comp.FacultyId;
                LstCompetitions[ind].FacultyName = comp.FacultyName;
                LstCompetitions[ind].LicenseProgramId = comp.LicenseProgramId;
                LstCompetitions[ind].LicenseProgramName = comp.LicenseProgramName;
                LstCompetitions[ind].ObrazProgramId = comp.ObrazProgramId;
                LstCompetitions[ind].ObrazProgramName = comp.ObrazProgramName;
                LstCompetitions[ind].ProfileId = comp.ProfileId;
                LstCompetitions[ind].ProfileName = comp.ProfileName;

                LstCompetitions[ind].DocInsertDate = comp.DocInsertDate;
                LstCompetitions[ind].IsGosLine = comp.IsGosLine;
                LstCompetitions[ind].IsListener = comp.IsListener;
                LstCompetitions[ind].IsSecond = comp.IsSecond;

                LstCompetitions[ind].StudyFormId = comp.StudyFormId;
                LstCompetitions[ind].StudyFormName = comp.StudyFormName;
                LstCompetitions[ind].StudyBasisId = comp.StudyBasisId;
                LstCompetitions[ind].StudyBasisName = comp.StudyBasisName;
                LstCompetitions[ind].StudyLevelId = comp.StudyLevelId;
                LstCompetitions[ind].StudyLevelName = comp.StudyLevelName;

                LstCompetitions[ind].HasCompetition = comp.HasCompetition;
                LstCompetitions[ind].ChangeEntry();

                UpdateApplicationGrid();
            }
        }
        protected override void FillCard()
        {
            if (!_personBarc.HasValue)
                return;
            try
            {
                FillPersonData(GetPerson());
                FillApplication();
                FillFiles();
            }
            catch (DataException de)
            {
                WinFormsServ.Error("Ошибка при заполнении формы " + de.Message);
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при заполнении формы " + ex.Message);
            }
        }

        private void FillFiles()
        {
            List<KeyValuePair<string, string>> lstFiles = _docs.UpdateFiles();
            if (lstFiles == null || lstFiles.Count == 0)
                return;

            chlbFile.DataSource = new BindingSource(lstFiles, null);
            chlbFile.ValueMember = "Key";
            chlbFile.DisplayMember = "Value";   
        }

        private extPersonSPO GetPerson()
        {
            if (_personBarc == null)
                return null;

            try
            {
                if (!MainClass.CheckPersonBarcode(_personBarc))
                {
                    _closePerson = true;

                    using (PriemEntities context = new PriemEntities())
                    {
                        extPersonSPO person = (from pers in context.extPersonSPO
                                            where pers.Barcode == _personBarc
                                            select pers).FirstOrDefault();

                        personId = person.Id;

                        tbNum.Text = person.PersonNum.ToString();
                        this.Text = "ПРОВЕРКА ДАННЫХ " + person.FIO;
                        
                        return person;
                    }
                }
                else
                {
                    if (_personBarc == 0)
                        return null;

                    _closePerson = false;
                    personId = null;

                    tcCard.SelectedIndex = 0;
                    tbSurname.Focus();

                    extPersonSPO person = load.GetPersonByBarcode(_personBarc.Value); 
                    
                    this.Text = "ЗАГРУЗКА " + person.FIO;
                    return person;
                }
            }

            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при заполнении формы " + ex.Message);
                return null;
            }       
        }
        private void FillPersonData(extPersonSPO person)
        {
            if (person == null)
            {
                WinFormsServ.Error("Не найдены записи!");
                _isModified = false;
                this.Close();
            }

            try
            {
                PersonName = person.Name;
                SecondName = person.SecondName;
                Surname = person.Surname;
                BirthDate = person.BirthDate;
                BirthPlace = person.BirthPlace;
                PassportTypeId = person.PassportTypeId;
                PassportSeries = person.PassportSeries;
                PassportNumber = person.PassportNumber;
                PassportAuthor = person.PassportAuthor;
                PassportDate = person.PassportDate;
                PassportCode = person.PassportCode;
                PersonalCode = person.PersonalCode;
                SNILS = person.SNILS;
                Sex = person.Sex;
                CountryId = person.CountryId;
                NationalityId = person.NationalityId;
                RegionId = person.RegionId;
                Phone = person.Phone;
                Mobiles = person.Mobiles;
                Email = person.Email;
                Code = person.Code;
                City = person.City;
                Street = person.Street;
                House = person.House;
                Korpus = person.Korpus;
                Flat = person.Flat;
                CodeReal = person.CodeReal;
                CityReal = person.CityReal;
                StreetReal = person.StreetReal;
                HouseReal = person.HouseReal;
                KorpusReal = person.KorpusReal;
                FlatReal = person.FlatReal;
                KladrCode = person.KladrCode;
                HostelAbit = person.HostelAbit ?? false;
                HostelEduc = person.HostelEduc ?? false;
                IsExcellent = person.IsExcellent ?? false;
                LanguageId = person.LanguageId;
                SchoolCity = person.SchoolCity;
                SchoolTypeId = person.SchoolTypeId;
                SchoolName = person.SchoolName;
                SchoolNum = person.SchoolNum;
                SchoolExitYear = person.SchoolExitYear;
                CountryEducId = person.CountryEducId;
                RegionEducId = person.RegionEducId;
                IsEqual = person.IsEqual ?? false;
                AttestatRegion = person.AttestatRegion;
                AttestatSeries = person.AttestatSeries;
                AttestatNum = person.AttestatNum;
                DiplomSeries = person.DiplomSeries;
                DiplomNum = person.DiplomNum;
                SchoolAVG = person.SchoolAVG;
                HighEducation = person.HighEducation;
                HEProfession = person.HEProfession;
                HEQualification = person.HEQualification;
                HEEntryYear = person.HEEntryYear;
                HEExitYear = person.HEExitYear;
                HEWork = person.HEWork;
                HEStudyFormId = person.HEStudyFormId;
                Stag = person.Stag;
                WorkPlace = person.WorkPlace;
                Privileges = person.Privileges;
                ExtraInfo = person.ExtraInfo;
                PersonInfo = person.PersonInfo;
                StartEnglish = person.StartEnglish ?? false;
                EnglishMark = person.EnglishMark;

                IsEqual = person.IsEqual ?? false;
                EqualDocumentNumber = person.EqualDocumentNumber; 

                SportQualificationId = person.SportQualificationId;
                SportQualificationLevel = person.SportQualificationLevel;
                OtherSportQualification = person.OtherSportQualification;

                if (MainClass.dbType == PriemType.Priem)
                {
                    DataTable dtEge = load.GetPersonEgeByBarcode(_personBarc.Value);
                    FillEgeFirst(dtEge);
                }
            }
            catch (DataException de)
            {
                WinFormsServ.Error("Ошибка при заполнении формы (DataException)" + de.Message);
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при заполнении формы " + ex.Message);
            } 
        }
        public void FillApplication()
        {
            try
            {
                string query =
@"SELECT Abiturient.[Id]
,[Priority]
,[PersonId]
,[Priority]
,[Barcode]
,[DateOfStart]
,[EntryId]
,[FacultyId]
,[FacultyName]
,[LicenseProgramId]
,[LicenseProgramCode]
,[LicenseProgramName]
,[ObrazProgramId]
,[ObrazProgramCrypt]
,[ObrazProgramName]
,[ProfileId]
,[ProfileName]
,[StudyBasisId]
,[StudyBasisName]
,[StudyFormId]
,[StudyFormName]
,[StudyLevelId]
,[StudyLevelName]
,[IsSecond]
,[IsReduced]
,[IsParallel]
,[IsGosLine]
,[CommitId]
,[DateOfStart]
,(SELECT MAX(ApplicationCommitVersion.Id) FROM ApplicationCommitVersion WHERE ApplicationCommitVersion.CommitId = [Abiturient].CommitId) AS VersionNum
,(SELECT MAX(ApplicationCommitVersion.VersionDate) FROM ApplicationCommitVersion WHERE ApplicationCommitVersion.CommitId = [Abiturient].CommitId) AS VersionDate
,ApplicationCommit.IntNumber
,[Abiturient].HasInnerPriorities
FROM [Abiturient] 
INNER JOIN ApplicationCommit ON ApplicationCommit.Id = Abiturient.CommitId
WHERE IsCommited = 1 AND IntNumber=@CommitId";

                DataTable tbl = _bdcInet.GetDataSet(query, new SortedList<string, object>() { { "@CommitId", _abitBarc } }).Tables[0];

                LstCompetitions =
                         (from DataRow rw in tbl.Rows
                          select new ShortCompetition(rw.Field<Guid>("Id"), rw.Field<Guid>("CommitId"), rw.Field<Guid>("EntryId"), rw.Field<Guid>("PersonId"),
                              rw.Field<int?>("VersionNum"), rw.Field<DateTime?>("VersionDate"))
                          {
                              CompetitionId = rw.Field<int>("StudyBasisId") == 1 ? 4 : 3,
                              CompetitionName = "не указана",
                              HasCompetition = false,
                              LicenseProgramId = rw.Field<int>("LicenseProgramId"),
                              LicenseProgramName = rw.Field<string>("LicenseProgramName"),
                              ObrazProgramId = rw.Field<int>("ObrazProgramId"),
                              ObrazProgramName = rw.Field<string>("ObrazProgramName"),
                              ProfileId = rw.Field<Guid?>("ProfileId"),
                              ProfileName = rw.Field<string>("ProfileName"),
                              StudyBasisId = rw.Field<int>("StudyBasisId"),
                              StudyBasisName = rw.Field<string>("StudyBasisName"),
                              StudyFormId = rw.Field<int>("StudyFormId"),
                              StudyFormName = rw.Field<string>("StudyFormName"),
                              StudyLevelId = rw.Field<int>("StudyLevelId"),
                              StudyLevelName = rw.Field<string>("StudyLevelName"),
                              FacultyId = rw.Field<int>("FacultyId"),
                              FacultyName = rw.Field<string>("FacultyName"),
                              DocDate = rw.Field<DateTime>("DateOfStart"),
                              Priority = rw.Field<int>("Priority"),
                              IsGosLine = rw.Field<bool>("IsGosLine"),
                              HasInnerPriorities = rw.Field<bool>("HasInnerPriorities"),
                              lstObrazProgramsInEntry = new List<ShortObrazProgramInEntry>()
                          }).ToList();

                if (LstCompetitions.Count == 0)
                {
                    WinFormsServ.Error("Заявления отсутствуют!");
                    _isModified = false;
                    this.Close();
                }

                tbApplicationVersion.Text = (LstCompetitions[0].VersionNum.HasValue ? "№ " + LstCompetitions[0].VersionNum.Value.ToString() : "n/a") +
                    (LstCompetitions[0].VersionDate.HasValue ? (" от " + LstCompetitions[0].VersionDate.Value.ToShortDateString() + " " + LstCompetitions[0].VersionDate.Value.ToShortTimeString()) : "n/a");


                //ObrazProgramInEntry
                foreach (var C in LstCompetitions.Where(x => x.HasInnerPriorities))
                {
                    C.lstObrazProgramsInEntry = new List<ShortObrazProgramInEntry>();
                    query = @"SELECT ObrazProgramInEntryId, ObrazProgramInEntryPriority, ObrazProgramName, ProfileInObrazProgramInEntryId, ProfileInObrazProgramInEntryPriority, ProfileName, CurrVersion, CurrDate
FROM [extApplicationDetails] WHERE [ApplicationId]=@AppId";
                    tbl = _bdcInet.GetDataSet(query, new SortedList<string, object>() { { "@AppId", C.Id } }).Tables[0];

                    var data = from DataRow rw in tbl.Rows
                               select new
                               {
                                   ObrazProgramInEntryId = rw.Field<Guid>("ObrazProgramInEntryId"),
                                   ObrazProgramInEntryPriority = rw.Field<int>("ObrazProgramInEntryPriority"),
                                   ObrazProgramName = rw.Field<string>("ObrazProgramName"),
                                   ProfileInObrazProgramInEntryId = rw.Field<Guid?>("ProfileInObrazProgramInEntryId"),
                                   ProfileInObrazProgramInEntryPriority = rw.Field<int?>("ProfileInObrazProgramInEntryPriority"),
                                   ProfileName = rw.Field<string>("ProfileName"),
                                   CurrVersion = rw.Field<int>("CurrVersion"),
                                   CurrDate = rw.Field<DateTime>("CurrDate")
                               };

                    foreach (var OPIE in data.Select(x => new { x.ObrazProgramInEntryId, x.ObrazProgramInEntryPriority, x.ObrazProgramName, x.CurrDate, x.CurrVersion }).Distinct())
                    {
                        var OP = new ShortObrazProgramInEntry(OPIE.ObrazProgramInEntryId, OPIE.ObrazProgramName) { ObrazProgramInEntryPriority = OPIE.ObrazProgramInEntryPriority, CurrVersion = OPIE.CurrVersion, CurrDate = OPIE.CurrDate };
                        OP.ListProfiles = new List<ShortProfileInObrazProgramInEntry>();
                        foreach (var PROF in data.Where(x => x.ObrazProgramInEntryId == OPIE.ObrazProgramInEntryId && x.ProfileInObrazProgramInEntryId.HasValue).Select(x => new { x.ProfileInObrazProgramInEntryId, x.ProfileInObrazProgramInEntryPriority, x.ProfileName }))
                        {
                            OP.ListProfiles.Add(new ShortProfileInObrazProgramInEntry(PROF.ProfileInObrazProgramInEntryId.Value, PROF.ProfileName) { ProfileInObrazProgramInEntryPriority = PROF.ProfileInObrazProgramInEntryPriority.Value });
                        }

                        C.lstObrazProgramsInEntry.Add(OP);
                    }
                }

                UpdateApplicationGrid();

                //if (_closeAbit || _abitBarc == null)
                //    return;
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при заполнении формы заявления" + ex.Message);
            }
        }

        private void UpdateApplicationGrid()
        {
            dgvApplications.DataSource = LstCompetitions.OrderBy(x => x.Priority)
                .Select(x => new
                {
                    x.Id,
                    x.Priority,
                    x.LicenseProgramName,
                    x.ObrazProgramName,
                    x.ProfileName,
                    x.StudyFormName,
                    x.StudyBasisName,
                    x.HasCompetition,
                    comp = x.lstObrazProgramsInEntry.Count > 0 ? "приоритеты" : ""
                }).ToList();
            dgvApplications.Columns["Id"].Visible = false;
            dgvApplications.Columns["Priority"].HeaderText = "Приор";
            dgvApplications.Columns["Priority"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            dgvApplications.Columns["LicenseProgramName"].HeaderText = "Направление";
            dgvApplications.Columns["ObrazProgramName"].HeaderText = "Образ. программа";
            dgvApplications.Columns["ProfileName"].HeaderText = "Профиль";
            dgvApplications.Columns["StudyFormName"].HeaderText = "Форма обуч";
            dgvApplications.Columns["StudyBasisName"].HeaderText = "Основа обуч";
            dgvApplications.Columns["comp"].HeaderText = "";
            dgvApplications.Columns["HasCompetition"].Visible = false;
        }

        protected override void SetReadOnlyFieldsAfterFill()
        {
            base.SetReadOnlyFieldsAfterFill();
            
            if (_closePerson)
            {
                tcCard.SelectedTab = tpApplication;

                foreach (TabPage tp in tcCard.TabPages)
                {
                    if (tp != tpApplication && tp != tpDocs)
                    {
                        foreach (Control control in tp.Controls)
                        {
                            control.Enabled = false;
                            foreach (Control crl in control.Controls)
                                crl.Enabled = false;
                        }
                    }
                }
            }
        }
       
        private void FillEgeFirst(DataTable dtEge)
        {
            if (MainClass.dbType == PriemType.PriemMag)
                return;
            
            try
            {                
                DataTable examTable = new DataTable();

                DataColumn clm;
                clm = new DataColumn();
                clm.ColumnName = "Предмет";
                clm.ReadOnly = true;
                examTable.Columns.Add(clm);

                clm = new DataColumn();
                clm.ColumnName = "ExamId";
                clm.ReadOnly = true;
                examTable.Columns.Add(clm);

                clm = new DataColumn();
                clm.ColumnName = "Баллы";
                examTable.Columns.Add(clm);

                clm = new DataColumn();
                clm.ColumnName = "Номер сертификата";
                examTable.Columns.Add(clm);

                clm = new DataColumn();
                clm.ColumnName = "Типографский номер";
                examTable.Columns.Add(clm);

                clm = new DataColumn();
                clm.ColumnName = "EgeCertificateId";
                examTable.Columns.Add(clm);


                string defQuery = "SELECT ed.EgeExamName.Name AS 'Предмет', ed.EgeExamName.Id AS ExamId FROM ed.EgeExamName";
                DataSet ds = _bdc.GetDataSet(defQuery);
                foreach (DataRow dsRow in ds.Tables[0].Rows)
                {
                    DataRow newRow;
                    newRow = examTable.NewRow();
                    newRow["Предмет"] = dsRow["Предмет"].ToString();
                    newRow["ExamId"] = dsRow["ExamId"].ToString();
                    examTable.Rows.Add(newRow);
                }

                foreach (DataRow dsRow in dtEge.Rows)
                {
                    for (int i = 0; i < examTable.Rows.Count; i++)
                    {
                        if (examTable.Rows[i]["ExamId"].ToString() == dsRow["ExamId"].ToString())
                        {
                            examTable.Rows[i]["Баллы"] = dsRow["Value"].ToString();
                            examTable.Rows[i]["Номер сертификата"] = dsRow["Number"].ToString();                            
                        }
                    }
                }

                DataView dv = new DataView(examTable);
                dv.AllowNew = false;

                dgvEGE.DataSource = dv;
                dgvEGE.Columns["ExamId"].Visible = false;
                dgvEGE.Columns["EgeCertificateId"].Visible = false;               

                dgvEGE.Columns["Предмет"].Width = 162;
                dgvEGE.Columns["Баллы"].Width = 45;
                dgvEGE.Columns["Номер сертификата"].Width = 110;
                dgvEGE.ReadOnly = false;

                dgvEGE.Update();
            }
            catch (DataException de)
            {
                WinFormsServ.Error("Ошибка при заполнении формы " + de.Message);
            }
        }

        #region Save

        // проверка на уникальность абитуриента
        private bool CheckIdent()
        {
            using (PriemEntities context = new PriemEntities())
            {
                ObjectParameter boolPar = new ObjectParameter("result", typeof(bool));

                if(_Id == null)
                    context.CheckPersonIdent(Surname, PersonName, SecondName, BirthDate, PassportSeries, PassportNumber, AttestatRegion, AttestatSeries, AttestatNum, boolPar);
                else
                    context.CheckPersonIdentWithId(Surname, PersonName, SecondName, BirthDate, PassportSeries, PassportNumber, AttestatRegion, AttestatSeries, AttestatNum, GuidId, boolPar);

                return Convert.ToBoolean(boolPar.Value);
            }
        }

        protected override bool CheckFields()
        {
            if (Surname.Length <= 0)
            {
                epError.SetError(tbSurname, "Отсутствует фамилия абитуриента");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (PersonName.Length <= 0)
            {
                epError.SetError(tbName, "Отсутствует имя абитуриента");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            //Для О'Коннор сделал добавку в регулярное выражение: \'
            if (!Regex.IsMatch(Surname, @"^[A-Za-zА-Яа-яёЁ\-\'\s]+$"))
            {
                epError.SetError(tbSurname, "Неправильный формат");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (!Regex.IsMatch(PersonName, @"^[A-Za-zА-Яа-яёЁ\-\'\s]+$"))
            {
                epError.SetError(tbName, "Неправильный формат");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (!Regex.IsMatch(SecondName, @"^[A-Za-zА-Яа-яёЁ\-\'\s]+$"))
            {
                epError.SetError(tbSecondName, "Неправильный формат");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (SecondName.StartsWith("-"))
            {
                SecondName = SecondName.Replace("-", "");
            }

            if (BirthDate == null)
            {
                epError.SetError(dtBirthDate, "Неправильно указана дата");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            int checkYear = DateTime.Now.Year - 12;
            if (BirthDate.Value.Year > checkYear || BirthDate.Value.Year < 1920)
            {
                epError.SetError(dtBirthDate, "Неправильно указана дата");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (PassportDate.Value.Year > DateTime.Now.Year || PassportDate.Value.Year < 1970)
            {
                epError.SetError(dtPassportDate, "Неправильно указана дата");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (PassportTypeId == MainClass.pasptypeRFId)
            {
                if (!(PassportSeries.Length == 4))
                {
                    epError.SetError(tbPassportSeries, "Неправильно введена серия паспорта РФ абитуриента");
                    tabCard.SelectedIndex = 0;
                    return false;
                }
                else
                    epError.Clear();

                if (!(PassportNumber.Length == 6))
                {
                    epError.SetError(tbPassportNumber, "Неправильно введен номер паспорта РФ абитуриента");
                    tabCard.SelectedIndex = 0;
                    return false;
                }
                else
                    epError.Clear();
            }

            if (NationalityId == MainClass.countryRussiaId)
            {
                if (PassportSeries.Length <= 0)
                {
                    epError.SetError(tbPassportSeries, "Отсутствует серия паспорта абитуриента");
                    tabCard.SelectedIndex = 0;
                    return false;
                }
                else
                    epError.Clear();

                if (PassportNumber.Length <= 0)
                {
                    epError.SetError(tbPassportNumber, "Отсутствует номер паспорта абитуриента");
                    tabCard.SelectedIndex = 0;
                    return false;
                }
                else
                    epError.Clear();
            }

            if (PassportSeries.Length > 10)
            {
                epError.SetError(tbPassportSeries, "Слишком длинное значение серии паспорта абитуриента");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();


            if (PassportNumber.Length > 20)
            {
                epError.SetError(tbPassportNumber, "Слишком длинное значение номера паспорта абитуриента");
                tabCard.SelectedIndex = 0;
                return false;
            }
            else
                epError.Clear();

            if (!chbHostelAbitYes.Checked && !chbHostelAbitNo.Checked)
            {
                epError.SetError(chbHostelEducNo, "Не указаны данные о предоставлении общежития");
                tabCard.SelectedIndex = 1;
                return false;
            }
            else
                epError.Clear();

            if (!chbHostelEducYes.Checked && !chbHostelEducNo.Checked)
            {
                epError.SetError(chbHostelEducNo, "Не указаны данные о предоставлении общежития");
                tabCard.SelectedIndex = 1;
                return false;
            }
            else
                epError.Clear();

            if (!Regex.IsMatch(SchoolExitYear.ToString(), @"^\d{0,4}$"))
            {
                epError.SetError(tbSchoolExitYear, "Неправильно указан год");
                tabCard.SelectedIndex = 2;
                return false;
            }
            else
                epError.Clear();

            if (gbAtt.Visible && AttestatNum.Length <= 0)
            {
                epError.SetError(tbAttestatNum, "Отсутствует номер аттестата абитуриента");
                tabCard.SelectedIndex = 2;
                return false;
            }
            else
                epError.Clear();

            double d = 0;
            if (tbSchoolAVG.Text.Trim() != "")
            {
                if (!double.TryParse(tbSchoolAVG.Text.Trim().Replace(".", ","), out d))
                {
                    epError.SetError(tbSchoolAVG, "Неправильный формат");
                    tabCard.SelectedIndex = 2;
                    return false;
                }
                else
                    epError.Clear();
            }
            
            if (tbExtraInfo.Text.Length >= 4000)
            {
                epError.SetError(tbExtraInfo, "Длина поля превышает 4000 символов. Укажите только самое основное.");
                tabCard.SelectedIndex = MainClass.dbType == PriemType.Priem ? 4 : 3;
                return false;
            }
            else
                epError.Clear();

            if (tbPersonInfo.Text.Length > 4000)
            {
                epError.SetError(tbPersonInfo, "Длина поля превышает 4000 символов. Укажите только самое основное.");
                tabCard.SelectedIndex = MainClass.dbType == PriemType.Priem ? 4 : 3;
                return false;
            }
            else
                epError.Clear();

            if (tbWorkPlace.Text.Length > 500)
            {
                epError.SetError(tbWorkPlace, "Длина поля превышает 500 символов. Укажите только самое основное.");
                tabCard.SelectedIndex = MainClass.dbType == PriemType.Priem ? 4 : 3;
                return false;
            }
            else
                epError.Clear();

            if (!CheckIdent())
            {
                WinFormsServ.Error("В базе уже существует абитуриент с такими же либо ФИО, либо данными паспорта, либо данными аттестата!");
                return false;
            }

            if (MainClass.dbType == PriemType.Priem)
            {
                SortedList<string, string> slNumbers = new SortedList<string, string>();

                foreach (DataGridViewRow dr in dgvEGE.Rows)
                {
                    string num = dr.Cells["Номер сертификата"].Value.ToString();
                    string prNum = dr.Cells["Типографский номер"].Value.ToString();
                    string balls = dr.Cells["Баллы"].Value.ToString();

                    if (num.Length == 0 && balls.Length == 0)
                        continue;

                    int bls;
                    if (!(int.TryParse(balls, out bls) && bls > 0 && bls < 101))
                    {
                        epError.SetError(dgvEGE, "Неверно введены баллы");
                        tabCard.SelectedIndex = 3;
                        return false;
                    }
                    else
                        epError.Clear();

                    if (!IsMatchEgeNumber(num))
                    {
                        epError.SetError(dgvEGE, "Номер свидетельства не соответствует формату **-*********-**");
                        tabCard.SelectedIndex = 3;
                        return false;
                    }
                    else
                        epError.Clear();

                    if (slNumbers.Keys.Contains(num))
                    {
                        if (slNumbers[num].CompareTo(prNum) != 0)
                        {
                            epError.SetError(dgvEGE, "У свидетельств с одним номером разные типографские номера");
                            tabCard.SelectedIndex = 3;
                            return false;
                        }
                    }
                    else
                    {
                        epError.Clear();
                        slNumbers.Add(num, prNum);
                    }
                }
            }

            return true;
        }

        private bool CheckFieldsAbit()
        {
            using (PriemEntities context = new PriemEntities())
            { 
                if (LstCompetitions.Where(x => !x.HasCompetition).Count() > 0)
                {
                    epError.SetError(dgvApplications, "Не по всем конкурсным позициям указаны типы конкурсов");
                    tabCard.SelectedIndex = 8;
                    return false;
                }
                else
                    epError.Clear();

                return true;
               
            } 
        }

        protected override bool SaveClick()
        {
            try
            {
                if (_closePerson)
                {
                    if (!SaveApplication(personId.Value))
                        return false;
                }
                else
                {
                    if (!CheckFields() || !CheckFieldsAbit())
                        return false;

                    using (PriemEntities context = new PriemEntities())
                    {
                        using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.RequiresNew))
                        {
                            try
                            {
                                ObjectParameter entId = new ObjectParameter("id", typeof(Guid));
                                context.Person_SPO_insert(_personBarc, PersonName, SecondName, Surname, BirthDate, BirthPlace, PassportTypeId, PassportSeries, PassportNumber,
                                    PassportAuthor, PassportDate, SNILS, Sex, CountryId, NationalityId, RegionId, Phone, Mobiles, Email,
                                    KladrCode, Code, City, Street, House, Korpus, Flat, CodeReal, CityReal, StreetReal, HouseReal, KorpusReal, FlatReal, HostelAbit, HostelEduc, false,
                                    null, false, null, IsExcellent, LanguageId, SchoolCity, SchoolTypeId, SchoolExitClassId, SchoolName, SchoolNum, SchoolExitYear,
                                    SchoolAVG, CountryEducId, RegionEducId, IsEqual, EqualDocumentNumber, AttestatRegion, AttestatSeries, AttestatNum, DiplomSeries, DiplomNum, HighEducation, HEProfession,
                                    HEQualification, HEEntryYear, HEExitYear, HEStudyFormId, HEWork, Stag, WorkPlace, Privileges, PassportCode,
                                    PersonalCode, PersonInfo, ExtraInfo, null, StartEnglish, EnglishMark, EgeInSpbgu, entId);

                                personId = (Guid)entId.Value;
                                context.PersonSportQualification_insert(personId, SportQualificationId, SportQualificationLevel, OtherSportQualification);

                                SaveEgeFirst();

                                if (!SaveApplication(personId.Value))
                                    return false;

                                transaction.Complete();
                            }
                            catch (Exception exc)
                            {
                                WinFormsServ.Error("Ошибка при сохранении:\n" + exc.Message + (exc.InnerException != null ? "\n\nВнутреннее исключение:\n" + exc.InnerException.Message : ""));
                                return false;
                            }
                        }

                        _bdcInet.ExecuteQuery("UPDATE Person SET IsImported = 1 WHERE Person.Barcode = " + _personBarc);
                    }
                }

                _isModified = false;

                OnSave();

                this.Close();
                return true;
            }
            catch (Exception de)
            {
                WinFormsServ.Error("Ошибка обновления данных" + de.Message);
                return false;
            }
        }

        private bool SaveApplication(Guid PersonId)
        {
            if (_closeAbit)
                return true;

            if (personId == null)
                return false;

            if (!CheckFieldsAbit())
                return false;

            try
            {
                using (TransactionScope trans = new TransactionScope(TransactionScopeOption.Required))
                {
                    using (PriemEntities context = new PriemEntities())
                    {
                        ObjectParameter entId = new ObjectParameter("id", typeof(Guid));

                        foreach (var Comp in LstCompetitions)
                        {
                            var DocDate = Comp.DocDate;
                            var DocInsertDate = Comp.DocInsertDate;
                            bool isViewed = Comp.HasCompetition;
                            context.Abiturient_InsertDirectly(PersonId, Comp.EntryId, Comp.CompetitionId, Comp.IsListener,
                                false, false, false, null, DocDate, DocInsertDate,
                                false, false, null, Comp.OtherCompetitionId, Comp.CelCompetitionId, Comp.CelCompetitionText,
                                LanguageId, Comp.HasOriginals, Comp.Priority, Comp.Barcode, Comp.CommitId, _abitBarc, Comp.IsGosLine, isViewed, Comp.Id);

                            if (Comp.lstObrazProgramsInEntry.Count > 0)
                            {
                                //загружаем внутренние приоритеты по профилям
                                int currVersion = Comp.lstObrazProgramsInEntry.Select(x => x.CurrVersion).FirstOrDefault();
                                DateTime currDate = Comp.lstObrazProgramsInEntry.Select(x => x.CurrDate).FirstOrDefault();
                                Guid ApplicationVersionId = Guid.NewGuid();
                                context.ApplicationVersion.AddObject(new ApplicationVersion() { IntNumber = currVersion, Id = ApplicationVersionId, ApplicationId = Comp.Id, VersionDate = currDate });
                                foreach (var OPIE in Comp.lstObrazProgramsInEntry)
                                {
                                    if (OPIE.ListProfiles.Count == 0)
                                    {
                                        context.ApplicationDetails.AddObject(new ApplicationDetails()
                                        {
                                            ApplicationId = Comp.Id,
                                            Id = Guid.NewGuid(),
                                            ObrazProgramInEntryId = OPIE.Id,
                                            ObrazProgramInEntryPriority = OPIE.ObrazProgramInEntryPriority,
                                        });

                                        context.ApplicationVersionDetails.AddObject(new ApplicationVersionDetails()
                                        {
                                            ApplicationVersionId = ApplicationVersionId,
                                            ObrazProgramInEntryId = OPIE.Id,
                                            ObrazProgramInEntryPriority = OPIE.ObrazProgramInEntryPriority
                                        });
                                    }

                                    foreach (var ProfInOPIE in OPIE.ListProfiles)
                                    {
                                        context.ApplicationDetails.AddObject(new ApplicationDetails()
                                        {
                                            ApplicationId = Comp.Id,
                                            Id = Guid.NewGuid(),
                                            ObrazProgramInEntryId = OPIE.Id,
                                            ObrazProgramInEntryPriority = OPIE.ObrazProgramInEntryPriority,
                                            ProfileInObrazProgramInEntryId = ProfInOPIE.Id,
                                            ProfileInObrazProgramInEntryPriority = ProfInOPIE.ProfileInObrazProgramInEntryPriority
                                        });

                                        context.ApplicationVersionDetails.AddObject(new ApplicationVersionDetails()
                                        {
                                            ApplicationVersionId = ApplicationVersionId,
                                            ObrazProgramInEntryId = OPIE.Id,
                                            ObrazProgramInEntryPriority = OPIE.ObrazProgramInEntryPriority,
                                            ProfileInObrazProgramInEntryId = ProfInOPIE.Id,
                                            ProfileInObrazProgramInEntryPriority = ProfInOPIE.ProfileInObrazProgramInEntryPriority
                                        });
                                    }
                                }
                            }
                        }

                        /* context.Abiturient_Insert(personId, EntryId, CompetitionId, false, false, false, false, false, null, DocDate, DateTime.Now,
                         AttDocOrigin, EgeDocOrigin, false, false, null, OtherCompetitionId, CelCompetitionId, CelCompetitionText, LanguageId, false,
                         Priority, _abitBarc, entId);
                         */
                    }

                     // //_bdcInet.ExecuteQuery("UPDATE Application SET IsImported = 1 WHERE Application.Barcode = " + _abitBarc);
                     // //_bdcInet.ExecuteQuery("UPDATE Application SET IsImported = 1 WHERE Application.CommitId = (SELECT Id FROM ApplicationCommit WHERE IntNumber = '" + _abitBarc + "' )");
                   
                    _bdcInet.ExecuteQuery("UPDATE ApplicationCommit SET IsImported = 1 WHERE IntNumber = '" + _abitBarc + "'");
                    trans.Complete();
                    return true;
                }
            }
            catch (Exception de)
            {
                WinFormsServ.Error("Ошибка обновления данных Abiturient\n" + de.Message + (de.InnerException == null ? "" : "\n" + de.InnerException.Message));
                return false;
            }
        }
       
        private void SaveEgeFirst()
        {
            if (MainClass.dbType == PriemType.PriemMag)
                return;

            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    EgeList egeLst = new EgeList();

                    foreach (DataGridViewRow dr in dgvEGE.Rows)
                    {
                        if (dr.Cells["Баллы"].Value.ToString().Trim() != string.Empty)
                            egeLst.Add(new EgeMarkCert(dr.Cells["ExamId"].Value.ToString().Trim(), dr.Cells["Баллы"].Value.ToString().Trim(), dr.Cells["Номер сертификата"].Value.ToString().Trim(), dr.Cells["Типографский номер"].Value.ToString()));
                    }
                   
                    foreach (EgeCertificateClass cert in egeLst.EGEs.Keys)
                    {
                        // проверку на отсутствие одинаковых свидетельств
                        int res = (from ec in context.EgeCertificate
                                   where ec.Number == cert.Doc
                                   select ec).Count(); 
                        if (res > 0)
                        {
                            WinFormsServ.Error(string.Format("Свидетельство с номером {0} уже есть в базе, поэтому сохранено не будет!", cert.Doc));
                            continue;
                        }                        

                        ObjectParameter ecId = new ObjectParameter("id", typeof(Guid));
                        context.EgeCertificate_Insert(cert.Doc, cert.Tipograf, "20" + cert.Doc.Substring(cert.Doc.Length - 2, 2), personId, null, false, ecId);

                        Guid? certId = (Guid?)ecId.Value;
                        foreach (EgeMarkCert mark in egeLst.EGEs[cert])
                        {
                            int val;
                            if(!int.TryParse(mark.Value, out val))
                                continue;
                            
                            int subj;
                            if(!int.TryParse(mark.Subject, out subj))
                                continue;
                            
                            context.EgeMark_Insert((int?)val, (int?)subj, certId, false, false);
                        }
                    }
                }

            }
            catch (Exception de)
            {          
                WinFormsServ.Error("Ошибка сохранения данные ЕГЭ - данные не были сохранены. Введите их заново! \n" + de.Message);
            }
        }

        public bool IsMatchEgeNumber(string number)
        {
            string num = number.Trim();
            if (Regex.IsMatch(num, @"^\d{2}-\d{9}-(11|12|13)$"))//не даёт перегрузить воякам свои древние ЕГЭ
                return true;
            else
                return false;
        }

        #endregion 
        
        protected override void OnClosed()
        {
            base.OnClosed();
            load.CloseDB();                
        }
        protected override void OnSave()
        {
            base.OnSave();
            using (PriemEntities context = new PriemEntities())
            {
                if (_abitBarc != null)
                {
                    Guid? perId = (from ab in context.qAbiturient
                                   where ab.CommitNumber == _abitBarc
                                   select ab.PersonId).FirstOrDefault();

                    //MainClass.OpenCardAbit(abId.ToString(), null, null);
                    MainClass.OpenCardPerson(perId.ToString(), null, null);

                }
                else
                {
                    Guid? perId = (from per in context.extPersonSPO
                                   where per.Barcode == _personBarc
                                   select per.Id).FirstOrDefault();

                    MainClass.OpenCardPerson(perId.ToString(), null, null);
                }
            }
        }
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            List<KeyValuePair<string, string>> lstFiles = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, string> file in chlbFile.CheckedItems)
            {
                lstFiles.Add(file);
            }

            _docs.OpenFile(lstFiles);
        }
        private void tabCard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.D1)
                this.tcCard.SelectedIndex = 0;
            if (e.Control && e.KeyCode == Keys.D2)
                this.tcCard.SelectedIndex = 1;
            if (e.Control && e.KeyCode == Keys.D3)
                this.tcCard.SelectedIndex = 2;
            if (e.Control && e.KeyCode == Keys.D4)
                this.tcCard.SelectedIndex = 3;
            if (e.Control && e.KeyCode == Keys.D5)
                this.tcCard.SelectedIndex = 4;
            if (e.Control && e.KeyCode == Keys.D6)
                this.tcCard.SelectedIndex = 5;
            if (e.Control && e.KeyCode == Keys.D7)
                this.tcCard.SelectedIndex = 6;
            if (e.Control && e.KeyCode == Keys.D8)
                this.tcCard.SelectedIndex = 7;
            if (e.Control && e.KeyCode == Keys.S)
                SaveRecord();
        }

        private void dgvApplications_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rwNum = e.RowIndex;
            OpenCardCompetitionInInet(rwNum);
        }

        private void dgvApplications_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if ((bool)dgvApplications["HasCompetition", e.RowIndex].Value)
                {
                    e.CellStyle.BackColor = Color.Cyan;
                    e.CellStyle.SelectionBackColor = Color.Cyan;
                }
            }
        }
        private void OpenCardCompetitionInInet(int rwNum)
        {
            if (rwNum >= 0)
            {
                var ent = GetCompFromGrid(rwNum);
                if (ent != null)
                {
                    var crd = new CardCompetitionInInet(ent);
                    crd.OnUpdate += UpdateCommitCompetition;
                    crd.Show();
                }
            }
        }
       private ShortCompetition GetCompFromGrid(int rwNum)
        {
            if (rwNum < 0)
                return null;

            Guid Id = (Guid)dgvApplications["Id", rwNum].Value;
            return LstCompetitions.Where(x => x.Id == Id).FirstOrDefault();
        }

       private void btnOpenCompetition_Click(object sender, EventArgs e)
       {
           if (dgvApplications.SelectedCells.Count == 0)
               return;

           int rwNum = dgvApplications.SelectedCells[0].RowIndex;
           OpenCardCompetitionInInet(rwNum);
       }

       private void chbHostelEducYes_CheckedChanged(object sender, EventArgs e)
       {
           chbHostelEducNo.Checked = !chbHostelEducYes.Checked;  
       }

       private void chbHostelEducNo_CheckedChanged(object sender, EventArgs e)
       {
           chbHostelEducYes.Checked = !chbHostelEducNo.Checked;
       } 
    }
}
