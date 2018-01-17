using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ShippingClient.com.amtec.forms
{
    public partial class ProcessbarExt : UserControl
    {
        public ProcessbarExt()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = this.progressBar1.ClientRectangle;
           
           
            // 标签背景填充颜色，也可以是图片 
            SolidBrush bru = new SolidBrush(Color.FromArgb(152, 171, 207));
            SolidBrush bruFont = new SolidBrush(Color.Black);// 标签字体颜色 
          
               
             
            base.OnPaint(e);
        }
    }
}
