﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using EducServLib;
using RtfWriter;

namespace Priem
{
    public partial class RegionAbitStatstics_Reversed : Form
    {
        public int RegionId
        {
            get
            {
                return ComboServ.GetComboIdInt(cbRegion).Value;
            }
        }
        public int? StudyFormId
        {
            get
            {
                if (cbStudyForm.Text == ComboServ.DISPLAY_ALL_VALUE)
                    return null;
                return ComboServ.GetComboIdInt(cbStudyForm);
            }
        }

        public RegionAbitStatstics_Reversed()
        {
            InitializeComponent();
            this.MdiParent = MainClass.mainform;
            FillComboRegion();
        }

        private void FillComboRegion()
        {
            using (PriemEntities context = new PriemEntities())
            {
                var src = (from x in context.qAbitAll
                           join p in context.Person
                           on x.PersonId equals p.Id
                           where x.StudyLevelGroupId == MainClass.studyLevelGroupId
                           && p.Person_Contacts.RegionId != null
                           
                           select new
                           {
                               p.Person_Contacts.Region.Id,
                               p.Person_Contacts.Region.Name,
                               p.Person_Contacts.Region.Distance
                           }).Distinct().ToList().OrderBy(x => x.Distance)
                           .Select(x => new KeyValuePair<string, string>(x.Id.ToString(), x.Name)).Distinct().ToList();

                ComboServ.FillCombo(cbRegion, src, false, false);
            }
        }
        private void FillComboStudyForm()
        {
            using (PriemEntities context = new PriemEntities())
            {
                var src = (from x in context.qAbitAll
                           join p in context.Person
                           on x.PersonId equals p.Id
                           where x.StudyLevelGroupId == MainClass.studyLevelGroupId
                           && p.Person_Contacts.RegionId == RegionId
                           select new
                           {
                               x.StudyFormId,
                               x.StudyFormName
                           });
                
                var bind = src.Distinct().ToList().
                    Select(x => new KeyValuePair<string, string>(x.StudyFormId.ToString(), x.StudyFormName)).Distinct().ToList();

                ComboServ.FillCombo(cbStudyForm, bind, false, true);
            }
        }

