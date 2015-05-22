using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Data.Entity.Core.Objects;
using WordOut;
using iTextSharp.text;
using iTextSharp.text.pdf;

using EducServLib;
using PriemLib;

namespace Priem
{
    public class Print
    {
        public static void PrintApplication(Guid abitId, bool forPrint, string savePath)
        {
            FileStream fileS = new FileStream(savePath, FileMode.CreateNew);
            try
            {
                using (BinaryWriter bw = new BinaryWriter(fileS))
                {
                    bw.Write(GetApplicationPDF_SPO(abitId, MainClass.dirTemplates));
                    bw.Flush();
                    bw.Close();
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
        
        public static byte[] GetApplicationPDF_SPO(Guid PersonId, string dirPath)
        {
            using (PriemEntities context = new PriemEntities())
            {
                var abitList = (from x in context.extAbit
                                join Entry in context.extEntry on x.EntryId equals Entry.Id
                                where x.PersonId == PersonId
                                && MainClass.lstStudyLevelGroupId.Contains(x.StudyLevelGroupId)
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
                                    Priority = x.Priority ?? 0,
                                    IsGosLine = Entry.IsForeign && Entry.StudyBasisId == 1,
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
                                  x.Person_AdditionalInfo.WorkPlace,
                                  x.Person_AdditionalInfo.Stag,
                                  x.Person_AdditionalInfo.ScienceWork
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

                acrFlds.SetField("HostelAbitYes", person.HostelAbit ? "1" : "0");
                acrFlds.SetField("HostelAbitNo", person.HostelAbit ? "0" : "1");

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
                var OlympVseros = context.Olympiads.Where(x => x.Abiturient.PersonId == PersonId && x.OlympType.IsVseross)
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
                var OlympNoVseros = context.Olympiads.Where(x => x.Abiturient.PersonId == PersonId && !x.OlympType.IsVseross)
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

                if (!string.IsNullOrEmpty(person.WorkPlace))
                {
                    acrFlds.SetField("HasStag", "1");
                    acrFlds.SetField("WorkPlace", person.WorkPlace);
                    acrFlds.SetField("Stag", person.Stag);
                }
                else
                    acrFlds.SetField("NoStag", "1");

                
                string FIO = ((person.Surname ?? "") + " " + (person.Name ?? "") + " " + (person.SecondName ?? "")).Trim();

                {
                    int multiplyer = 3;
                    string code = ((multiplyer * 100000) + abitList.Select(x => x.CommitIntNumber ?? 0).DefaultIfEmpty(0).First()).ToString();

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
                    lstFiles.Add(GetApplicationPDF_FirstPage(lstAppsFirst, lstApps, dirPath, "ApplicationSPO_page1.pdf", FIO, "", code, false));
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
        public static byte[] GetApplicationPDF_FirstPage(List<ShortAppcation> lst, List<ShortAppcation> lstFullSource, string dirPath, string dotName, string FIO, string Version, string code, bool isMag)
        {
            MemoryStream ms = new MemoryStream();

            byte[] templateBytes;
            using (FileStream fs = new FileStream(dirPath + dotName, FileMode.Open, FileAccess.Read))
            {
                templateBytes = new byte[fs.Length];
                fs.Read(templateBytes, 0, templateBytes.Length);
            }

            PdfReader pdfRd = new PdfReader(templateBytes);
            PdfStamper pdfStm = new PdfStamper(pdfRd, ms);
            //pdfStm.SetEncryption(PdfWriter.STRENGTH128BITS, "", "", PdfWriter.ALLOW_SCREENREADERS | PdfWriter.ALLOW_PRINTING | PdfWriter.AllowPrinting);

            //добавляем штрихкод
            Barcode128 barcode = new Barcode128();
            barcode.Code = code;
            PdfContentByte cb = pdfStm.GetOverContent(1);
            iTextSharp.text.Image img = barcode.CreateImageWithBarcode(cb, null, null);
            if (isMag)
                img.SetAbsolutePosition(420, 720);
            else
                img.SetAbsolutePosition(100, 750);
            cb.AddImage(img);

            AcroFields acrFlds = pdfStm.AcroFields;
            acrFlds.SetField("Version", Version);
            acrFlds.SetField("FIO", FIO);

            int rwind = 1;
            foreach (var p in lst.OrderBy(x => x.Priority))
            {
                acrFlds.SetField("Priority" + rwind, p.Priority.ToString());
                acrFlds.SetField("Profession" + rwind, p.LicenseProgramName);
                acrFlds.SetField("ObrazProgram" + rwind, p.ObrazProgramName);
                acrFlds.SetField("Specialization" + rwind, p.HasInnerPriorities ? "Приложение к заявлению № " + p.InnerPrioritiesNum : p.ProfileName);
                acrFlds.SetField("StudyForm" + p.StudyFormId.ToString() + rwind.ToString(), "1");
                acrFlds.SetField("StudyBasis" + p.StudyBasisId.ToString() + rwind.ToString(), "1");

                if (lstFullSource.Where(x => x.LicenseProgramName == p.LicenseProgramName && x.ObrazProgramName == p.ObrazProgramName && x.ProfileName == p.ProfileName && x.StudyFormId == p.StudyFormId).Count() > 1)
                    acrFlds.SetField("IsPriority" + rwind, "1");

                rwind++;
            }

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
        }
        public static byte[] GetApplicationPDF_NextPage(List<ShortAppcation> lst, List<ShortAppcation> lstFullSource, string dirPath, string dotName, string FIO)
        {
            MemoryStream ms = new MemoryStream();

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
            int rwind = 1;
            foreach (var p in lst.OrderBy(x => x.Priority))
            {
                if (p.StudyBasisId == 1)
                    acrFlds.SetField("Priority" + rwind, p.Priority.ToString());

                acrFlds.SetField("Profession" + rwind, p.LicenseProgramName);
                acrFlds.SetField("ObrazProgram" + rwind, p.ObrazProgramName);
                acrFlds.SetField("Specialization" + rwind, p.HasInnerPriorities ? "Приложение к заявлению № " + p.InnerPrioritiesNum : p.ProfileName);
                acrFlds.SetField("StudyForm" + p.StudyFormId.ToString() + rwind.ToString(), "1");
                acrFlds.SetField("StudyBasis" + p.StudyBasisId.ToString() + rwind.ToString(), "1");

                if (lstFullSource.Where(x => x.LicenseProgramName == p.LicenseProgramName && x.ObrazProgramName == p.ObrazProgramName && x.ProfileName == p.ProfileName && x.StudyFormId == p.StudyFormId).Count() > 1)
                    acrFlds.SetField("IsPriority" + rwind, "1");

                rwind++;
            }

            pdfStm.FormFlattening = true;
            pdfStm.Close();
            pdfRd.Close();

            return ms.ToArray();
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