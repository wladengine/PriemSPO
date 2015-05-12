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
                                    join Entry in context.extEntry on x.EntryId equals Entry.Id
                                    where Entry.StudyLevelGroupId == MainClass.studyLevelGroupId
                                    && x.IsGosLine == false
                                    && x.PersonId == PersonId
                                    && x.BackDoc == false
                                    select new
                                    {
                                        x.Id,
                                        x.PersonId,
                                        x.Barcode,
                                        Faculty = Entry.FacultyName,
                                        Profession = Entry.LicenseProgramName,
                                        ProfessionCode = Entry.LicenseProgramCode,
                                        ObrazProgram = Entry.ObrazProgramCrypt + " " + Entry.ObrazProgramName,
                                        Specialization = Entry.ProfileName,
                                        Entry.StudyFormId,
                                        Entry.StudyFormName,
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

                        PdfContentByte cb = pdfStm.GetOverContent(1);

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
                        acrFlds.SetField("HostelEducYes", person.HostelEduc ? "1" : "0");
                        acrFlds.SetField("HostelEducNo", person.HostelEduc ? "0" : "1");
                        acrFlds.SetField("HostelAbitYes", person.HostelAbit ? "1" : "0");
                        acrFlds.SetField("HostelAbitNo", person.HostelAbit ? "0" : "1");
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
                        tmp = person.StartEnglish ? "Yes" : "No";
                        acrFlds.SetField("chbEnglish" + tmp, "1");
                        acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());  
                        // спорт
                        string SportQualification = "";
                        extPerson pSPO = (from per in context.extPerson
                                                  where per.Id == PersonId
                                                  select per).FirstOrDefault();
                        if (pSPO != null)
                        {
                            if (pSPO.SportQualificationId.HasValue && pSPO.SportQualificationId > 0)
                                SportQualification = pSPO.SportQualificationName + ((!String.IsNullOrEmpty(pSPO.SportQualificationLevel)) ? " разряд:" + pSPO.SportQualificationLevel : "");

                            else if (!pSPO.SportQualificationId.HasValue || pSPO.SportQualificationId == 0)
                                SportQualification = "нет";

                            else if (!pSPO.SportQualificationId.HasValue || pSPO.SportQualificationId == 44)
                                SportQualification = pSPO.OtherSportQualification;
                       
                            acrFlds.SetField("SportQualification", SportQualification); 
                        }

                        // Полученное образование
                        string SchoolTypeName = context.SchoolType.Where(x => x.Id == pSPO.SchoolTypeId).Select(x => x.Name).First();
                        if (SchoolTypeName + pSPO.SchoolName + pSPO.SchoolNum + pSPO.SchoolCity != string.Empty)
                            acrFlds.SetField("chbSchoolFinished", "1");

                        string CountryEducName = context.Country.Where(x => x.Id == pSPO.CountryEducId).Select(x => x.Name).FirstOrDefault();
                        acrFlds.SetField("CountryEduc", CountryEducName);

                        acrFlds.SetField("ExitYear", pSPO.SchoolExitYear.ToString());
                        splitStr = GetSplittedStrings(pSPO.SchoolName ?? "", 50, 70, 2);
                        for (int ii = 1; ii <= 2; ii++)
                            acrFlds.SetField("School" + ii, splitStr[ii - 1]);

                        if (pSPO.SchoolTypeId == 1)
                            acrFlds.SetField("Attestat", string.Format("аттестат серия {0} № {1}", pSPO.AttestatSeries, pSPO.AttestatNum));
                        else
                            acrFlds.SetField("Attestat", string.Format("диплом серия {0} № {1}", pSPO.DiplomSeries, pSPO.DiplomNum));

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
                        
                        for (int ii=0; ii< abitList.Count; ii++)
                        {
                            acrFlds.SetField("Priority"+(ii+1).ToString(), abitList[ii].Priority.ToString());
                            acrFlds.SetField("Profession" + (ii + 1).ToString(), "(" + abitList[ii].ProfessionCode + ") " + abitList[ii].Profession);
                            acrFlds.SetField("Specialization" + (ii + 1).ToString(), abitList[ii].Specialization);
                            acrFlds.SetField("ObrazProgram" + (ii + 1).ToString(), abitList[ii].ObrazProgram);
                            acrFlds.SetField("StudyBasis" + abitList[ii].StudyBasisId.ToString() + (ii + 1).ToString(), "1");
                            acrFlds.SetField("StudyForm" + abitList[ii].StudyFormId.ToString() + (ii + 1).ToString(), "1");
                        }


                        string addInfo = pSPO.Mobiles.Replace('\r', ',').Replace('\n', ' ').Trim();//если начнут вбивать построчно, то хотя бы в одну строку сведём
                        if (addInfo.Length > 100)
                        {
                            int cutpos = 0;
                            cutpos = addInfo.Substring(0, 100).LastIndexOf(',');
                            addInfo = addInfo.Substring(0, cutpos) + "; ";
                        }

                        acrFlds.SetField("Original", "0");  
                        acrFlds.SetField("Copy", "1");

                        // олимпиады
                        acrFlds.SetField("Extra", pSPO.ExtraInfo + "\r\n" + pSPO.ScienceWork);

                        //экстр. случаи
                        tmp = pSPO.PersonInfo.Replace('\r', ';').Replace('\n', ' ').Trim();
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
        public static byte[] GetApplicationPDF_SPO(Guid appId, string dirPath, Guid PersonId)
        {
            using (PriemEntities context = new PriemEntities())
            {
                var abitList = (from x in context.extAbit
                                join Entry in context.extEntry on x.EntryId equals Entry.Id
                                where x.CommitId == appId
                                select new
                                {
                                    x.Id,
                                    x.PersonId,
                                    x.Barcode,
                                    Faculty = Entry.FacultyName,
                                    Profession = Entry.LicenseProgramName,
                                    ProfessionCode = Entry.LicenseProgramCode,
                                    ObrazProgram = Entry.ObrazProgramName,
                                    Specialization = Entry.ProfileName,
                                    Entry.StudyFormId,
                                    Entry.StudyFormName,
                                    Entry.StudyBasisId,
                                    EntryType = (Entry.StudyLevelId == 17 ? 2 : 1),
                                    Entry.StudyLevelId,
                                    CommitIntNumber = x.CommitNumber,
                                    x.Priority,
                                    IsGosLine = Entry.IsForeign && Entry.StudyBasisId == 1
                                    //x.IsGosLine
                                }).ToList();

                var person = (from x in context.Person
                              where x.Id == PersonId
                              select new
                              {
                                  x.Surname,
                                  x.Name,
                                  x.SecondName,
                                  x.Barcode,
                                  x.Person_AdditionalInfo.HostelAbit,
                                  x.BirthDate,
                                  BirthPlace = x.BirthPlace ?? "",
                                  Sex = x.Sex,
                                  Nationality = x.Nationality.Name,
                                  Country = x.Person_Contacts.Country.Name,
                                  PassportType = x.PassportType.Name,
                                  x.PassportSeries,
                                  x.PassportNumber,
                                  x.PassportAuthor,
                                  x.PassportDate,
                                  x.Person_Contacts.City,
                                  Region = x.Person_Contacts.Region.Name,
                                  x.Person_Contacts.Code,
                                  x.Person_Contacts.Street,
                                  x.Person_Contacts.House,
                                  x.Person_Contacts.Korpus,
                                  x.Person_Contacts.Flat,
                                  x.Person_Contacts.Email,
                                  x.Person_Contacts.Phone,
                                  x.Person_Contacts.Mobiles,
                                  AddInfo = x.Person_AdditionalInfo.ExtraInfo,
                                  Parents = x.Person_AdditionalInfo.PersonInfo,
                                  HasPrivileges = x.Person_AdditionalInfo.Privileges > 0,
                                  SportQualificationName = x.PersonSportQualification.SportQualification.Name,
                                  x.PersonSportQualification.SportQualificationId,
                                  x.PersonSportQualification.SportQualificationLevel,
                                  x.PersonSportQualification.OtherSportQualification,
                                  x.Person_AdditionalInfo.HostelEduc,
                                  x.Person_Contacts.ForeignCountry.IsRussia,
                                  x.HasRussianNationality,
                                  x.Person_AdditionalInfo.StartEnglish,
                                  x.Person_AdditionalInfo.EnglishMark,
                                  Language = x.Person_AdditionalInfo.Language.Name,
                                  x.Person_AdditionalInfo.HasTRKI,
                                  x.Person_AdditionalInfo.TRKICertificateNumber,
                              }).FirstOrDefault();

                var personEducation = context.Person_EducationInfo.Where(x => x.PersonId == PersonId)
                    .Select(x => new
                    {
                        x.SchoolExitYear,
                        x.SchoolName,
                        x.IsEqual,
                        x.EqualDocumentNumber,
                        CountryEduc = x.CountryEducId != null ? x.Country.Name : "",
                        x.CountryEducId,
                        x.SchoolTypeId,
                        EducationDocumentSeries = x.SchoolTypeId == 1 ? x.AttestatSeries : x.DiplomSeries,
                        EducationDocumentNumber = x.SchoolTypeId == 1 ? x.AttestatNum : x.DiplomNum,
                    }).FirstOrDefault();

                MemoryStream ms = new MemoryStream();
                string dotName = "ApplicationSPO_page3.pdf";

                byte[] templateBytes;
                using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
                {
                    templateBytes = new byte[fs.Length];
                    fs.Read(templateBytes, 0, templateBytes.Length);
                }

                PdfReader pdfRd = new PdfReader(templateBytes);
                PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
                //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);
                AcroFields acrFlds = pdfStm.AcroFields;

                acrFlds.SetField("FIO", ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim());
                List<byte[]> lstFiles = new List<byte[]>();

                if (person.HostelEduc)
                    acrFlds.SetField("HostelEducYes", "1");
                else
                    acrFlds.SetField("HostelEducNo", "1");

                if (abitList.Where(x => x.IsGosLine).Count() > 0)
                    acrFlds.SetField("IsGosLine", "1");

                acrFlds.SetField("HostelAbitYes", person.HostelAbit ? "1" : "0";
                acrFlds.SetField("HostelAbitNo", person.HostelAbit ? "0" : "1";

                acrFlds.SetField("BirthDateYear", person.BirthDate.Year.ToString("D2"));
                acrFlds.SetField("BirthDateMonth", person.BirthDate.Month.ToString("D2"));
                acrFlds.SetField("BirthDateDay", person.BirthDate.Day.ToString());

                acrFlds.SetField("BirthPlace", person.BirthPlace);
                acrFlds.SetField("Male", person.Sex ? "1" : "0");
                acrFlds.SetField("Female", person.Sex ? "0" : "1");
                acrFlds.SetField("Nationality", person.Nationality);
                acrFlds.SetField("PassportSeries", person.PassportSeries);
                acrFlds.SetField("PassportNumber", person.PassportNumber);

                //dd.MM.yyyy :12.05.2000
                string[] splitStr = GetSplittedStrings(person.PassportAuthor + " " + person.PassportDate.Value.ToString("dd.MM.yyyy"), 60, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("PassportAuthor" + i, splitStr[i - 1]);

                string Address = string.Format("{0} {1}{2},", (person.Code) ?? "", (person.IsRussia ? (person.Region + ", ") ?? "" : person.Country + ", "), (person.City + ", ") ?? "") +
                    string.Format("{0} {1} {2} {3}", person.Street ?? "", person.House == string.Empty ? "" : "дом " + person.House,
                    person.Korpus == string.Empty ? "" : "корп. " + person.Korpus,
                    person.Flat == string.Empty ? "" : "кв. " + person.Flat);

                if (person.HasRussianNationality)
                    acrFlds.SetField("HasRussianNationalityYes", "1");
                else
                    acrFlds.SetField("HasRussianNationalityNo", "1");

                splitStr = GetSplittedStrings(Address, 50, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Address" + i, splitStr[i - 1]);

                acrFlds.SetField("EnglishMark", person.EnglishMark.ToString());
                if (person.StartEnglish)
                    acrFlds.SetField("chbEnglishYes", "1");
                else
                    acrFlds.SetField("chbEnglishNo", "1");

                acrFlds.SetField("Phone", person.Phone);
                acrFlds.SetField("Email", person.Email);
                acrFlds.SetField("Mobiles", person.Mobiles);

                acrFlds.SetField("ExitYear", personEducation.SchoolExitYear.ToString());
                splitStr = GetSplittedStrings(personEducation.SchoolName ?? "", 50, 70, 2);
                for (int i = 1; i <= 2; i++)
                    acrFlds.SetField("School" + i, splitStr[i - 1]);

                //только у магистров
                var HEInfo = context.Person_EducationInfo
                    .Where(x => x.PersonId == PersonId)
                    .Select(x => new { ProgramName = x.HEProfession, Qualification = x.HEQualification }).FirstOrDefault();

                if (HEInfo != null)
                {
                    acrFlds.SetField("HEProfession", HEInfo.ProgramName ?? "");
                    acrFlds.SetField("Qualification", HEInfo.Qualification ?? "");

                    acrFlds.SetField("Original", "0");
                    acrFlds.SetField("Copy", "0");
                    acrFlds.SetField("CountryEduc", personEducation.CountryEduc ?? "");
                    acrFlds.SetField("Language", person.Language ?? "");
                }
                //SportQualification
                if (person.SportQualificationId == 0)
                    acrFlds.SetField("SportQualification", "нет");
                else if (person.SportQualificationId == 44)
                    acrFlds.SetField("SportQualification", person.OtherSportQualification);
                else
                    acrFlds.SetField("SportQualification", person.SportQualificationName + "; " + person.SportQualificationLevel);

                string extraPerson = person.Parents ?? "";
                splitStr = GetSplittedStrings(extraPerson, 70, 70, 3);
                for (int i = 1; i <= 3; i++)
                    acrFlds.SetField("Parents" + i.ToString(), splitStr[i - 1]);

                string Attestat = personEducation.SchoolTypeId == 1 ? ("аттестат серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? "")) :
                        ("диплом серия " + (personEducation.EducationDocumentSeries ?? "") + " №" + (personEducation.EducationDocumentNumber ?? ""));
                acrFlds.SetField("Attestat", Attestat);
                acrFlds.SetField("Extra", person.AddInfo ?? "");

                if (personEducation.IsEqual && personEducation.CountryEducId != 193)
                {
                    acrFlds.SetField("IsEqual", "1");
                    acrFlds.SetField("EqualSertificateNumber", personEducation.EqualDocumentNumber);
                }
                else
                {
                    acrFlds.SetField("NoEqual", "1");
                }

                if (person.HasPrivileges)
                    acrFlds.SetField("HasPrivileges", "1");

                if ((personEducation.SchoolTypeId != 2) && (personEducation.SchoolTypeId != 5))//SSUZ & SPO
                    acrFlds.SetField("NoEduc", "1");
                else
                {
                    acrFlds.SetField("HasEduc", "1");
                    acrFlds.SetField("HighEducation", personEducation.SchoolName);
                }

                //VSEROS
                var OlympVseros = context.Olympiads.Where(x => x.PersonId == PersonId && x.OlympType.IsVseross)
                    .Select(x => new { x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                int egeCnt = 1;
                foreach (var ex in OlympVseros)
                {
                    acrFlds.SetField("OlympVserosName" + egeCnt, ex.Name);
                    acrFlds.SetField("OlympVserosYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympVserosDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                //OTHEROLYMPS
                var OlympNoVseros = context.Olympiads.Where(x => x.PersonId == PersonId && !x.OlympType.IsVseross)
                    .Select(x => new { x.OlympName.Name, OlympSubject = x.OlympSubject.Name, x.DocumentDate, x.DocumentSeries, x.DocumentNumber }).ToList();
                egeCnt = 1;
                foreach (var ex in OlympNoVseros)
                {
                    acrFlds.SetField("OlympName" + egeCnt, ex.Name + " (" + ex.OlympSubject + ")");
                    acrFlds.SetField("OlympYear" + egeCnt, ex.DocumentDate.HasValue ? ex.DocumentDate.Value.Year.ToString() : "");
                    acrFlds.SetField("OlympDiplom" + egeCnt, (ex.DocumentSeries + " " ?? "") + (ex.DocumentNumber ?? ""));

                    if (egeCnt == 2)
                        break;
                    egeCnt++;
                }

                query = "SELECT WorkPlace, WorkProfession, Stage FROM PersonWork WHERE PersonId=@PersonId";
                tbl = Util.AbitDB.GetDataTable(query, new SortedList<string, object>() { { "@PersonId", PersonId } });
                var work =
                    (from DataRow rw in tbl.Rows
                     select new
                     {
                         WorkPlace = rw.Field<string>("WorkPlace"),
                         WorkProfession = rw.Field<string>("WorkProfession"),
                         Stage = rw.Field<string>("Stage")
                     }).FirstOrDefault();
                if (work != null)
                {
                    acrFlds.SetField("HasStag", "1");
                    acrFlds.SetField("WorkPlace", work.WorkPlace + ", " + work.WorkProfession);
                    acrFlds.SetField("Stag", work.Stage);
                }
                else
                    acrFlds.SetField("NoStag", "1");

                var Version = context.ApplicationCommitVersion.Where(x => x.CommitId == appId).Select(x => new { x.VersionDate, x.Id }).ToList().LastOrDefault();
                string sVersion = "";
                if (Version != null)
                    sVersion = "Версия №" + Version.Id + " от " + Version.VersionDate.ToString("dd.MM.yyyy HH:mm");
                string FIO = ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim();

                var Comm = context.ApplicationCommit.Where(x => x.Id == appId).FirstOrDefault();
                if (Comm != null)
                {
                    int multiplyer = 3;
                    string code = ((multiplyer * 100000) + Comm.IntNumber).ToString();

                    List<ShortAppcation> lstApps = abitList
                    .Select(x => new ShortAppcation()
                    {
                        ApplicationId = x.Id,
                        LicenseProgramName = x.ProfessionCode + " " + x.Profession,
                        ObrazProgramName = x.ObrazProgram,
                        ProfileName = x.Specialization,
                        Priority = x.Priority,
                        StudyBasisId = x.StudyBasisId,
                        StudyFormId = x.StudyFormId,
                        HasInnerPriorities = false,
                    }).ToList();

                    List<ShortAppcation> lstAppsFirst = new List<ShortAppcation>();
                    for (int u = 0; u < 3; u++)
                    {
                        if (lstApps.Count > u)
                            lstAppsFirst.Add(lstApps[u]);
                    }

                    //добавляем первый файл
                    lstFiles.Add(GetApplicationPDF_FirstPage(lstAppsFirst, lstApps, dirPath, "ApplicationSPO_page1.pdf", FIO, sVersion, code, false));
                    //acrFlds.SetField("Version", sVersion);

                    //остальные - по 4 на новую страницу
                    int appcount = 3;
                    while (appcount < lstApps.Count)
                    {
                        lstAppsFirst = new List<ShortAppcation>();
                        for (int u = 0; u < 4; u++)
                        {
                            if (lstApps.Count > appcount)
                                lstAppsFirst.Add(lstApps[appcount]);
                            else
                                break;
                            appcount++;
                        }

                        lstFiles.Add(GetApplicationPDF_NextPage(lstAppsFirst, lstApps, dirPath, "ApplicationSPO_page2.pdf", FIO));
                    }
                }

                pdfStm.FormFlattening = true;
                pdfStm.Close();
                pdfRd.Close();

                lstFiles.Add(ms.ToArray());

                return MergePdfFiles(lstFiles);
            }
        }
        public static byte[] MergePdfFiles(List<byte[]> lstFilesBinary)
        {
            MemoryStream ms = new MemoryStream();
            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            foreach (byte[] doc in lstFilesBinary)
            {
                PdfReader reader = new PdfReader(doc);
                int n = reader.NumberOfPages;

                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page;

                for (int i = 0; i < n; i++)
                {
                    document.NewPage();
                    page = writer.GetImportedPage(reader, i + 1);
                    cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                }
            }

            document.Close();
            return ms.ToArray();
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