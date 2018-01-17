using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace com.amtec.forms
{
    public partial class ErrorForm : Form
    {
       public  MainView view = null;
        public ErrorForm(string errorMsg,string station)
        {
            InitializeComponent();
            string strVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string strCaption = string.Format("Laser Mark (v{0})-{1}",strVersion,station);
            this.lblTitle.Text = strCaption;
            this.txtErrorMsg.Text = errorMsg;
        }

        public ErrorForm(string errorMsg, string station,MainView mainView)
        {
            InitializeComponent();
            string strVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string strCaption = string.Format("Laser Mark  (v{0})-{1}", strVersion, station);
            this.lblTitle.Text = strCaption;
            this.txtErrorMsg.Text = errorMsg;
            view = mainView;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (view != null && !view.IsDisposed)
            {
                view.Dispose();
                view.Close();
            }
            else
            {
                this.Close();
            }
        }
    }
}
