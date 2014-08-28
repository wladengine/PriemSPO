﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.DirectoryServices.AccountManagement;

using BaseFormsLib;
using EducServLib;

namespace Priem
{    
    public static partial class MainClass
    {  
        //test
        public static Form mainform;
        private static DBPriem _bdc = null;

        public static PriemType dbType;
        public static string connString;
        public static string connStringOnline;        
        
        public static string directory;
        public static string dirTemplates;
        public static string saveTempFolder;
        public static string userName;
       
        public static int studyLevelGroupId;
        public static int countryRussiaId;
        public static int educSchoolId;
        public static int pasptypeRFId;
        public static int olympSpbguId;

        public const string PriemYear = "2014";

        public static QueryBuilder qBuilder;

        //пользовательские настройки
        public static ConfigFile _config;
        
        private static DataRefreshHandler _drHandler;
        private static ProtocolRefreshHandler _prHandler;
        
        public static DBPriem Bdc
        {
            get { return _bdc; }          
        }

        /// <summary>
        /// opens DataBase
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Init(Form mf)
        {
            try
            {
                _bdc = new DBPriem();
                _bdc.OpenDatabase(connString);

                mainform = mf;            
                userName = System.Environment.UserName;
                
                // database constant id
                using (PriemEntities context = new PriemEntities())
                {
                    //постоянный id страны Россия
                    countryRussiaId = 1;
                    //постоянный id типа уч.заведения Школа
                    educSchoolId = 1;
                    //постоянный id типа паспорта Паспорт РФ
                    pasptypeRFId = 1;
                    //постоянный id олимпиады СПбГУ
                    olympSpbguId = 3;
                }

                if(dbType == PriemType.Priem)
                    studyLevelGroupId = 1;
                else if (dbType == PriemType.PriemMag)
                    studyLevelGroupId = 2;
                else
                    studyLevelGroupId = 3;
              
                directory = string.Format(@"{0}\Priem", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));               
                saveTempFolder = string.Format(@"{0}\DocTempFiles",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                try
                {
                    // уточнить у Дениса, кто создавал эту папку!!!!! может будет ошибка
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // Determine whether the directory exists.
                    if (!Directory.Exists(saveTempFolder))
                        Directory.CreateDirectory(saveTempFolder);
                }
                catch (Exception e)
                {
                }      
                                
                //взяли конфиг
                _config = GetConfig();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        //прочитали конфиг
        private static ConfigFile GetConfig()
        {
            string configFile = "config.xml";
            return new ConfigFile(directory, configFile);
        }

        public static void DeleteTempFiles()
        {
            try
            {
                foreach (string file in Directory.GetFiles(saveTempFolder))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        public static string GetAbitNum(string abNum, string perNum)
        {
            return perNum + @"\"+ abNum;           
        }       

        public static string GetStringAbitNumber(string abitView)
        {
            return string.Format(" substring('000' + Convert(nvarchar(2), {0}.FacultyId), len('000' + Convert(nvarchar(2), {0}.FacultyId))-1, 2) + substring('000000' + Convert(nvarchar(5), {0}.RegNum), len('000000' + Convert(nvarchar(5), {0}.RegNum))-4, 5)", abitView);
        }

        /// <summary>
        /// Возвращает факт отсутствия человека в базе
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public static bool CheckPersonBarcode(int? barcode)
        {
            if (barcode == null)
                return true;

            using (PriemEntities context = new PriemEntities())
            {
                int cnt = (from pers in context.Person
                           where pers.Barcode == barcode
                           select pers).Count();

                if (cnt > 0)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Возвращает факт отсутствия заявления в базе
        /// </summary>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public static bool CheckAbitBarcode(int? barcode)
        {
            if (barcode == null)
                return true;

            using (PriemEntities context = new PriemEntities())
            {
                int cnt = (from abit in context.Abiturient
                           where abit.CommitNumber == barcode
                           select abit).Count();

                if (cnt > 0)
                    return false;
                else
                    return true;
            }
        }

        public static IEnumerable<qEntry> GetEntry(PriemEntities context)
        {
            try
            {
                IEnumerable<qEntry> entry = context.qEntry;
                return entry.Where(c => c.StudyLevelGroupId == 3);//только СПО, это приём только для СПО
            }
            catch (Exception exc)
            {
                WinFormsServ.Error("Ошибка qEntry " + exc.Message);
                return null;
            }      
        }

        public static string GetStLevelFilter(string tableName)
        {
            return string.Format(" AND {1}.StudyLevelGroupId = {0} ", studyLevelGroupId, tableName);
        }

        public static string GetADUserName(string userName)
        {
            try
            {
                var ADPrincipal = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ADPrincipal, userName);

                if (user != null)
                    return user.DisplayName + " (" + userName + ")";
            }
            catch { }

            return userName;
        }
    }
}
