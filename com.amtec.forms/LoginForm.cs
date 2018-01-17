using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using com.amtec.configurations;
using com.itac.mes.imsapi.domain.container;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.oem.common.container.imsapi.utils;
using com.amtec.forms;
using System.Threading;
using com.amtec.action;
using com.amtec.model;
using System.IO.Ports;
using System.Text.RegularExpressions;

namespace com.amtec.forms
{
    public partial class LoginForm : Form
    {
        private ApplicationConfiguration config;
        private IMSApiSessionValidationStruct sessionValidationStruct;
        public IMSApiSessionContextStruct sessionContext = null;
        private SessionContextHeandler sessionContextHandler;
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private LanguageResources res;
        public string UserName = "";
        public int LoginResult = 0;
        public bool isCanLogin = false;
        int logintype = 0; //0.init log in 1.Authorized_Seria_Number_Transfer
        MainView form;
        string RemoveWO = "";

        public LoginForm(int _logintype, MainView _form, string WO)
        {
            InitializeComponent();
            logintype = _logintype;
            form = _form;
            RemoveWO = WO;
            this.progressBar1.Value = 0;
            this.progressBar1.Maximum = 100;
            this.progressBar1.Step = 1;

            this.timer1.Interval = 100;
            this.timer1.Tick += new EventHandler(timer_Tick);

            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(worker_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (logintype == 0)
                System.Environment.Exit(0);
            else
                this.Hide();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!VerifyLoginInfo())
                return;
            LogHelper.Info("Login start...");
            backgroundWorker1.RunWorkerAsync();
            this.lblErrorMsg.Text = "Loading application....";
            this.timer1.Start();
            SetControlStatus(false);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (this.progressBar1.Value < this.progressBar1.Maximum - 5)
            {
                this.progressBar1.Value++;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            timer1.Stop();
            this.progressBar1.Value = this.progressBar1.Maximum;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //添加你初始化的代码
            res = new LanguageResources();
            CommonModel commonModel = ReadIhasFileData.getInstance();
            if (!isCanLogin)
            {
                if (logintype == 0)
                    sessionContextHandler = new SessionContextHeandler(null, this);
            }
            sessionValidationStruct = new IMSApiSessionValidationStruct();
            sessionValidationStruct.stationNumber = commonModel.Station;
            sessionValidationStruct.stationPassword = "";
            sessionValidationStruct.user = this.txtUserName.Text.Trim();
            sessionValidationStruct.password = this.txtPassword.Text.Trim();
            sessionValidationStruct.client = commonModel.Client;
            sessionValidationStruct.registrationType = commonModel.RegisterType;
            sessionValidationStruct.systemIdentifier = commonModel.Station;
            UserName = this.txtUserName.Text.Trim();

            LoginResult = imsapi.regLogin(sessionValidationStruct, out sessionContext);
            if (LoginResult == 0)
                LogHelper.Info("api regLogin.(error code=" + LoginResult + ")");
            else
                LogHelper.Error("api regLogin.(error code=" + LoginResult + ")");
            LogHelper.Info("Login end...");
            if (LoginResult != IMSApiDotNetConstants.RES_OK)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    SetStatusLabelText("api regLogin error.(error code=" + LoginResult + ")", 1);
                    SetControlStatus(true);
                }));
                return;
            }
            else
            {
                if (logintype == 0)
                {
                    //add by qy 160614
                    if (!VerifyUserTeam())
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            SetControlStatus(true);
                        }));
                        return;
                    }

                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.Hide();
                        if (config.LogInType == "COM" && serialPort.IsOpen)
                            serialPort.Close();
                        MainView view = new MainView(this.txtUserName.Text.Trim(), DateTime.Now, sessionContext);
                        view.ShowDialog();
                    }));
                }
                else
                {
                    if (!VerifyUserTeam())
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            SetControlStatus(true);
                        }));
                        return;
                    }
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.Hide();
                        if (logintype == 4) //班长确认
                        {
                            form.SupervisorConfirm(this.txtUserName.Text.Trim());
                        }
                        else if (logintype == 5) //IPQC确认
                        {
                            form.IPQCConfirm(this.txtUserName.Text.Trim());
                        }
                        if (config.LogInType == "COM" && serialPort.IsOpen)
                        {
                            serialPort.Close();
                            form.OpenScanPort();
                        }

                    }));
                }
            }
        }

        public delegate void SetStatusLabelTextDel(string strText, int iCase);
        public void SetStatusLabelText(string strText, int iCase)
        {
            if (this.lblErrorMsg.InvokeRequired)
            {
                SetStatusLabelTextDel setText = new SetStatusLabelTextDel(SetStatusLabelText);
                Invoke(setText, new object[] { strText, iCase });
            }
            else
            {
                this.lblErrorMsg.Text = strText;
                if (iCase == 0)
                {
                    this.lblErrorMsg.ForeColor = Color.Black;
                }
                else if (iCase == 1)
                {
                    this.lblErrorMsg.ForeColor = Color.Red;
                }
            }
        }

        private void SetControlStatus(bool isOK)
        {
            this.btnOK.Enabled = isOK;
            this.txtPassword.Enabled = isOK;
            this.txtUserName.Enabled = isOK;
        }

        #region add by liuxue for scan user & psw when login
        private SerialPort serialPort;
        private void LoginForm_Load(object sender, EventArgs e)
        {
            config = new ApplicationConfiguration();
            if (config.LogInType == "COM")
            {
                this.txtPassword.ReadOnly = true;
                this.txtUserName.ReadOnly = true;
                InitSerialPort();
            }
            else
            {
                this.txtPassword.ReadOnly = false;
                this.txtUserName.ReadOnly = false;
            }
        }

        private void InitSerialPort()
        {
            serialPort = new SerialPort();
            serialPort.PortName = config.SerialPort;
            serialPort.BaudRate = int.Parse(config.BaudRate);
            serialPort.Parity = (Parity)int.Parse(config.Parity);
            serialPort.StopBits = (StopBits)1;
            serialPort.Handshake = Handshake.None;
            serialPort.DataBits = int.Parse(config.DataBits);
            serialPort.NewLine = "\r";

            serialPort.DataReceived += new SerialDataReceivedEventHandler(DataRecivedHeandler);
            serialPort.Open();
        }

        public void DataRecivedHeandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                Thread.Sleep(200);
                Byte[] bt = new Byte[sp.BytesToRead];
                sp.Read(bt, 0, sp.BytesToRead);
                string indata = System.Text.Encoding.ASCII.GetString(bt).Trim();
                Match match = Regex.Match(indata, config.LoadExtractPattern);
                if (match.Success)
                {
                    SetUserControlText(match.Groups[1].ToString());
                    SetPasswordControlText(match.Groups[2].ToString());
                    this.Invoke(new MethodInvoker(delegate
                    {
                        btnOK_Click(null, null);
                    }));
                }
                else
                {
                    SetStatusLabelText("条码错误", 1);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public void SetUserControlText(string strText)
        {
            this.InvokeEx(x => this.txtUserName.Text = strText);
        }

        public void SetPasswordControlText(string strText)
        {
            this.InvokeEx(x => this.txtPassword.Text = strText);
        }
        #endregion

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOK_Click(null, null);

            }
        }

        private bool VerifyLoginInfo()
        {
            bool isValidate = true;
            if (string.IsNullOrEmpty(this.txtUserName.Text.Trim()) || string.IsNullOrEmpty(this.txtPassword.Text.Trim()))
            {
                SetStatusLabelText("Pls input user name/password.", 1);
                isValidate = false;
            }

            return isValidate;
        }

        private bool VerifyUserTeam()
        {
            bool isValid = true;
            string Supervisor = "";
            string Supervisor_OPTION = "1";
            string IPQC = "";
            string IPQC_OPTION = "1";
            if (config.AUTH_CHECKLIST_APP_TEAM != "" && config.AUTH_CHECKLIST_APP_TEAM != null)
            {
                string[] teams = config.AUTH_CHECKLIST_APP_TEAM.Split(';');
                string[] items = teams[0].Split(',');
                Supervisor = items[0];
                Supervisor_OPTION = items[1];
                string[] IPQCitems = teams[1].Split(',');
                IPQC = IPQCitems[0];
                IPQC_OPTION = IPQCitems[1];
            }
            if (logintype == 0)
            {
                if (config.UserTeam != "" && config.UserTeam != null)
                {
                    GetUserData getUser = new GetUserData(sessionContext);
                    string[] mdataGetUserDataValues = getUser.mdataGetUserData(this.txtUserName.Text.Trim(), this.txtPassword.Text.Trim(), config.StationNumber);
                    if (mdataGetUserDataValues != null && mdataGetUserDataValues.Length > 0)
                    {
                        string teamnumber = mdataGetUserDataValues[2];
                        if (!config.UserTeam.Contains(teamnumber))
                        {
                            SetStatusLabelText("User Team not authorized", 1);
                            isValid = false;
                        }
                    }
                    else
                    {
                        SetStatusLabelText("User Team not authorized", 1);
                        isValid = false;
                    }
                }
            }
            else if (logintype == 4)
            {
                if (Supervisor != "" && Supervisor_OPTION == "0")
                {
                    GetUserData getUser = new GetUserData(sessionContext);
                    string[] mdataGetUserDataValues = getUser.mdataGetUserData(this.txtUserName.Text.Trim(), this.txtPassword.Text.Trim(), config.StationNumber);
                    if (mdataGetUserDataValues != null && mdataGetUserDataValues.Length > 0)
                    {
                        string teamnumber = mdataGetUserDataValues[2];
                        if (!Supervisor.Contains(teamnumber))
                        {
                            SetStatusLabelText("User Team not authorized", 1);
                            isValid = false;
                        }
                    }
                    else
                    {
                        SetStatusLabelText("User Team not authorized", 1);
                        isValid = false;
                    }
                }
            }
            else if (logintype == 5)
            {
                if (IPQC != "" && IPQC_OPTION == "0")
                {
                    GetUserData getUser = new GetUserData(sessionContext);
                    string[] mdataGetUserDataValues = getUser.mdataGetUserData(this.txtUserName.Text.Trim(), this.txtPassword.Text.Trim(), config.StationNumber);
                    if (mdataGetUserDataValues != null && mdataGetUserDataValues.Length > 0)
                    {
                        string teamnumber = mdataGetUserDataValues[2];
                        if (!IPQC.Contains(teamnumber))
                        {
                            SetStatusLabelText("User Team not authorized", 1);
                            isValid = false;
                        }
                    }
                    else
                    {
                        SetStatusLabelText("User Team not authorized", 1);
                        isValid = false;
                    }
                }
            }
            return isValid;
        }
    }
}
