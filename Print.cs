using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Data.Objects;
using WordOut;
using iTextSharp.text;
using iTextSharp.text.pdf;

using EducServLib;
using PriemLib;

namespace Priem
{
    public class Print
    {
        public static void PrintApplication(Guid? abitId, bool forPrint, string savePath)
        {
            FileStream fileS = null;

            try
            {
                using (PriemEntities context = new PriemEntities())
                {
                    Guid PersonId = (from ab in context.extAbit
                                     where ab.Id == abitId
                                     select ab.PersonId).FirstOrDefault();

                    var abitList = (from x in context.Abiturient
                                    join Entry in context.Entry on x.EntryId equals Entry.Id
                                    where Entry.StudyLevel.StudyLevelGroup.Id == MainClass.studyLevelGroupId
                                    && x.IsGosLine == false
                                    && x.PersonId == PersonId
                                    && x.BackDoc == false
                                    select new
                                    {
                                        x.Id,
                                        x.PersonId,
                                        x.Barcode,
                                        Faculty = Entry.SP_Faculty.Name,
                                        Profession = Entry.SP_LicenseProgram.Name,
                                        ProfessionCode = Entry.SP_LicenseProgram.Code,
                                        ObrazProgram = Entry.StudyLevel.Acronym + "." + Entry.SP_ObrazProgram.Number + "." + MainClass.PriemYear + " " + Entry.SP_ObrazProgram.Name,
                                        Specialization = Entry.ProfileName,
                                        Entry.StudyFormId,
                                        Entry.StudyForm.Name,
                                        Entry.StudyBasisId,
                                        EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                        Entry.StudyLevelId,
                                        x.Priority,
                                        x.IsGosLine,
                                        Entry.CommissionId,
                                        ComissionAddress = Entry.CommissionId
                                    }).OrderBy(x => x.Priority).ToList();

                    extPerson person = (from per in context.extPerson
                                        where per.Id == PersonId
                                           select per).FirstOrDefault();

                    string tmp;
                    string dotName;

                    dotName = "ApplicationSPO_2014";

                    using (FileStream fs = new FileStream(string.Format(@"{0}\{1}.pdf", MainClass.dirTemplates, dotName), FileMode.Open, FileAccess.Read))
                    {

                        byte[] bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, bytes.Length);
                        fs.Close();

                        PdfReader pdfRd = new PdfReader(bytes);

                        try
                        {
                            fileS = new FileStream(string.Format(savePath), FileMode.Create);
                        }
                        catch
                        {
                            if (fileS != null)
                                fileS.Dispose();
                            WinFormsServ.Error("Пожалуйста, закройте открытые файлы pdf");
                            return;
                        }


                        PdfStamper pdfStm = new PdfStamper(pdfRd, fileS);
                        pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "",
                        PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING |
                        PdfWriter.AllowPrinting);
                        AcroFields acrFlds = pdfStm.AcroFields;

                        //чей мы рисуем штрих-код??
                        //upd 03.06.2011 - уже не рисуем
                        //Barcode128 barcode = new Barcode128();
                        //barcode.Code = abit.RegNum;
                        //barcode.Code = "0008456";

                        PdfContentByte cb = pdfStm.GetOverContent(1);

                        //iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
                        //img.SetAbsolutePosition(420, 720);
                        //cb.AddImage(img);
                        //acrFlds.SetField("RegNum", abit.PersonNum + @"\" + abit.RegNum);
                        string[] splitStr;

                        // ФИО
                        acrFlds.SetField("FIO", person.FIO);
                        acrFlds.SetField("Male", person.Sex ? "1" : "0");
                        acrFlds.SetField("Female", person.Sex ? "0" : "1");
                        // дата рождения
                        acrFlds.SetField("BirthDateYear", person.BirthDate.Year.ToString("D2"));
                        acrFlds.SetField("BirthDateMonth", person.BirthDate.Month.ToString("D2"));
                        acrFlds.SetField("BirthDateDay", person.BirthDate.Day.ToString());
                        acrFlds.SetField("BirthPlace", person.BirthPlace);
                        // паспорт
                        acrFlds.SetField("Nationality", person.NationalityName);
                        if (person.NationalityId == 1)
                            acrFlds.SetField("HasRussianNationalityYes", "1");
                        acrFlds.SetField("PassportSeries", person.PassportSeries);
                        acrFlds.SetField("PassportNumber", person.PassportNumber);
                        splitStr = GetSplittedStrings(person.PassportAuthor + " " + person.PassportDate.Value.ToString("dd.MM.yyyy"), 60, 70, 2);
                        for (int ii = 1; ii <= 2; ii++)
                            acrFlds.SetField("PassportAuthor" + ii, splitStr[ii - 1]);
                        // адрес
                        string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.NationalityId == 1 ? (person.RegionName + ", ") ?? "" : person.CountryName + ", "), (person.City + ", ") ?? "") +
                        string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                        person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                        person.Flat == string.Empty ? "" : "кв. " + person.Flat);
                        splitStr = GetSplittedStrings(Address, 50, 70, 3);
                        for (int ii = 1; ii <= 3; ii++)
                            acrFlds.SetField("Address" + ii, splitStr[ii - 1]);
                        // телефон
                        acrFlds.SetField("Phone", person.Phone);
                        acrFlds.SetField("Email", person.Email);
                        acrFlds.SetField("Mobiles", person.Mobiles);
                        // общежитие
                        acrFlds.SetField("HostelEducYes", person.HostelEduc ?? false ? "1" : "0");
                        acrFlds.SetField("HostelEducNo", person.HostelEduc ?? false ? "0" : "1");
                        acrFlds.SetField("HostelAbitYes", person.HostelAbit ?? false ? "1" : "0");
                        acrFlds.SetField("HostelAbitNo", person.HostelAbit ?? false ? "0" : "1");
                        // стаж
                        if (person.Stag != string.Empty)
                        {
                            acrFlds.SetField("HasStag", "1");
                            acrFlds.SetField("Stag", person.Stag);
                            acrFlds.SetField("WorkPlace", person.WorkPlace);
                        }
                        else
                            acrFlds.SetField("NoStag", "1");

