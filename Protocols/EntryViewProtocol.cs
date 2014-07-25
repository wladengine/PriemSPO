﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Transactions;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.Objects;

using BDClassLib;
using EducServLib;
using WordOut;

namespace Priem
{
    public class EntryViewProtocol : ProtocolCard
    {
        Dictionary<int?, List<string>> lstSelected;
  
        public EntryViewProtocol(ProtocolList owner, int sFac, int sSection, int sForm, int? sProf, bool? isSec, bool? isReduced, bool? isParal, bool? isList, bool isCel)
            : this(owner, sFac, sSection, sForm, sProf, isSec, isReduced, isParal, isList, isCel, null)
        {
        }

        //конструктор 
        public EntryViewProtocol(ProtocolList owner, int sFac, int sSection, int sForm, int? sProf, bool? isSec, bool? isReduced, bool? isParal, bool? isList, bool isCel, Guid? sProtocol)
            : base(owner,sFac,sSection,sForm,sProf, isSec, isReduced, isParal, isList, isCel, sProtocol)
        {
            _type = ProtocolTypes.EntryView;                      
        }

        //дополнительная инициализация
        protected override void  InitControls()
        {
            using (PriemEntities context = new PriemEntities())
            {
               string ehQuery = string.Empty; 
                
                if (_isCel.Value)
                   ehQuery = "SELECT CONVERT(varchar(100), Id) AS Id, Acronym as Name FROM ed.EntryHeader WHERE Id IN (7) ORDER BY Id";
                else
                {
                    if(MainClass.dbType == PriemType.PriemMag)
                        ehQuery = "SELECT CONVERT(varchar(100), Id) AS Id, Acronym as Name FROM ed.EntryHeader WHERE Id IN (8,10,9) ORDER BY Id";                        
                    else
                        ehQuery = "SELECT CONVERT(varchar(100), Id) AS Id, Acronym as Name FROM ed.EntryHeader WHERE Id IN (8, 9, 10) ORDER BY Id";
                }

                ComboServ.FillCombo(cbHeaders, HelpClass.GetComboListByQuery(ehQuery), false, false);
                
                cbHeaders.Visible = true;
                cbHeaders.SelectedIndexChanged += new EventHandler(cbHeaders_SelectedIndexChanged);

                lstSelected = new Dictionary<int?, List<string>>();
                foreach(KeyValuePair<string,string> val in cbHeaders.Items)
                { 
                    lstSelected.Add(int.Parse(val.Key), new List<string>());
                }
                
                base.InitControls();

                this.Text = "Представление о зачислении";
                this.chbEnable.Text = "Добавить всех выбранных слева абитуриентов в представление о зачислении";

                this.chbFilter.Visible = false;
            }
        }

        void cbHeaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateGrids();
        }

        public int? HeaderId
        {
            get { return ComboServ.GetComboIdInt(cbHeaders); }
            set { ComboServ.SetComboId(cbHeaders, value); }
        }        

        string GetTotalFilter()
        {
            return GetTotalFilter(true);
        }

