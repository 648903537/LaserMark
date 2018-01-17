using com.amtec.action;
using com.amtec.forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LaserMark.com.amtec.forms
{
    public partial class Reprint : Form
    {
        MainView view;
        string[] serialnumbers;
        public List<string> ListWreprintSN;
        public Reprint(string[] _serialnumbers, MainView _view)
        {
            InitializeComponent();
            view = _view;
            serialnumbers = _serialnumbers;
        }

        private void btnComfirmPrint_Click(object sender, EventArgs e)
        {
            ListWreprintSN = new List<string>();
            if (this.dgvRePrintSN.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in this.dgvRePrintSN.Rows)
                {
                    if (row.Selected)
                    {
                        if (view.config.PRINTER_MODE == "2")
                        ListWreprintSN.Add(row.Cells[1].Value.ToString());
                        else if (view.config.PRINTER_MODE == "3")
                        {
                            ListWreprintSN.Add("[" +view.txbCDAMONumber.Text + ";" + row.Cells[1].Value.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;0;1;1]");
                        }
                    }
                }
                //DataGridViewRow row = this.dgvRePrintSN.SelectedRows[0];
                //string equipmentNo = row.Cells["EquipNo"].Value.ToString();
            }
            this.Hide();
        }

        private void Reprint_Load(object sender, EventArgs e)
        {
            view.InitCintrolLanguage(this);
            try
            {
                if (view.rdbRefSerialnumber.Checked)
                {
                    for (int i = 0; i < serialnumbers.Length; i += 4)
                    {
                        if (CheckRefSNExistIngrid(serialnumbers[i].Substring(0, serialnumbers[i].Length - 3)))
                        {
                            view.list.Add(serialnumbers[i].Substring(0, serialnumbers[i].Length - 3));
                            AddDataToPCBGrid(new object[2] { this.dgvRePrintSN.RowCount + 1, serialnumbers[i].Substring(0, serialnumbers[i].Length - 3) });
                        }
                      
                    }
                }
                if (view.rdbSerialnumber.Checked)
                {
                    for (int i = 0; i < serialnumbers.Length; i += 2)
                    {
                        view.list.Add(serialnumbers[i]);
                        AddDataToPCBGrid(new object[2] { this.dgvRePrintSN.RowCount + 1, serialnumbers[i] });
                    }
                }
                if (view.rdbRefandSN.Checked)
                {
                    for (int i = 0; i < serialnumbers.Length; i += 2)
                    {
                        if (CheckRefSNExistIngrid(serialnumbers[i].Substring(0, serialnumbers[i].Length - 3)))
                        {
                            view.list.Add(serialnumbers[i].Substring(0, serialnumbers[i].Length - 3));
                            AddDataToPCBGrid(new object[2] { this.dgvRePrintSN.RowCount + 1, serialnumbers[i].Substring(0, serialnumbers[i].Length - 3) });
                        }
                        view.list.Add(serialnumbers[i]);
                        AddDataToPCBGrid(new object[2] { this.dgvRePrintSN.RowCount + 1, serialnumbers[i] });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }
        private bool CheckRefSNExistIngrid(string Refsn)
        {
            bool isValid = true;
            foreach (DataGridViewRow row in this.dgvRePrintSN.Rows)
            {
                string sn = row.Cells[1].Value.ToString();
                if (sn == Refsn)
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }
        private delegate void AddDataToPCBGridHandle(object[] values);
        private void AddDataToPCBGrid(object[] values)
        {
            try
            {
                if (this.dgvRePrintSN.InvokeRequired)
                {
                    AddDataToPCBGridHandle addDataDel = new AddDataToPCBGridHandle(AddDataToPCBGrid);
                    Invoke(addDataDel, new object[] { values });
                }
                else
                {
                    this.dgvRePrintSN.Rows.Insert(0, values);
                    this.dgvRePrintSN.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                List<string> listresult = (view.list.Where(p => p.Contains(textBox1.Text))).ToList<string>();
                if (listresult.Count >0 && listresult != null)
                {
                    dgvRePrintSN.Rows.Clear();
                    foreach (var sn in listresult)
                    {
                        AddDataToPCBGrid(new object[2] { this.dgvRePrintSN.RowCount + 1, sn });
                    }
                }
            }
        }
    }
}