        private void cbRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillComboStudyForm();
        }
        private void cbStudyForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillGrid();
        }
        private void chbEntered_CheckedChanged(object sender, EventArgs e)
        {
            FillGrid();
        }

        private void FillGrid()
        {
            string query = @"
                SELECT extAbit.FacultyName, extAbit.StudyBasisId, COUNT(extAbit.Id) AS CNT_Abit, COUNT(DISTINCT extAbit.PersonId) AS CNT_Person
                FROM ed.extAbit
                INNER JOIN ed.extPerson ON extPerson.Id = extAbit.PersonId
                LEFT JOIN ed.extEntryView ON extEntryView.AbiturientId = extAbit.Id
                WHERE extAbit.StudyLevelGroupId=@StudyLevelGroupId AND extPerson.RegionId=@RegionId";
            string groupby = " GROUP BY extAbit.FacultyName, extAbit.StudyBasisId ";

            SortedList<string, object> sl = new SortedList<string, object>();
            sl.Add("@StudyLevelGroupId", MainClass.studyLevelGroupId);
            sl.Add("@RegionId", RegionId);

            if (StudyFormId.HasValue)
            {
                query += " AND extAbit.StudyFormId=@StudyFormId";
                sl.Add("@StudyFormId", StudyFormId);
            }
            if (chbEntered.Checked)
                query += " AND extEntryView.AbiturientId IS NOT NULL";

            DataTable tbl = MainClass.Bdc.GetDataSet(query + groupby, sl).Tables[0];
            var data_counts = (from DataRow rw in tbl.Rows
                               select new
                               {
                                   FacultyName = rw.Field<string>("FacultyName"),
                                   StudyBasisId = rw.Field<int>("StudyBasisId"),
                                   CNT_abit = rw.Field<int>("CNT_Abit"),
                                   CNT_Person = rw.Field<int>("CNT_Person"),
                               }).ToList();

            query = @"
SELECT extAbit.FacultyName, extAbit.StudyBasisId, MIN(hlpStatMarksSum.SUM) AS MinSUM, AVG(convert(float,hlpStatMarksSum.SUM)) AS AvgSUM, MAX(hlpStatMarksSum.SUM) AS MaxSUM
FROM ed.extPerson
INNER JOIN ed.extAbit ON extAbit.PersonId = extPerson.Id
LEFT JOIN ed.extEntryView ON extEntryView.AbiturientId = extAbit.Id
LEFT JOIN ed.hlpStatMarksSum ON hlpStatMarksSum.AbiturientId = extAbit.Id
WHERE extPerson.RegionId=@RegionId AND extAbit.StudyLevelGroupId=@StudyLevelGroupId";
            sl.Clear();
            sl.Add("@StudyLevelGroupId", MainClass.studyLevelGroupId);
            sl.Add("@RegionId", RegionId);

            if (StudyFormId.HasValue)
            {
                query += " AND extAbit.StudyFormId=@StudyFormId";
                sl.Add("@StudyFormId", StudyFormId);
            }
            if (chbEntered.Checked)
                query += " AND extEntryView.AbiturientId IS NOT NULL";

            groupby = " GROUP BY extAbit.FacultyName, extAbit.StudyBasisId ";

            tbl = MainClass.Bdc.GetDataSet(query + groupby, sl).Tables[0];

            var data_marks = (from DataRow rw in tbl.Rows
                             select new
                             {
                                 FacultyName = rw.Field<string>("FacultyName"),
                                 StudyBasisId = rw.Field<int>("StudyBasisId"),
                                 MinSUM = rw.Field<int?>("MinSUM"),
                                 AvgSUM = rw.Field<double?>("AvgSUM") ?? 0d,
                                 MaxSUM = rw.Field<int?>("MaxSUM")
                             }).ToList();

            DataTable src_tbl = new DataTable();

            src_tbl.Columns.Add("Факультет");
            src_tbl.Columns.Add("Подано заявл. г/б", typeof(int));
            src_tbl.Columns.Add("Подано заявл. дог", typeof(int));
            src_tbl.Columns.Add("Абитуриентов г/б", typeof(int));
            src_tbl.Columns.Add("Абитуриентов дог", typeof(int));
            src_tbl.Columns.Add("% заявл. г/б", typeof(float));
            src_tbl.Columns.Add("% заявл. дог", typeof(float));
            src_tbl.Columns.Add("% абит. г/б", typeof(float));
            src_tbl.Columns.Add("% абит. дог", typeof(float));

            src_tbl.Columns.Add("Мин. балл г/б", typeof(int));
            src_tbl.Columns.Add("Мин. балл дог", typeof(int));
            src_tbl.Columns.Add("Сред. балл г/б", typeof(double));
            src_tbl.Columns.Add("Сред. балл дог", typeof(double));
            src_tbl.Columns.Add("Макс. балл г/б", typeof(int));
            src_tbl.Columns.Add("Макс. балл дог", typeof(int));


            int iCntAbitBudzh = data_counts.Where(x => x.StudyBasisId == 1).Select(x => x.CNT_abit).Sum();
            int iCntAbitPlatn = data_counts.Where(x => x.StudyBasisId == 2).Select(x => x.CNT_abit).Sum();

            int iCntPersBudzh = data_counts.Where(x => x.StudyBasisId == 1).Select(x => x.CNT_Person).Sum();
            int iCntPersPlatn = data_counts.Where(x => x.StudyBasisId == 2).Select(x => x.CNT_Person).Sum();

            foreach (string fac in data_counts.Select(x => x.FacultyName).Distinct())
            {
                DataRow row = src_tbl.NewRow();

                row["Факультет"] = fac;
                int cnt = data_counts.Where(x => x.FacultyName == fac && x.StudyBasisId == 1).Select(x => x.CNT_abit).DefaultIfEmpty(0).First();
                row["Подано заявл. г/б"] = cnt;
                row["% заявл. г/б"] = iCntAbitBudzh == 0 ? 0f : Math.Round((float)cnt / (float)iCntAbitBudzh, 4) * 100f;
                cnt = data_counts.Where(x => x.FacultyName == fac && x.StudyBasisId == 2).Select(x => x.CNT_abit).DefaultIfEmpty(0).First();
                row["Подано заявл. дог"] = cnt;
                row["% заявл. дог"] = iCntAbitPlatn == 0 ? 0f : Math.Round((float)cnt / (float)iCntAbitPlatn, 4) * 100f;

                cnt = data_counts.Where(x => x.FacultyName == fac && x.StudyBasisId == 1).Select(x => x.CNT_Person).DefaultIfEmpty(0).First();
                row["Абитуриентов г/б"] = cnt;
                row["% абит. г/б"] = iCntPersBudzh == 0 ? 0f : Math.Round((float)cnt / (float)iCntPersBudzh, 4) * 100f;
                cnt = data_counts.Where(x => x.FacultyName == fac && x.StudyBasisId == 2).Select(x => x.CNT_Person).DefaultIfEmpty(0).First();
                row["Абитуриентов дог"] = cnt;
                row["% абит. дог"] = iCntPersPlatn == 0 ? 0f : Math.Round((float)cnt / (float)iCntPersPlatn, 4) * 100f;

                row["Мин. балл г/б"] = data_marks.Where(x => x.FacultyName == fac && x.StudyBasisId == 1).Select(x => x.MinSUM).Sum();
                row["Мин. балл дог"] = data_marks.Where(x => x.FacultyName == fac && x.StudyBasisId == 2).Select(x => x.MinSUM).Sum();
                row["Сред. балл г/б"] = data_marks.Where(x => x.FacultyName == fac && x.StudyBasisId == 1).Select(x => Math.Round(x.AvgSUM, 2)).Sum();
                row["Сред. балл дог"] = data_marks.Where(x => x.FacultyName == fac && x.StudyBasisId == 2).Select(x => Math.Round(x.AvgSUM, 2)).Sum();
                row["Макс. балл г/б"] = data_marks.Where(x => x.FacultyName == fac && x.StudyBasisId == 1).Select(x => x.MaxSUM).Sum();
                row["Макс. балл дог"] = data_marks.Where(x => x.FacultyName == fac && x.StudyBasisId == 2).Select(x => x.MaxSUM).Sum();

                src_tbl.Rows.Add(row);
            }

            dgv.DataSource = src_tbl;

            dgv.Columns["Факультет"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            dgv.Columns["Подано заявл. г/б"].Width = 50;
            dgv.Columns["Подано заявл. дог"].Width = 50;
            dgv.Columns["Абитуриентов г/б"].Width = 50;
            dgv.Columns["Абитуриентов дог"].Width = 50;
            dgv.Columns["% заявл. г/б"].Width = 50;
            dgv.Columns["% заявл. дог"].Width = 50;
            dgv.Columns["% абит. г/б"].Width = 50;
            dgv.Columns["% абит. дог"].Width = 50;
            dgv.Columns["Мин. балл г/б"].Width = 50;
            dgv.Columns["Мин. балл дог"].Width = 50;
            dgv.Columns["Сред. балл г/б"].Width = 50;
            dgv.Columns["Сред. балл дог"].Width = 50;
            dgv.Columns["Макс. балл г/б"].Width = 50;
            dgv.Columns["Макс. балл дог"].Width = 50;
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            try
            {
                RtfDocument doc = new RtfDocument(PaperSize.A4, PaperOrientation.Landscape, Lcid.Russian);
                RtfParagraph p = doc.addParagraph();
                p.Alignment = Align.Center;
                p.DefaultCharFormat.FontSize = 14.0f;
                p.DefaultCharFormat.FontStyle.addStyle(FontStyleFlag.Bold);
                p.Text = string.Format("Статистика по регионам по {0}абитуриентам", chbEntered.Checked ? "зачисленным " : "");

                doc.addParagraph().Text = "";
                p = doc.addParagraph();
                p.Alignment = Align.Left;
                p.Text = "По региону: " + cbRegion.Text;

                p = doc.addParagraph();
                //p.LineSpacing = RtfConstants.MILLIMETERS_TO_POINTS
                if (StudyFormId.HasValue)
                    p.Text = "Форма обучения: " + cbStudyForm.Text;
                else
                    p.Text = "Все формы обучения";

                doc.addParagraph().Text = "";

                RtfTable table = doc.addTable(dgv.Rows.Count + 1, dgv.ColumnCount);

                table.setInnerBorder(RtfWriter.BorderStyle.Single, 0.25f);
                table.setOuterBorder(RtfWriter.BorderStyle.Single, 0.25f);
                //заголовки
                for (int i = 0; i < dgv.ColumnCount; i++)
                {
                    table.cell(0, i).Alignment = Align.Center;
                    table.cell(0, i).addParagraph().Text = dgv.Columns[i].Name;
                }

                table.setColWidth(0, (float)(60.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(1, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(2, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(3, (float)(17.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(4, (float)(17.5m * RtfConstants.MILLIMETERS_TO_POINTS));

                table.setColWidth(5, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(6, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(7, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(8, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(9, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(10, (float)(12.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(11, (float)(17.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(12, (float)(17.5m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(13, (float)(10.0m * RtfConstants.MILLIMETERS_TO_POINTS));
                table.setColWidth(14, (float)(10.0m * RtfConstants.MILLIMETERS_TO_POINTS));

                DataView dv = new DataView((DataTable)dgv.DataSource);
                dv.Sort = "Абитуриентов г/б DESC";
                int rowId = 1;
                foreach (DataRow rw in dv.ToTable().Rows)
                {
                    int colId = 0;
                    foreach (DataColumn col in dv.ToTable().Columns)
                    {
                        var par = table.cell(rowId, colId).addParagraph();
                        par.Text = rw[col.ColumnName].ToString();
                        par.Alignment = Align.Center;
                        colId++;
                    }
                    //запрет переноса строк
                    table.setRowKeepInSamePage(rowId++, false);
                }

                doc.save(MainClass.saveTempFolder + "/RegionAbitStat_Rev_Report.rtf");
                System.Diagnostics.Process.Start(MainClass.saveTempFolder + "/RegionAbitStat_Rev_Report.rtf");
            }
            catch (Exception ex)
            {
                WinFormsServ.Error(ex.Message);
            }
        }
    }
}
