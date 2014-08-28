﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;
using System.Linq;
using PriemLib;

using EducServLib;
using PriemLib;

namespace Priem
{
    public static class ExportClass
    {
        public static void SetStudyNumbers()
        {
            if (!MainClass.IsPasha())
                return;
            
            using (PriemEntities context = new PriemEntities())
            {
                //взять максимум номера, если еще ничего не назначено
                string num = (from ab in context.extAbit
                              where ab.StudyLevelGroupId == MainClass.studyLevelGroupId
                              select ab.StudyNumber).Max();
               

                var abits = from ab in context.extAbit
                            join ev in context.extEntryView
                            on ab.Id equals ev.AbiturientId
                            where ab.StudyLevelGroupId == MainClass.studyLevelGroupId && (ab.StudyNumber == null || ab.StudyNumber.Length == 0)
                            orderby ab.FacultyId, ab.FIO
                            select ab;

                List<Guid> lstAbits = (from a in abits select a.Id).ToList();

                int stNum = 0;

                if (num != null && num.Length != 0)
                    stNum = int.Parse(num.Substring(3));

                stNum++;

                foreach (Guid abitId in lstAbits)
                {
                    string sNum = "0000" + stNum.ToString();
                    sNum = sNum.Substring(sNum.Length - 4, 4);
                    sNum = "13" + 5/*для 2013 года это префикс для СПО*/ + sNum;                

                    context.Abiturient_UpdateStudyNumber(sNum, abitId);                    
                    stNum++;
                }
                MessageBox.Show("Done");
            }            
        }