        string GetTotalFilter(bool header)
        {
            string sFilter = string.Empty;

            //обработали Направление 
            if (_licenseProgramId != null)
                sFilter += " AND ed.qAbiturient.LicenseProgramId = " + _licenseProgramId;
                        
            if (_isSecond.HasValue)           
                sFilter += " AND ed.qAbiturient.IsSecond = " + QueryServ.StringParseFromBool(_isSecond.Value);
            if (_isReduced.HasValue)
                sFilter += " AND ed.qAbiturient.IsReduced = " + QueryServ.StringParseFromBool(_isReduced.Value);
            if (_isParallel.HasValue)
                sFilter += " AND ed.qAbiturient.IsParallel = " + QueryServ.StringParseFromBool(_isParallel.Value);

            //обработали слушатель           
            if (_isListener.HasValue)
                sFilter += " AND ed.qAbiturient.IsListener = " + QueryServ.StringParseFromBool(_isListener.Value);

            sFilter += " AND ed.qAbiturient.BackDoc = 0 ";

            sFilter += " AND (ed.qAbiturient.Id NOT IN (SELECT AbiturientId FROM ed.extEntryView WHERE IsListener = 0 AND StudyLevelGroupId = 3) OR ed.qAbiturient.IsListener = 1)";

            sFilter += "AND ((ed.qAbiturient.IsListener = 0 AND ed.qAbiturient.IsSecond = 0 AND ed.qAbiturient.IsReduced = 0 AND ed.qAbiturient.IsParallel = 0 AND ed.qAbiturient.HasOriginals > 0) OR ed.qAbiturient.IsListener = 1 OR ed.qAbiturient.IsSecond = 1 OR ed.qAbiturient.IsReduced = 1 OR ed.qAbiturient.IsParallel = 1)";
      
            if (_studyBasisId == 2)
            {
                sFilter += " AND ed.qAbiturient.IsPaid>0 ";
                sFilter += " AND EXISTS (SELECT Top(1) ed.PaidData.Id FROM ed.PaidData WHERE ed.PaidData.AbiturientId = ed.qAbiturient.Id) ";
            }

            if (header)
            {
                switch (HeaderId)
                {
                    case 1:
                        sFilter += " AND ed.qAbiturient.CompetitionId=1 ";
                        sFilter += " AND PersonId IN (SELECT PersonId FROM ed.Olympiads WHERE OlympTypeId=1) ";
                        break;
                    case 2:
                        sFilter += " AND ed.qAbiturient.CompetitionId=1 ";
                        sFilter += " AND PersonId IN (SELECT PersonId FROM ed.Olympiads WHERE OlympValueId=6 AND OlympTypeId=2) ";
                        break;
                    case 3:
                        //sFilter += " AND ed.qAbiturient.CompetitionId=1 ";
                        sFilter += " AND PersonId IN (SELECT PersonId FROM ed.Olympiads WHERE OlympValueId=5 AND OlympTypeId=2) ";
                        break;
                    case 4:
                        //sFilter += " AND ed.qAbiturient.CompetitionId=1 ";
                        sFilter += " AND PersonId IN (SELECT PersonId FROM ed.Olympiads WHERE OlympValueId=6 AND OlympTypeId IN (3,4)) ";
                        break;
                    case 5:
                        //sFilter += " AND ed.qAbiturient.CompetitionId=1 ";
                        sFilter += " AND PersonId IN (SELECT PersonId FROM ed.Olympiads WHERE OlympValueId=5 AND OlympTypeId IN (3,4)) ";
                        break;
                    case 6:
                        sFilter += " AND ed.qAbiturient.CompetitionId=2 ";
                        break;
                    case 7:
                        sFilter += " AND ed.qAbiturient.CompetitionId = 6";
                        break;
                    case 8:
                        sFilter += " AND ed.qAbiturient.CompetitionId NOT IN (1,2,6,7,8) ";
                        break;
                    case 9:
                        sFilter += " AND ed.qAbiturient.CompetitionId IN (1,8) ";
                        break;
                    case 10:
                        sFilter += " AND ed.qAbiturient.CompetitionId IN (2,7) ";
                        break;
                }
            }
            return GetWhereClause("ed.qAbiturient") + sFilter;
        }
                
        //int GetTotalCount()
        //{            
        //    string query = string.Format("SELECT DISTINCT Count(qAbiturient.Id) " +
        //    " FROM qAbiturient INNER JOIN PErson ON ed.qAbiturientPErsonId = Person.Id " +
        //    " INNER JOIN extEnableProtocol ON ed.qAbiturientId=extEnableProtocol.AbiturientId " +
        //    " INNER JOIN _FirstWaveGreenBackup ON ed.qAbiturientId=_FirstWaveGreenBackup.AbiturientId " +
        //    " INNER JOIN extAbitMarksSum ON ed.qAbiturientId=extAbitMarksSum.Id " +
        //    " LEFT JOIN Specialization ON Specialization.Id = ed.qAbiturientSpecializationId LEFT JOIN StudyBasis ON StudyBasis.Id = ed.qAbiturientStudyBasisId " +
        //    " LEFT JOIN StudyForm ON StudyForm.Id = ed.qAbiturientStudyFormId LEFT JOIN Profession ON Profession.Id = ed.qAbiturientProfessionId " +
        //    " LEFT JOIN Competition ON Competition.Id = ed.qAbiturientCompetitionId ", MainClass.GetStringAbitNumber("qAbiturient"));

