using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace com.amtec.action
{
    class MouseMoveClass2
    {
        #region 全局变量
        private int mouseX = 0;
        private int mouseY = 0;
        private Form login;
        public Timer timer_shake;
        int times = 0;
        const int AW_HOR_POSITIVE = 0x0001;
        const int AW_HOR_NEGATIVE = 0x0002;
        const int AW_VER_POSITIVE = 0X0004;
        const int AW_VER_NEGATIVE = 0x0008;
        const int AW_CENTER = 0x0010;
        const int AW_HIDE = 0x10000;
        const int AW_ACTIVATE = 0x20000;
        const int AW_SLIDE = 0x40000;
        const int AW_BLEND = 0x80000;
        #endregion

        [DllImport("user32")]
        private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        private static extern int GetWindowLong(HandleRef hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// 登陆窗体构造函数
        /// </summary>
        /// <param name="form">需要进行处理的窗体</param>
        public MouseMoveClass2(Form form,PictureBox pic)
        {
            login = form;
            login.Load += new EventHandler(login_Load);
            //login.FormClosing += new FormClosingEventHandler(Login_FormClosing);
            pic.MouseDown += new MouseEventHandler(Login_MouseDown);
            pic.MouseMove += new MouseEventHandler(Login_MouseMove);
            //login.BackColor = Color.Blue;
            //login.TransparencyKey = Color.Blue;
            //login.FormBorderStyle = FormBorderStyle.None;

            //timer_shake = new Timer();
            //timer_shake.Interval = 100;
            //timer_shake.Tick += new EventHandler(timer_shake_Tick);
        }

        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
        }

        private void Login_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                login.Location = new Point(Control.MousePosition.X - mouseX, Control.MousePosition.Y - mouseY);
            }
        }

        private void login_Load(object sender, EventArgs e)
        {
            AnimateWindow(login.Handle, 550, AW_CENTER | AW_ACTIVATE);
            int WS_SYSMENU = 0x00080000; int windowLong = (GetWindowLong(new HandleRef(login, login.Handle), -16));
            SetWindowLong(new HandleRef(login, login.Handle), -16, windowLong | WS_SYSMENU);
            SetWindowLong(new HandleRef(login, login.Handle), -16, WS_SYSMENU | WS_SYSMENU);//屏蔽最大化
            // SetWindowLong(new HandleRef(this, this.Handle), -16, windowLong | WS_SYSMENU); //屏蔽最小化
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            AnimateWindow(login.Handle, 1000, AW_SLIDE | AW_HIDE | AW_VER_NEGATIVE);
            Application.Exit();
        }

        private void timer_shake_Tick(object sender, EventArgs e)
        {
            times++;
            if (times < 8)
            {
                for (int i = 0; i < 10; i++)
                {
                    login.Location = new Point(login.Location.X + 1, login.Location.Y);
                }
                for (int i = 0; i < 10; i++)
                {
                    login.Location = new Point(login.Location.X - 1, login.Location.Y);
                }
            }
            else
            {
                times = 0;
                timer_shake.Stop();
            }
        }
    }
}