        public static void ExportVTB()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV files|*.csv";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                using (StreamWriter writer = new StreamWriter(sfd.OpenFile(), Encoding.GetEncoding(1251)))
                {
                    string[] headers = new string[] { 
                "Фамилия","Имя","Отчество","Пол","Дата рождения","Место рождения","Гражданство","Код документа"
                ,"Серия паспорта","Номер паспорта","Когда выдан паспорт","Кем выдан паспорт","Код подразделения"
                ,"Адрес регистрации","Индекс","Район","Город","Улица","Дом","Корпус","Квартира","Телефон по месту работы"
                ,"Контактный телефон","Рабочий телефон","Должность","Кодовое слово","Основной доход","Тип карты","Дата приема на работу"};


                    writer.WriteLine(string.Join(";", headers));


                    string query = @"select 
ed.person.surname, ed.person.name, ed.person.secondname,
case when ed.person.sex>0 then 'М' else 'Ж' end as sex,

CAST(
(
STR( DAY( ed.person.Birthdate ) ) + '/' +
STR( MONTH( ed.person.Birthdate ) ) + '/' +
STR( YEAR( ed.person.Birthdate ) )
)
AS DATETIME
) as birthdate,
ed.person.birthplace,
nation.name as nationality,
ed.passporttype.name as passporttype,
case when passporttypeid=1 then substring(ed.person.passportseries,1,2)+ ' ' + substring(ed.person.passportseries,3,2) else ed.person.passportseries end as passportseries, 
ed.person.passportnumber, ed.person.passportauthor, ed.person.passportcode,
CAST(
(
STR( DAY( ed.person.passportDate ) ) + '/' +
STR( MONTH( ed.person.passportDate ) ) + '/' +
STR( YEAR( ed.person.passportDate ) )
)
AS DATETIME
) as passportwhen,


ed.person.code,
ed.region.name as region,
ed.person.city,
ed.person.street,
ed.person.house,
ed.person.korpus,
ed.person.flat,

ed.person.codereal,
ed.person.cityreal,
ed.person.streetreal,
ed.person.housereal,
ed.person.korpusreal,
ed.person.flatreal,

ed.person.phone,
ed.person.mobiles
from
ed.extentryview 
inner join ed.extAbit on ed.extabit.id=ed.extentryview.abiturientid
inner join ed.person on ed.person.id=ed.extabit.personid
inner join ed.country as nation on nation.id=ed.person.nationalityid
inner join ed.passporttype on ed.passporttype.id=ed.person.passporttypeid
left join ed.region on ed.region.id=ed.person.regionid
where ed.extentryview.studyformid=1 and ed.extentryview.studybasisid=1 and ed.extabit.studylevelgroupid = " + MainClass.studyLevelGroupId;


                    DataSet ds = MainClass.Bdc.GetDataSet(query);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        List<string> list = new List<string>();

                        list.Add(row["surname"].ToString());
                        list.Add(row["name"].ToString());
                        list.Add(row["secondname"].ToString());
                        list.Add(row["sex"].ToString());
                        list.Add(DateTime.Parse(row["birthdate"].ToString()).ToString("dd/MM/yyyy"));

                        list.Add(row["birthplace"].ToString());
                        list.Add(row["nationality"].ToString());
                        list.Add(row["passporttype"].ToString());
                        list.Add(row["passportseries"].ToString());
                        list.Add(row["passportnumber"].ToString());

                        list.Add(DateTime.Parse(row["passportwhen"].ToString()).ToString("dd/MM/yyyy"));
                        list.Add(row["passportauthor"].ToString());
                        list.Add(row["passportcode"].ToString());
                        list.Add("по паспорту");
                        list.Add(row["code"].ToString());

                        list.Add(row["region"].ToString());
                        list.Add(row["city"].ToString());
                        list.Add(row["street"].ToString());
                        list.Add(row["house"].ToString());
                        list.Add(row["korpus"].ToString());

                        list.Add(row["flat"].ToString());
                        list.Add("");
                        list.Add(row["phone"].ToString() + ", " + row["mobiles"].ToString().Replace(";", ","));
                        list.Add("");
                        list.Add("студент");

                        list.Add("");
                        list.Add("0");
                        list.Add("visaelectron");
                        list.Add("01/09/2012");

                        writer.WriteLine(string.Join(";", list.ToArray()));
                    }
                }
            }
            catch
            {
                WinFormsServ.Error("Ошибка при экспорте");
            }
            return;
        }

        public static void ExportSber()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV files|*.csv";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                using (StreamWriter writer = new StreamWriter(sfd.OpenFile(), Encoding.GetEncoding(1251)))
                {
                    string[] headers = new string[] { 
                "Пол","ФИО","Паспорт","Дата выдачи", "Кем выдан", "Дата рождения","Место рождения",
                "Контрольное слово","Индекс","Адрес 1","Адрес 2","Адрес 3","Адрес 4","Телефон домашний","Телефон мобильный",
                "Телефон рабочий","Должность","Гражданство"};


                    writer.WriteLine(string.Join(";", headers));


                    string query = @"select 
ed.person.surname, ed.person.name, ed.person.secondname,
case when ed.person.sex>0 then 'М' else 'Ж' end as sex,

CAST(
(
STR( DAY( ed.person.Birthdate ) ) + '/' +
STR( MONTH( ed.person.Birthdate ) ) + '/' +
STR( YEAR( ed.person.Birthdate ) )
)
AS DATETIME
) as birthdate,
ed.person.birthplace,
nation.name as nationality,
ed.passporttype.name as passporttype,
case when passporttypeid=1 then substring(ed.person.passportseries,1,2)+ ' ' + substring(ed.person.passportseries,3,2) else ed.person.passportseries end as passportseries, 
ed.person.passportnumber, ed.person.passportauthor, ed.person.passportcode,
CAST(
(
STR( DAY( ed.person.passportDate ) ) + '/' +
STR( MONTH( ed.person.passportDate ) ) + '/' +
STR( YEAR( ed.person.passportDate ) )
)
AS DATETIME
) as passportwhen,


ed.person.code,
ed.region.name as region,
ed.person.city,
ed.person.street,
ed.person.house,
ed.person.korpus,
ed.person.flat,

ed.person.codereal,
ed.person.cityreal,
ed.person.streetreal,
ed.person.housereal,
ed.person.korpusreal,
ed.person.flatreal,

ed.person.phone,
ed.person.mobiles



from
ed.extentryview 
inner join ed.extAbit on ed.extabit.id=ed.extentryview.abiturientid
inner join ed.person on ed.person.id=ed.extabit.personid
inner join ed.country as nation on nation.id=ed.person.nationalityid
inner join ed.passporttype on ed.passporttype.id=ed.person.passporttypeid
left join ed.region on ed.region.id=ed.person.regionid
where ed.extentryview.studyformid=1 and ed.extentryview.studybasisid=1 and ed.extabit.studylevelgroupid = " + MainClass.studyLevelGroupId;


                    DataSet ds = MainClass.Bdc.GetDataSet(query);
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        List<string> list = new List<string>();

                        list.Add(row["sex"].ToString());
                        list.Add((row["surname"].ToString() + " " + row["name"].ToString() + " " + row["secondname"].ToString()).Trim());
                        list.Add((row["passportseries"].ToString() + " " + row["passportnumber"].ToString()).Trim());
                        list.Add(DateTime.Parse(row["passportwhen"].ToString()).ToString("dd.MM.yyyy"));
                        list.Add(row["passportauthor"].ToString());

                        list.Add(DateTime.Parse(row["birthdate"].ToString()).ToString("dd.MM.yyyy"));
                        list.Add(row["birthplace"].ToString());
                        list.Add("");
                        list.Add(row["code"].ToString());
                        list.Add(row["region"].ToString() + " " + row["city"].ToString());

                        list.Add(row["street"].ToString() + ", " + row["house"].ToString());
                        list.Add(row["korpus"].ToString());
                        list.Add(row["flat"].ToString());
                        list.Add(row["phone"].ToString());
                        list.Add(row["mobiles"].ToString().Replace(";", ","));

                        list.Add("");
                        list.Add("студент");
                        list.Add(row["nationality"].ToString());

                        writer.WriteLine(string.Join(";", list.ToArray()));
                    }
                }
            }
            catch
            {
                WinFormsServ.Error("Ошибка при экспорте");
            }
            return;
        }

        public static void SetAvgBall()
        {
            if (!MainClass.IsPasha())
                return;

            //OpenFileDialog ofd = new OpenFileDialog();

            //ofd.Filter = "CSV|*.csv";
            //ofd.Multiselect = true;

            //if (!(ofd.ShowDialog() == DialogResult.OK))
            //    return;

            ////try
            ////{
            //foreach (string fileName in ofd.FileNames)
            //{
            //    using (StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(1251)))
            //    {
            //        List<string[]> list = new List<string[]>();

            //        while (!sr.EndOfStream)
            //        {
            //            string str = sr.ReadLine();
            //            char[] split = new char[] { ';' };
            //            string[] values = str.Split(split, 4);

            //            list.Add(values);
            //        }

            //        int i = 0;
            //        foreach (string[] val in list)
            //        {
            //            i++;

            //            int regNum = 0;
            //            if (!int.TryParse(val[0].Trim(), out regNum))
            //            {
            //                MessageBox.Show("Строка " + i.ToString() + ": некорректный рег номер!");
            //                continue;
            //            }

            //            if (regNum.ToString().Substring(0, 1) != "1")
            //                continue;

            //            int rNum = int.Parse(regNum.ToString().Substring(1));

            //            double sum = 0;
            //            if (!double.TryParse(val[1].Trim(), out sum))
            //                sum = 0;

            //            int stat = 0;
            //            if (!int.TryParse(val[2].Trim(), out stat))
            //                stat = 5;

            //            string abId = MainClass.Bdc.GetStringValue(string.Format("SELECT qAbiturient.Id FROM qAbiturient INNER JOIN Person ON qAbiturient.Personid = Person.Id INNER JOIN extEntryView ON qAbiturient.Id = extEntryView.AbiturientId WHERE Person.Num = {0} ", rNum));

            //            if (abId == string.Empty)
            //                continue;

            //            SortedList<string, object> sl = new SortedList<string, object>();

            //            sl.Add("SessionAVG", sum);
            //            if (stat < 2)
            //                sl.Add("StudentStatus", stat);

            //            MainClass.Bdc.UpdateRecord("Abiturient", abId, sl);
            //        }
            //    }
            //}

            MessageBox.Show("Выполнено!");
            //}
            //catch (Exception exc)
            //{
            //    WinFormsServ.Error("Ошибка загрузки:" + exc.Message);
            //}
        }

        public static void SetPayData()
        {
            if (!MainClass.IsPasha())
                return;

            //OpenFileDialog ofd = new OpenFileDialog();

            //ofd.Filter = "CSV|*.csv";
            //ofd.Multiselect = true;

            //if (!(ofd.ShowDialog() == DialogResult.OK))
            //    return;

            ////try
            ////{
            //foreach (string fileName in ofd.FileNames)
            //{
            //    using (StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(1251)))
            //    {
            //        List<string[]> list = new List<string[]>();

            //        while (!sr.EndOfStream)
            //        {
            //            string str = sr.ReadLine();
            //            char[] split = new char[] { ';' };
            //            string[] values = str.Split(split, 11);

            //            list.Add(values);
            //        }

            //        int i = 0;
            //        foreach (string[] val in list)
            //        {
            //            i++;

            //            int planId = 0;
            //            if (!int.TryParse(val[0].Trim(), out planId))
            //            {
            //                MessageBox.Show("Строка " + i.ToString() + ": некорректный номер плана!");
            //                continue;
            //            }                       

            //            string unName = val[1].Trim();
            //            string unAddress = val[2].Trim();
            //            string unINN = val[3].Trim();
            //            string unRS = val[4].Trim();
            //            string unDop = val[5].Trim();
                        
            //            int prorId = 0;
            //            if (!int.TryParse(val[6].Trim(), out prorId))
            //                prorId = 0;

            //            string qualif = val[7].Trim();
            //            string srok = val[8].Trim() + " года";

            //            string dateStart = val[9].Replace("\"", "").Trim();
            //            DateTime dtStart = DateTime.Now;
            //            if (!DateTime.TryParse(dateStart, out dtStart))  
            //                continue;

            //            string dateFinish = val[10].Replace("\"", "").Trim();
            //            DateTime dtFinish = DateTime.Now;
            //            if (!DateTime.TryParse(dateFinish, out dtFinish))
            //                continue;                      
                        
            //            SortedList<string, object> sl = new SortedList<string, object>();
                                                
            //            sl.Add("UniverName", unName);
            //            sl.Add("UniverAddress", unAddress);
            //            sl.Add("UniverINN", unINN);
            //            sl.Add("UniverRS", unRS);
            //            sl.Add("UniverDop", unDop);
            //            sl.Add("ProrektorId", prorId);
            //            sl.Add("Qualification", qualif);
            //            sl.Add("EducPeriod", srok);
            //            sl.Add("DateStart", dtStart);
            //            sl.Add("DateFinish", dtFinish);

            //            int cnt = (int)MainClass.Bdc.GetValue(string.Format("SELECT Count(StudyPlanId) FROM PayDataStudyPlan WHERE StudyPlanId = {0}", planId));
            //            if (cnt == 0)
            //            {
            //                sl.Add("StudyPlanId", planId);
            //                MainClass.Bdc.InsertRecord("PayDataStudyPlan", sl);
            //            }
            //            else
            //                MainClass.Bdc.UpdateRecordOnSetField("PayDataStudyPlan", "StudyPlanId", planId.ToString(), sl);
            //        }
            //    }
            //}

            MessageBox.Show("Выполнено!");
            //}
            //catch (Exception exc)
            //{
            //    WinFormsServ.Error("Ошибка загрузки:" + exc.Message);
            //}
        }
    }
}