        //    return (int)MainClass.Bdc.GetValue(query + GetTotalFilter(false));
        //}

        protected override void InitAndFillGrids()
        {
            base.InitAndFillGrids();            
            /*
            FillGrid(dgvRight, sQuery, GetTotalFilter() , sOrderby);
                        
            //заполнили левый
            if (_id!=null)
            {
                string sFilter = string.Format(" WHERE ed.qAbiturientId IN (SELECT AbiturientId FROM qProtocolHistory WHERE ProtocolId = '{0}')", _id);
                FillGrid(dgvLeft, sQuery, sFilter, sOrderby);
            }
            else //новый
            {
                InitGrid(dgvLeft);
            }
            
            //блокируем редактирование кроме первого столбца
            for (int i = 1; i < dgvLeft.ColumnCount; i++)
                dgvLeft.Columns[i].ReadOnly = true;

            dgvLeft.Update();    */
            UpdateLeft();
            UpdateRight();
        }

        //подготовка нужного грида
        protected override void InitGrid(DataGridView dgv)
        {
            base.InitGrid(dgv);

            dgv.Columns["Pasport"].Visible = false;
            dgv.Columns["Attestat"].Visible = false;

        }

        string GetSelectedIdList()
        {
            List<string> lstRez = new List<string>();

            foreach (List<string> lst in lstSelected.Values)
            {
                string temp = Util.BuildStringWithCollection(lst);
                if(temp.Length>0)
                    lstRez.Add(temp);
            }

            return Util.BuildStringWithCollection(lstRez);
        }

        void UpdateGrids()
        {
            UpdateLeft();
            UpdateRight();
        }

