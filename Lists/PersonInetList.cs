using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using EducServLib;
using BDClassLib;
using BaseFormsLib;
using PriemLib;

namespace Priem
{
    public partial class PersonInetList : BookList
    {
        private DBPriem bdcInet;
        private LoadFromInet loadClass;

        //конструктор
        public PersonInetList()
        {            
            InitializeComponent();

            Dgv = dgvAbiturients;
            _tableName = "ed.Person";
            _title = "Список абитуриентов СПбГУ";           
     
            InitControls();            
        }        
        
        //дополнительная инициализация контролов
        protected override void  ExtraInit()
        {
            base.ExtraInit();

            if (MainClass.RightsJustView())
            {
                btnLoad.Enabled = false;
                btnAdd.Enabled = false;
            }
           
            if (MainClass.dbType == PriemType.PriemMag)
                tbPersonNum.Visible = lblBarcode.Visible = btnLoad.Visible = false;

            //Dgv.CellDoubleClick -= new System.Windows.Forms.DataGridViewCellEventHandler(Dgv_CellDoubleClick);
        }

        //поле поиска
        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            WinFormsServ.Search(this.dgvAbiturients, "FIO", tbSearch.Text);
        }

        protected override void OpenCard(string id, BaseFormEx formOwner, int? index)
        {
            MainClass.OpenCardPerson(id, formOwner, index);
        }

        protected override void GetSource()
        {
            _sQuery = "SELECT DISTINCT extPersonSPO.Id, extPersonSPO.PersonNum, extPersonSPO.FIO, extPersonSPO.PassportData, extPersonSPO.EducDocument FROM ed.extPersonSPO ";
            string join = "";
            if (chbShowAll.Checked)
            {
                _sQuery += " UNION SELECT DISTINCT extPerson.Id, extPerson.PersonNum, extPerson.FIO, extPerson.PassportData, extPerson.EducDocument FROM ed.extPerson ";
            }
            HelpClass.FillDataGrid(Dgv, _bdc, _sQuery + join, "", " ORDER BY FIO");
            SetVisibleColumnsAndNameColumns();    
        }

        protected override void SetVisibleColumnsAndNameColumns()
        {
            Dgv.AutoGenerateColumns = false;

            foreach (DataGridViewColumn col in Dgv.Columns)
            {
                col.Visible = false;
            }
            
            this.Width = 608;
            dgvAbiturients.Columns["PersonNum"].Width = 70;
            dgvAbiturients.Columns["FIO"].Width = 246;

            SetVisibleColumnsAndNameColumnsOrdered("PersonNum", "Ид_номер", 0);
            SetVisibleColumnsAndNameColumnsOrdered("FIO", "ФИО", 1);
            SetVisibleColumnsAndNameColumnsOrdered("PassportData", "Паспортные данные", 2);
            SetVisibleColumnsAndNameColumnsOrdered("EducDocument", "Документ об образовании", 3);          
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            loadClass = new LoadFromInet();
            bdcInet = loadClass.BDCInet;

            int fileNum = 0;

            string barcText = tbPersonNum.Text.Trim();

            if (barcText == string.Empty)
            {
                WinFormsServ.Error("Не введен номер");
                return;
            }
            /*
            if (barcText.Length == 7)
            {
                if (barcText.StartsWith("2"))
                {
                    WinFormsServ.Error("Выбран человек, подавший заявления в магистратуру");
                    return;
                }

                barcText = barcText.Substring(1);
            }
            */
             if (barcText.Length == 6)
            {
                if (barcText.StartsWith("3"))
                {
                    barcText = barcText.Substring(1);
                }
            }

            if (!int.TryParse(barcText, out fileNum))
            {
                WinFormsServ.Error("Неправильно введен номер");
                return;
            }

            if (MainClass.CheckAbitBarcode(fileNum))
            {
                try
                {
                    bool hasDeleted = loadClass.GetHasDeleted(fileNum);

                    if (hasDeleted)
                        return;

                    DataTable dtEge = new DataTable();
                                        
                    //if(person != null)
                    //{
                    //    string queryEge = "SELECT EgeMark.Id, EgeMark.EgeExamNameId AS ExamId, EgeMark.Value, EgeCertificate.PrintNumber, EgeCertificate.Number, EgeMark.EgeCertificateId FROM EgeMark LEFT JOIN EgeCertificate ON EgeMark.EgeCertificateId = EgeCertificate.Id LEFT JOIN Person ON EgeCertificate.PersonId = Person.Id";
                    //    DataSet dsEge = bdcInet.GetDataSet(queryEge + " WHERE Person.Barcode = " + fileNum + " ORDER BY EgeMark.EgeCertificateId ");
                    //    dtEge = dsEge.Tables[0];
                    //}

                    CardFromInet crd = new CardFromInet(null, fileNum, false);
                    crd.ToUpdateList += new UpdateListHandler(UpdateDataGrid);
                    crd.Show();
                }
                catch (Exception exc)
                {
                    WinFormsServ.Error(exc.Message);
                    tbPersonNum.Text = "";
                    tbPersonNum.Focus();
                }
            }
            else
            {
                UpdateDataGrid();
                using (PriemEntities context = new PriemEntities())
                {
                    extPerson person = (from per in context.extPerson
                                           join ab in context.Abiturient on per.Id equals ab.PersonId
                                        where ab.CommitNumber == fileNum
                                        select per).FirstOrDefault();

                    string fio = person.FIO;
                    string num = person.PersonNum;
                    string persId = person.Id.ToString();

                    WinFormsServ.Search(this.dgvAbiturients, "PersonNum", num);
                    DialogResult dr = MessageBox.Show(string.Format("Абитуриент {0} с данным номером баркода уже импортирован в базу.\nОткрыть карточку абитуриента?", fio), "Внимание", MessageBoxButtons.YesNo);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                        MainClass.OpenCardPerson(persId, this, null);
                }
            }

            tbPersonNum.Text = "";
            tbPersonNum.Focus();
            loadClass.CloseDB();  
        }

        private void PersonList_Load(object sender, EventArgs e)
        {
            tbPersonNum.Focus();
        }

        private void PersonList_Activated(object sender, EventArgs e)
        {
            tbPersonNum.Focus();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateDataGrid();            
        }

        private void tbNumber_TextChanged(object sender, EventArgs e)
        {
            WinFormsServ.Search(this.dgvAbiturients, "PersonNum", tbNumber.Text);
        }

        private void chbShowAll_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        protected override void DeleteSelectedRows(string sId)
        {
            Guid g = Guid.Empty;
            Guid.TryParse(sId, out g);
            using (PriemEntities context = new PriemEntities())
            {
                context.Person_deleteAllInfo(g);
            }
        }

    }
}