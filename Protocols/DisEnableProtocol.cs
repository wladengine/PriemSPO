﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Priem
{
    public class DisEnableProtocol : ProtocolCard
    {
        public DisEnableProtocol(ProtocolList owner, int iFacultyId, int iStudyFormId, int iStudyBasisId)
            : this(owner, iFacultyId, iStudyBasisId, iStudyFormId, null)
        {
        }

        //конструктор 
        public DisEnableProtocol(ProtocolList owner, int iFacultyId, int iStudyFormId, int iStudyBasisId, Guid? id)
            : base(owner, iFacultyId, iStudyBasisId, iStudyFormId, id)
        {
            _type = ProtocolTypes.DisEnableProtocol;
            base.sQuery = this.sQuery;
        }

        //дополнительная инициализация
        protected override void InitControls()
        {
            sQuery = @"SELECT DISTINCT ed.extAbitSPO.Sum, ed.extPersonSPO.AttestatSeries, ed.extPersonSPO.AttestatNum, ed.extAbitSPO.Id as Id, ed.extAbitSPO.BAckDoc as backdoc, 
             (ed.extAbitSPO.BAckDoc | ed.extAbitSPO.NotEnabled ) as Red, ed.extAbitSPO.RegNum as Рег_Номер,
             ed.extPersonSPO.FIO as ФИО, 
             ed.extPersonSPO.EducDocument as Документ_об_образовании, 
             ed.extPersonSPO.PassportSeries + ' №' + ed.extPersonSPO.PassportNumber as Паспорт, 
             extAbitSPO.ObrazProgramNameEx + ' ' + (Case when extAbitSPO.ProfileId IS NULL then '' else extAbitSPO.ProfileName end) as Направление, 
             Competition.NAme as Конкурс, extAbitSPO.BackDoc 
             FROM ed.extAbitSPO INNER JOIN ed.extPersonSPO ON ed.extAbitSPO.PersonId = ed.extPersonSPO.Id
             LEFT JOIN ed.Competition ON ed.Competition.Id = ed.extAbitSPO.CompetitionId
             INNER JOIN ed.extProtocol ON ed.extProtocol.AbiturientId = ed.extAbitSPO.Id";

             // case when (NOT ed.hlpMinEgeAbiturient.Id IS NULL) then 'true' else 'false' end//
             //LEFT JOIN ed.hlpMinEgeAbiturient ON ed.hlpMinEgeAbiturient.Id = ed.extAbitSPO.Id      

            base.InitControls();

            this.Text = "Протокол об исключении из протокола о допуске";
            this.chbEnable.Text = "Добавить всех выбранных слева абитуриентов в протокол об исключении";

            this.chbFilter.Text = "Отфильтровать абитуриентов с проверенными данными";
            this.chbFilter.Visible = false;
        }

        protected override void InitAndFillGrids()
        {
            base.InitAndFillGrids();

            string sFilter = string.Empty;
            sFilter = string.Format(" AND ed.extProtocol.Excluded=0 AND ed.extProtocol.ISOld=0 AND extProtocol.ProtocolTypeId=1 AND extProtocol.FacultyId ={0} AND extProtocol.StudyFormId = {1} AND extProtocol.StudyBasisId = {2} ", 
                _facultyId.ToString(), _studyFormId.ToString(), _studyBasisId.ToString());

            if (!MainClass.RightsSov_SovMain())
                sFilter += " AND ed.extAbitSPO.CompetitionId NOT IN (1,2,7,8) ";
            FillGrid(dgvRight, sQuery, GetWhereClause("ed.extAbitSPO") + sFilter, sOrderby);

            //заполнили левый
            if (_id != null)
            {
                sFilter = string.Format(" WHERE ed.extAbitSPO.Id IN (SELECT AbiturientId FROM ed.extProtocol WHERE ProtocolId = '{0}')", _id);
                FillGrid(dgvLeft, sQuery, sFilter, sOrderby);
            }
            else //новый
            {
                InitGrid(dgvLeft);
            }
        }

        //подготовка нужного грида
        protected override void InitGrid(DataGridView dgv)
        {
            base.InitGrid(dgv);
        }

        /*
         *Переопределяем, чтобы можно было красных таскать
         */
        //функция заполнения грида
        protected override void FillGrid(DataGridView dgv, string sQuery, string sWhere, string sOrderby)
        {
            try
            {
                //подготовили грид
                InitGrid(dgv);
                DataSet ds = MainClass.Bdc.GetDataSet(sQuery + sWhere + sOrderby);

                //заполняем строки
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    DataGridViewRow r = new DataGridViewRow();
                    r.CreateCells(dgv, false, dr["Id"].ToString(), false, false, dr["backdoc"].ToString(), dr["Рег_номер"].ToString(), dr["ФИО"].ToString(), dr["Sum"].ToString(), dr["Документ_об_образовании"].ToString(), dr["Направление"].ToString(), dr["Конкурс"].ToString(), dr["Паспорт"].ToString());
                    if (bool.Parse(dr["Red"].ToString()))
                    {
                        r.Cells[5].Style.ForeColor = Color.Red;
                        r.Cells[6].Style.ForeColor = Color.Red;
                    }

                    dgv.Rows.Add(r);
                }

                //блокируем редактирование
                for (int i = 1; i < dgv.ColumnCount; i++)
                    dgv.Columns[i].ReadOnly = true;

                dgv.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при заполнении грида данными протокола: " + ex.Message);
            }
        }
    }
}