        void UpdateRight()
        {  
            string ids = GetSelectedIdList();

            string filt = string.IsNullOrEmpty(ids) ? "" : string.Format(" AND ed.qAbiturient.Id NOT IN ({0}) ", ids);
            dgvRight.Rows.Clear();

            
            DataTable dtAbits = new DataTable();
                
            DataSet dsPrograms = MainClass.Bdc.GetDataSet(string.Format(@"SELECT DISTINCT ObrazProgramId, ProfileId, KCP AS Value, KCPCel AS ValueCel
                    FROM ed.qEntry 
                    WHERE ed.qEntry.StudyLevelGroupId = {7} AND ed.qEntry.FacultyId={0} AND ed.qEntry.StudyFormId={1} AND
                    ed.qEntry.StudyBasisId={2} AND ed.qEntry.LicenseProgramId={3} AND ed.qEntry.IsSEcond = {4} AND ed.qEntry.IsReduced = {5} AND ed.qEntry.IsParallel = {6}", _facultyId, _studyFormId, _studyBasisId, _licenseProgramId, 
                    QueryServ.StringParseFromBool(_isSecond.Value), QueryServ.StringParseFromBool(_isReduced.Value), QueryServ.StringParseFromBool(_isParallel.Value), MainClass.studyLevelGroupId));

            foreach (DataRow dr in dsPrograms.Tables[0].Rows)
            {                    
                string obProg = dr["ObrazProgramId"].ToString();
                string spec = dr["ProfileId"].ToString();

                string enteredQuery = string.Format(@"SELECT Count(ed.extAbit.Id) FROM ed.extAbit 
                        INNER JOIN ed.extEntryView ON ed.extAbit.Id=ed.extEntryView.AbiturientId 
                        WHERE Excluded=0 AND ed.extAbit.StudyLevelGroupId = {9}
                        AND ed.extAbit.FacultyId={0} AND ed.extAbit.StudyFormId={1} AND ed.extAbit.StudyBasisId={2} 
                        AND ed.extAbit.LicenseProgramId={3} AND ed.extAbit.IsSEcond = {4} AND ed.extAbit.IsReduced = {5} AND ed.extAbit.IsParallel = {6} AND ed.extAbit.ObrazProgramId={7} {8}", _facultyId, _studyFormId, _studyBasisId, _licenseProgramId, QueryServ.StringParseFromBool(_isSecond.Value), QueryServ.StringParseFromBool(_isReduced.Value), QueryServ.StringParseFromBool(_isParallel.Value),
                        obProg, string.IsNullOrEmpty(spec) ? " AND ed.extAbit.ProfileId IS NULL " : " AND ed.extAbit.ProfileId='" + spec + "'", MainClass.studyLevelGroupId);

                if (_isCel.Value)
                    enteredQuery += " AND ed.extAbit.CompetitionId=6 ";
                  
                int entered = 0;
                int kc = 0;
                    
                int.TryParse(MainClass.Bdc.GetStringValue(enteredQuery), out entered);
                if(_isCel.Value)
                    int.TryParse(dr["ValueCel"].ToString(), out kc);
                else
                    int.TryParse(dr["Value"].ToString(), out kc);

                int kcRest = kc - entered;
                               
                string sQueryBody = string.Format("SELECT DISTINCT TOP ({0}) ed.extAbitMarksSum.TotalSum as Sum, ed.extPersonSPO.AttestatSeries, ed.extPersonSPO.AttestatNum, ed.qAbiturient.Id as Id, ed.qAbiturient.BAckDoc as backdoc, " +
                    " 'false' as Red, ed.qAbiturient.RegNum as Рег_Номер, " +
                    " ed.extPersonSPO.FIO as ФИО, " +
                    " (case when ed.extPersonSPO.SchoolTypeId = 1 then ed.extPersonSPO.AttestatRegion + ' ' + ed.extPersonSPO.AttestatSeries + '  №' + ed.extPersonSPO.AttestatNum else ed.extPersonSPO.DiplomSeries + '  №' + ed.extPersonSPO.DiplomNum end) as Документ_об_образовании, " +
                    " ed.extPersonSPO.PassportSeries + ' №' + ed.extPersonSPO.PassportNumber as Паспорт, " +
                    " LicenseProgramCode + ' ' + LicenseProgramName + ' ' +(Case when NOT ed.qAbiturient.ProfileId IS NULL then ProfileName else ObrazProgramName end) as Направление, " +
                    " Competition.NAme as Конкурс, ed.qAbiturient.BackDoc, ed._FirstWave.SortNum " +
                    " FROM ed.qAbiturient INNER JOIN ed.extPersonSPO ON ed.qAbiturient.PErsonId =  ed.extPersonSPO.Id " +
                    " INNER JOIN ed.extEnableProtocol ON ed.qAbiturient.Id=ed.extEnableProtocol.AbiturientId " +
                    " INNER JOIN ed._FirstWave ON ed.qAbiturient.Id=ed._FirstWave.AbiturientId " +
                    " LEFT JOIN ed.extAbitMarksSum ON ed.qAbiturient.Id=ed.extAbitMarksSum.Id " +
                    " LEFT JOIN ed.Competition ON ed.Competition.Id = ed.qAbiturient.CompetitionId ", kcRest);

                string sQueryJoinFW = string.Empty;               
                                
                string sFilter = GetTotalFilter() + filt;
                sFilter += " AND ed.qAbiturient.ObrazProgramId = " + obProg;
                sFilter += string.IsNullOrEmpty(spec) ? " AND ed.qAbiturient.ProfileId IS NULL " : " AND ed.qAbiturient.ProfileId='" + spec + "'";

                string orderBy = " ORDER BY SortNum";

                DataTable dtProg = MainClass.Bdc.GetDataSet(sQueryBody + sQueryJoinFW + sFilter + orderBy).Tables[0];

                dtAbits.Merge(dtProg);

                //// на вторую волну сделать так же и для бакалавров
                //if (MainClass.dbType == PriemType.PriemMag)
                //{
                //    int cntRestAfterGreen = kcRest - dtProg.Rows.Count;
                //    if (cntRestAfterGreen > 0)
                //    {
                //        kcRest = cntRestAfterGreen;                    
                //        DataTable dtProgAdd = MainClass.Bdc.GetDataSet(sQueryBody + sFilter + orderBy).Tables[0];
                //        dtAbits.Merge(dtProgAdd);
                //    }
                //}
            }

            FillGrid(dgvRight, dtAbits);            
        }

