using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using com.amtec.action;

namespace com.amtec.forms
{
    public partial class TopMostForm : Form
    {
        MainView mv = null;
        public TopMostForm(MainView mv1)
        {
            InitializeComponent();
            mv = mv1;
            textBox1.Text = mv.CaptionName; ;
        }

        public void UpdateData(string serialNumber, string message)
        {
            if (message != "")
            {
                this.BackColor = Color.Red;
                this.textBox2.Text = serialNumber + System.Environment.NewLine + message;
                mv.hasWarningMsg = true;
            }
            else
            {
                this.BackColor = Color.Gray;
                this.textBox2.Text = serialNumber + System.Environment.NewLine + message;
                mv.hasWarningMsg = false;
            }
        }

        private void TopMostForm_Load(object sender, EventArgs e)
        {
            new MouseMoveClass(this);
            this.Opacity = Convert.ToDouble(mv.config.OpacityValue) / 100;
            int x = Convert.ToInt32(mv.config.LocationXY.Split('|')[0]);
            int y = Convert.ToInt32(mv.config.LocationXY.Split('|')[1]);
            this.Location = new Point(x,y);
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            if (mv.WindowState == FormWindowState.Minimized)
            {
                mv.Show();
                mv.TopMost = true;
                //mv.WindowState = FormWindowState.Minimized;
                mv.WindowState = FormWindowState.Normal;
                mv.TopMost = false;
            }
            else if (mv.WindowState == FormWindowState.Normal)
            {
                mv.Show();
                mv.TopMost = true;
                //mv.WindowState = FormWindowState.Minimized;
                mv.WindowState = FormWindowState.Normal;
                mv.TopMost = false;
            }
        }
        private void TopMostForm_MouseHover(object sender, EventArgs e)
        {
            //mv.Opacity =90/100;
        }

        private void TopMostForm_MouseLeave(object sender, EventArgs e)
        {
            //this.Opacity = Convert.ToDouble(ConfigItem.opacityValue) / 100;
        }
    }
}
