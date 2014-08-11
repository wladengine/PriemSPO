using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Priem
{
    public class EnableProtocol : ProtocolCard
    {
        //конструктор         
        public EnableProtocol(ProtocolList owner, int iFacultyId, int iStudyBasisId, int iStudyFormId)
            : base(owner, iFacultyId, iStudyBasisId, iStudyFormId)
        {
            _type = ProtocolTypes.EnableProtocol;
        }

        public EnableProtocol() : base()
        { 
            //InitializeComponent(); 
        }

        //дополнительная инициализация
        protected override void InitControls()
        {
            sQuery = @"SELECT DISTINCT ed.extAbitSPO.Sum, 
            --ed.extPersonSPO.AttestatSeries, 
            --ed.extPersonSPO.AttestatNum, 
            ed.extPerson.AttestatSeries, 
            ed.extPerson.AttestatNum, 

            ed.extAbitSPO.Id as Id, 
            ed.extAbitSPO.BAckDoc as backdoc, 
             (ed.extAbitSPO.BAckDoc | ed.extAbitSPO.NotEnabled | case when (NOT ed.hlpMinEgeAbiturient.Id IS NULL) then 'true' else 'false' end) as Red, ed.extAbitSPO.RegNum as Рег_Номер, 
             --ed.extPersonSPO.FIO as ФИО, 
             --ed.extPersonSPO.EducDocument as Документ_об_образовании, 
             --ed.extPersonSPO.PassportSeries + ' №' + ed.extPersonSPO.PassportNumber as Паспорт, 

              ed.extPerson.FIO as ФИО, 
             ed.extPerson.EducDocument as Документ_об_образовании, 
             ed.extPerson.PassportSeries + ' №' + ed.extPerson.PassportNumber as Паспорт, 

             extAbitSPO.ObrazProgramNameEx + ' ' + (Case when extAbitSPO.ProfileId IS NULL then '' else extAbitSPO.ProfileName end) as Направление, 
             Competition.NAme as Конкурс, extAbitSPO.BackDoc 
             FROM ed.extAbitSPO 
             --left JOIN ed.extPersonSPO ON ed.extAbitSPO.PersonId = ed.extPersonSPO.Id  
             inner JOIN ed.extPerson ON ed.extAbitSPO.PersonId = ed.extPerson.Id    
             LEFT JOIN ed.hlpMinEgeAbiturient ON ed.hlpMinEgeAbiturient.Id = ed.extAbitSPO.Id          
             LEFT JOIN ed.Competition ON ed.Competition.Id = ed.extAbitSPO.CompetitionId
                ";

            base.InitControls();

            this.Text = "Протокол о допуске";
            this.chbEnable.Text = "Добавить всех выбранных слева абитуриентов в протокол о допуске";

            this.chbFilter.Text = "Отфильтровать абитуриентов с проверенными данными";
            this.chbFilter.Visible = true;
        }

        protected override void InitAndFillGrids()
        {
            base.InitAndFillGrids();

            string sFilter = string.Empty;
            sFilter = string.Format(" AND ed.extAbitSPO.BackDoc = 0 AND ed.extAbitSPO.NotEnabled=0 AND ed.extAbitSPO.Id NOT IN (SELECT AbiturientId FROM ed.qProtocolHistory WHERE Excluded=0 AND ProtocolId IN (SELECT Id FROM ed.qProtocol WHERE ISOld=0 AND ProtocolTypeId=1 AND FacultyId ={0} AND StudyFormId = {1} AND StudyBasisId = {2}))", 
                _facultyId.ToString(), _studyFormId.ToString(), _studyBasisId.ToString(), MainClass.studyLevelGroupId);

            if (chbFilter.Checked)
                sFilter += " AND ed.extAbitSPO.Checked > 0";

            //сперва общий конкурс (не общ-преим), т.к. чернобыльцы негодуют - льготы есть, а в протокол не попасть
           // FillGrid(dgvRight, sQuery, GetWhereClause("ed.extAbitSPO") + sFilter + " AND ed.extAbitSPO.CompetitionId NOT IN (1,2,7,8)/* AND (ed.extPersonSPO.Privileges=0  OR ed.extAbitSPO.CompetitionId IN (5,6))*/", sOrderby);
            FillGrid(dgvRight, sQuery, GetWhereClause("ed.extAbitSPO") + sFilter + " AND ed.extAbitSPO.CompetitionId NOT IN (1,2,7,8)/* AND (ed.extPerson.Privileges=0  OR ed.extAbitSPO.CompetitionId IN (5,6))*/", sOrderby);

            //заполнили левый
            if (_id != null)
            {
                sFilter = string.Format(" WHERE ed.extAbitSPO.Id IN (SELECT AbiturientId FROM ed.qProtocolHistory WHERE ProtocolId = '{0}')", _id.ToString());
                FillGrid(dgvLeft, sQuery, sFilter, sOrderby);
            }
            else //новый
            {
                InitGrid(dgvLeft);
            }

            // заполнение льготников, проверенных советниками
            //string query = sQuery + GetWhereClause("ed.extAbitSPO") + sFilter + " AND (ed.extAbitSPO.CompetitionId IN (1,8) OR (ed.extPersonSPO.Privileges>0 AND ed.extAbitSPO.CompetitionId IN (2,7))) AND ed.extAbitSPO.Checked>0 ";
            string query = sQuery + GetWhereClause("ed.extAbitSPO") + sFilter + " AND (ed.extAbitSPO.CompetitionId IN (1,8) OR (ed.extPerson.Privileges>0 AND ed.extAbitSPO.CompetitionId IN (2,7))) AND ed.extAbitSPO.Checked>0 ";

            DataSet ds = MainClass.Bdc.GetDataSet(query);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                DataGridViewRow r = new DataGridViewRow();
                r.CreateCells(dgvLeft, false, dr["Id"].ToString(), dr["Red"].ToString(), true, dr["backdoc"].ToString(), dr["Рег_номер"].ToString(), dr["ФИО"].ToString(), dr["Sum"].ToString(), dr["Документ_об_образовании"].ToString(), dr["Направление"].ToString(), dr["Конкурс"].ToString(), dr["Паспорт"].ToString());
                if (bool.Parse(dr["Red"].ToString()))
                {
                    r.Cells[5].Style.ForeColor = Color.Red;
                    r.Cells[6].Style.ForeColor = Color.Red;
                }

                r.Cells[5].Style.BackColor = Color.Green;
                r.Cells[6].Style.BackColor = Color.Green;

                dgvLeft.Rows.Add(r);
            }

            //блокируем редактирование кроме первого столбца
            for (int i = 1; i < dgvLeft.ColumnCount; i++)
                dgvLeft.Columns[i].ReadOnly = true;

            dgvLeft.Update();
        }

        //подготовка нужного грида
        protected override void InitGrid(DataGridView dgv)
        {
            base.InitGrid(dgv);
        }
    }
}