                        if ((int)person.Privileges > 0)
                            acrFlds.SetField("Privileges", "1");


                        string LangName = (from ab in context.Language
                                     where ab.Id == person.LanguageId
                                     select ab.Name).FirstOrDefault();
                        
                        acrFlds.SetField("Language", LangName ?? "");

                        if (person.HighEducation != string.Empty)
                        {
                            acrFlds.SetField("HasEduc", "1");
                            acrFlds.SetField("HighEducation", person.HighEducation + " " + person.HEProfession);
                        }
                        else
                            acrFlds.SetField("NoEduc", "1");
                        tmp = person.StartEnglish ?? false ? "Yes" : "No";  //+
                        acrFlds.SetField("chbEnglish" + tmp, "1");          //+
                        acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());  
                        // спорт
                        string SportQualification = "";
                        extPersonSPO personSPO = (from per in context.extPersonSPO
                                                  where per.Id == PersonId
                                                  select per).FirstOrDefault();
                        if (personSPO != null)
                        {
                            if (personSPO.SportQualificationId.HasValue && personSPO.SportQualificationId > 0)
                                SportQualification = personSPO.SportQualificationName + ((!String.IsNullOrEmpty(personSPO.SportQualificationLevel)) ? " разряд:" + personSPO.SportQualificationLevel : "");

                            else if (!personSPO.SportQualificationId.HasValue || personSPO.SportQualificationId == 0)
                                SportQualification = "нет";

                            else if (!personSPO.SportQualificationId.HasValue || personSPO.SportQualificationId == 44)
                                SportQualification = personSPO.OtherSportQualification;
                       
                            acrFlds.SetField("SportQualification", SportQualification); 
                        }
                        // Полученное образование
                        string SchoolTypeName = context.SchoolType.Where(x => x.Id == person.SchoolTypeId).Select(x => x.Name).First();
                        if (SchoolTypeName + person.SchoolName + person.SchoolNum + person.SchoolCity != string.Empty)
                            acrFlds.SetField("chbSchoolFinished", "1");

                        string CountryEducName = context.Country.Where(x => x.Id == person.CountryEducId).Select(x => x.Name).FirstOrDefault();
                        acrFlds.SetField("CountryEduc", CountryEducName);

                        acrFlds.SetField("ExitYear", person.SchoolExitYear.ToString());
                        splitStr = GetSplittedStrings(person.SchoolName ?? "", 50, 70, 2);
                        for (int ii = 1; ii <= 2; ii++)
                            acrFlds.SetField("School" + ii, splitStr[ii - 1]);

                        string attreg = person.AttestatRegion;
                        if (person.SchoolTypeId == 1)
                            acrFlds.SetField("Attestat", string.Format("аттестат  {0} серия {1} № {2}", attreg == string.Empty ? "" : "регион " + attreg, person.AttestatSeries, person.AttestatNum));
                        else
                            acrFlds.SetField("Attestat", string.Format("диплом серия {0} № {1}", person.DiplomSeries, person.DiplomNum));

                        string queryEge = "SELECT TOP 5 EgeMark.Id, EgeExamName.Name AS ExamName, EgeMark.Value, " +
                                      "EgeCertificate.Number, EgeMark.EgeCertificateId " +
                                      "FROM ed.EgeMark LEFT JOIN ed.EgeExamName ON EgeMark.EgeExamNameId = EgeExamName.Id " +
                                      "LEFT JOIN ed.EgeToExam ON EgeExamName.Id = EgeToExam.EgeExamNameId " +
                                      "LEFT JOIN ed.EgeCertificate ON EgeMark.EgeCertificateId = EgeCertificate.Id " +
                                      "LEFT JOIN ed.extPersonSPO ON EgeCertificate.PersonId = extPersonSPO.Id " +
                                      "LEFT JOIN ed.qAbiturient ON qAbiturient.PersonId = extPersonSPO.Id " +
                                      "WHERE qAbiturient.Id = '" + abitId + "'" +
                                      "AND EgeToExam.ExamId IN (SELECT ExamId FROM ed.ExamInEntry WHERE ExamInEntry.EntryId = qAbiturient.EntryId) " +
                                      "ORDER BY EgeMark.EgeCertificateId ";

                        DataSet dsEge = MainClass.Bdc.GetDataSet(queryEge);

                        int i = 1;

                        foreach (DataRow dre in dsEge.Tables[0].Rows)
                        {
                            acrFlds.SetField("TableName" + i, dre["ExamName"].ToString());
                            acrFlds.SetField("TableValue" + i, dre["Value"].ToString());
                            acrFlds.SetField("TableNumber" + i, dre["Number"].ToString());
                            i++;
                        }
                        /*
                        acrFlds.SetField("Priority1", abit.Priority.ToString()); 
                        acrFlds.SetField("Profession1", "(" + abit.LicenseProgramCode + ") " + abit.LicenseProgramName);
                        acrFlds.SetField("Specialization1", abit.ProfileName);
                        acrFlds.SetField("ObrazProgram1", abit.ObrazProgramCrypt + " " + abit.ObrazProgramName);
                        acrFlds.SetField("StudyBasis" + abit.StudyBasisId.ToString() + "1", "1");
                        acrFlds.SetField("StudyForm" + abit.StudyFormId.ToString() + "1", "1");
                        */

                        for (int ii=0; ii< abitList.Count; ii++)
                        {
                            acrFlds.SetField("Priority"+(ii+1).ToString(), abitList[ii].Priority.ToString());
                            acrFlds.SetField("Profession" + (ii + 1).ToString(), "(" + abitList[ii].ProfessionCode + ") " + abitList[ii].Profession);
                            acrFlds.SetField("Specialization" + (ii + 1).ToString(), abitList[ii].Specialization);
                            acrFlds.SetField("ObrazProgram" + (ii + 1).ToString(), abitList[ii].ObrazProgram);
                            acrFlds.SetField("StudyBasis" + abitList[ii].StudyBasisId.ToString() + (ii + 1).ToString(), "1");
                            acrFlds.SetField("StudyForm" + abitList[ii].StudyFormId.ToString() + (ii + 1).ToString(), "1");
                        }


                        string addInfo = person.Mobiles.Replace('\r', ',').Replace('\n', ' ').Trim();//если начнут вбивать построчно, то хотя бы в одну строку сведём
                        if (addInfo.Length > 100)
                        {
                            int cutpos = 0;
                            cutpos = addInfo.Substring(0, 100).LastIndexOf(',');
                            addInfo = addInfo.Substring(0, cutpos) + "; ";
                        }

                        acrFlds.SetField("Original", "0");  
                        acrFlds.SetField("Copy", "1");

                        // олимпиады
                        acrFlds.SetField("Extra", person.ExtraInfo + "\r\n" + person.ScienceWork);

                        //экстр. случаи
                        tmp = person.PersonInfo.Replace('\r', ';').Replace('\n', ' ').Trim();
                        string[] mamaPapaWords = tmp.Split(' ');

                        string[] mamaPapa = new string[3];
                        string strb = "";
                        int index = 0;
                        foreach (string str in mamaPapaWords)
                        {
                            if (index >= 2)
                                break;
                            if (strb.Length + str.Length < 40 && index == 0 || strb.Length + str.Length < 80 && index != 0)
                                strb += str + " ";
                            else
                            {
                                mamaPapa[index] = strb + str + " ";
                                index++;
                                strb = "";
                                continue;
                            }
                            mamaPapa[index] = strb;
                        }
                        acrFlds.SetField("Parents1", mamaPapa[0]);
                        acrFlds.SetField("Parents2", mamaPapa[1]);
                        acrFlds.SetField("Parents3", mamaPapa[2]);


                        pdfStm.FormFlattening = true;
                        pdfStm.Close();
                        pdfRd.Close();

                        Process pr = new Process();
                        if (forPrint)
                        {
                            pr.StartInfo.Verb = "Print";
                            pr.StartInfo.FileName = string.Format(savePath);
                            pr.Start();
                        }
                        else
                        {
                            pr.StartInfo.Verb = "Open";
                            pr.StartInfo.FileName = string.Format(savePath);
                            pr.Start();
                        }
                    }
                }
            }

            catch (Exception exc)
            {
                WinFormsServ.Error(exc.Message);
            }
            finally
            {
                if (fileS != null)
                    fileS.Dispose();
            }
        }
        public static string[] GetSplittedStrings(string sourceStr, int firstStrLen, int strLen, int numOfStrings)
        {
            sourceStr = sourceStr ?? "";
            string[] retStr = new string[numOfStrings];
            int index = 0, startindex = 0;
            for (int i = 0; i < numOfStrings; i++)
            {
                if (sourceStr.Length > startindex && startindex >= 0)
                {
                    int rowLength = firstStrLen;//длина первой строки
                    if (i > 1) //длина остальных строк одинакова
                        rowLength = strLen;
                    index = startindex + rowLength;
                    if (index < sourceStr.Length)
                    {
                        index = sourceStr.IndexOf(" ", index);
                        string val = index > 0 ? sourceStr.Substring(startindex, index - startindex) : sourceStr.Substring(startindex);
                        retStr[i] = val;
                    }
                    else
                        retStr[i] = sourceStr.Substring(startindex);
                }
                startindex = index;
            }

            return retStr;
        }
    }
}