        void UpdateLeft()
        { 
            string ids = Util.BuildStringWithCollection(lstSelected[HeaderId]);


            dgvLeft.Rows.Clear();
            if (ids.Length > 0)
                FillGrid(dgvLeft, sQuery, GetTotalFilter() + string.Format(" AND ed.qAbiturient.Id IN ({0}) ", ids), sOrderby);
            else
                InitGrid(dgvLeft);
        }

        protected override void OnMoved()
        {
            base.OnMoved();

            List<string> curList = lstSelected[HeaderId];
            curList.Clear();
            foreach (DataGridViewRow row in dgvLeft.Rows)
                curList.Add("'" + row.Cells["Id"].Value.ToString() + "'");
        }

        //сохранение
        protected override bool Save()
        {
            //проверка данных
            if (!CheckData())
                return false;            
            /*
            int total = GetTotalCount();

            int selected = 0;

            foreach (List<string> lst in lstSelected)
                selected += lst.Count;

            if (selected != total)
            {
                MessageBox.Show("Выберите формулировку для всех абитуриентов, имеющих право на зачисления, из зеленый зоны волны!", "Внимание");
                return false;
            }
            */
            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        Guid protocolId;

                        ObjectParameter paramId = new ObjectParameter("id", typeof(Guid));
                        int iProtocolTypeId = ProtocolList.TypeToInt(_type);

                        context.Protocol_InsertAll(MainClass.studyLevelGroupId,
                                  _facultyId, _licenseProgramId, _studyFormId, _studyBasisId, tbNum.Text, dtpDate.Value, iProtocolTypeId,
                                  string.Empty, !isNew, null, _isSecond, _isReduced, _isParallel, _isListener, paramId);

                        protocolId = (Guid)paramId.Value;                        

                        foreach (int? key in lstSelected.Keys)
                        //for (int i=0; i<lstSelected.Count; i++)
                        {
                            List<string> lst = lstSelected[key];
                            foreach (string str in lst)
                            {                              
                                Guid abId = new Guid(str.Trim(new char[] { '\'' }));
                                context.ProtocolHistory_Insert(abId, protocolId, false, null, key, paramId);                                
                            }
                        }
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при сохранении протокола: " + ex.Message);
            }

            return true;
        }

        //предварительный просмотр
        protected override void Preview()
        {            
            try
            {
                WordDoc wd = new WordDoc(string.Format(@"{0}\EmptyTemplate.dot", MainClass.dirTemplates));

                int counter = 0;

                int lstCount = -1;
                int lstTableCount = 0;
                //foreach (List<string> lst in lstSelected.Values)
                foreach (int? key in lstSelected.Keys)
                {
                    List<string> lst = lstSelected[key];

                    lstCount++;
                    if (lst.Count == 0)
                        continue;

                    string header = MainClass.Bdc.GetStringValue("SELECT Name FROM ed.EntryHeader WHERE id=" + key);
                    wd.AddParagraph(string.Format("\r\n {0}",header));
                    wd.AddNewTable(lst.Count+1,6);
                    TableDoc td = wd.Tables[lstTableCount];
                    ++lstTableCount;
                    //заполняем таблицу в шаблоне

                    int r = 0;
                    
                    td[0, r] = "№ п/п";
                    td[1, r] = "Рег.номер";
                    td[2, r] = "ФАМИЛИЯ, ИМЯ, ОТЧЕСТВО";
                    td[3, r] = "Сумма баллов";
                    td[4, r] = "Направление или Направление (профиль или Профиль)";
                    td[5, r] = "Вид конкурса";

                    DataSet ds = MainClass.Bdc.GetDataSet(sQuery + string.Format(" WHERE ed.qAbiturientId IN ({0})", Util.BuildStringWithCollection(lst)) + sOrderby);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        ++r;
                        ++counter;
                        td[0, r] = counter.ToString();
                        td[1, r] = row["Рег_Номер"].ToString();
                        td[2, r] = row["ФИО"].ToString();
                        td[3, r] = row["Sum"].ToString();
                        td[4, r] = row["Направление"].ToString();
                        td[5, r] = row["Конкурс"].ToString();                                                
                    }
                }
            }
            catch (Exception ex)
            {
                WinFormsServ.Error("Ошибка при выводе в Word протокола о допуске: " + ex.Message);
            }
        }
   }
}