using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data.Entity.Core.Objects;
using System.Data;
using System.Transactions;

using BDClassLib;
using EducServLib;
using PriemLib;

namespace Priem
{
    public static class SomeMethodsClass
    {
        // проверка на 3 направления
        public static bool CheckThreeAbits(PriemEntities context, Guid? personId, int? LicenseProgramId, int? ObrazProgramId, int ProfileId)
        {
            // если прием - то проверяем на три заявления
            if (MainClass.dbType != PriemType.Priem)
                return true;

            //просто сосчитаем количество созданных конкурсов на человека
            var concurses = (from allab in context.qAbitAll
                             where allab.PersonId == personId
                                 //&& allab.LicenseProgramId != LicenseProgramId
                                 //&& allab.ObrazProgramId != ObrazProgramId
                                 //&& (ProfileId == null ? allab.ProfileId != null : allab.ProfileId != ProfileId)
                             && MainClass.lstStudyLevelGroupId.Contains(allab.StudyLevelGroupId)
                             && allab.BackDoc != true
                             select new { allab.LicenseProgramId, allab.ObrazProgramId, allab.ProfileId }).Distinct();
            int cntExist = concurses.Count();

            // теперь проверка на три заявления на образ программу!      
            //если конкурсов три (более трёх) - не давать создавать заявление
            if (cntExist >= 3)
            {
                int cntJourn = concurses.Where(x => x.LicenseProgramId == 464 && x.ObrazProgramId == 39 && x.ProfileId == 0).Count();

                if (cntJourn > 1)
                    if (cntExist == 3)
                        return true;

                //если подача на уже созданный конкурс
                int iLP = LicenseProgramId ?? 0;
                int iOP = ObrazProgramId ?? 0;

                int cnt;

                if (LicenseProgramId == 464 && ObrazProgramId == 39 && ProfileId == 0)
                    cnt = concurses.Where(x => x.LicenseProgramId == iLP && x.ObrazProgramId == iOP && x.ProfileId == 0).Count();
                else
                    cnt = concurses.Where(x => x.LicenseProgramId == iLP && x.ObrazProgramId == iOP && x.ProfileId == ProfileId).Count();

                if (cnt > 0)
                    return true;
                return false;
            }
            else
                return true;
        }

        // проверка на уникальность абитуриента
        private static bool CheckIdent(extPerson person, string AttestatSeries, string AttestatNum)
        {
            using (PriemEntities context = new PriemEntities())
            {
                ObjectParameter boolPar = new ObjectParameter("result", typeof(bool));

                context.CheckPersonIdent(person.Surname, person.Name, person.SecondName, person.BirthDate, person.PassportSeries, person.PassportNumber,
                    AttestatSeries, AttestatNum, boolPar);

                return Convert.ToBoolean(boolPar.Value);
            }
        }
    }
}
