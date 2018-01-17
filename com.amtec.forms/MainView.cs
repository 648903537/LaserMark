using com.amtec.action;
using com.amtec.configurations;
using com.amtec.device;
using com.amtec.model;
using com.itac.mes.imsapi.domain.container;
using com.itac.oem.common.container.imsapi.utils;
using Compal.Onlineprg.Printing.Port;
using LaserMark.com.amtec.forms;
using ScreenPrinter.com.amtec.forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;

namespace com.amtec.forms
{
    public partial class MainView : Form
    {
        public ApplicationConfiguration config;
        IMSApiSessionContextStruct sessionContext;
        public bool isScanProcessEnabled = false;
        private InitModel initModel;
        private LanguageResources res;
        public string UserName = "";
        private string indata = "";
        List<SerialNumberData> serialNumberArray = new List<SerialNumberData>();
        public delegate void HandleInterfaceUpdateTopMostDelegate(string sn, string message);
        public HandleInterfaceUpdateTopMostDelegate topmostHandle;
        public TopMostForm topmostform = null;
        public bool hasWarningMsg = false;
        CommonModel commonModel = null;
        public string CaptionName = "";
        private SocketClientHandler cSocket = null;
        private SocketClientHandler2 cSocket2 = null;
        private System.Timers.Timer CheckConnectTimer = new System.Timers.Timer();
        private System.Timers.Timer RestoreMaterialTimer = null;
        string Supervisor_OPTION = "1";
        string IPQC_OPTION = "1";
        private SocketClientHandler2 checklist_cSocket = null;
        bool isStartLineCheck = true;//开线点检已经获取=true. 过程点检=false

        #region Init
        public MainView(string userName, DateTime dTime, IMSApiSessionContextStruct _sessionContext)
        {
            //初始化控件
            InitializeComponent();
            //ITAC存储用户信息的IMSApiSessionContextStruct类
            sessionContext = _sessionContext;
            //用户名
            UserName = userName;
            //获取ihas.properties文件
            commonModel = ReadIhasFileData.getInstance();
            this.lblLoginTime.Text = dTime.ToString("yyyy/MM/dd HH:mm:ss");
            this.lblUser.Text = userName == "" ? commonModel.Station : userName;
            this.lblStationNO.Text = commonModel.Station;
        }

        private void MainView_Shown(object sender, EventArgs e)
        {
            //BackgroundWorker控件触发(后台)
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorkerInit);
            bgWorker.RunWorkerAsync();
        }
        bool isOK = true;
        private void bgWorkerInit(object sender, DoWorkEventArgs args)
        {
            errorHandler(0, "Application start...", "");
            errorHandler(0, "Version :" + Assembly.GetExecutingAssembly().GetName().Version.ToString(), "");
            //中,繁,英对照
            res = new LanguageResources();
            //配置文件
            config = new ApplicationConfiguration(sessionContext, this);
            InitializeMainGUI init = new InitializeMainGUI(sessionContext, config, this, res);
            initModel = init.Initialize();
            this.InvokeEx(x =>
            {
                try
                {
                    //this.tabDocument.Parent = null;

                    this.Text = res.MAIN_TITLE + " (" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ")";
                    CaptionName = res.MAIN_TITLE + System.Environment.NewLine + config.StationNumber;
                    //显示小提示窗口
                    ShowTopWindow();
                    #region add by qy
                    //语言选择
                    SystemVariable.CurrentLangaugeCode = config.Language;
                    InitCintrolLanguage(this);
                    #endregion
                    InitWorkOrderList();//获取工单类表
                    //InitTaskData();
                    //读取TXT的确认结果文档
                    InitCheckResultMapping();
                    InitDocumentGrid();//获取指导手册MDA
                    if (config.AUTH_CHECKLIST_APP_TEAM != "" && config.AUTH_CHECKLIST_APP_TEAM != null)
                    {
                        string[] teams = config.AUTH_CHECKLIST_APP_TEAM.Split(';');
                        string[] items = teams[0].Split(',');
                        string Super = items[0];
                        Supervisor_OPTION = items[1];
                        string[] IPQCitems = teams[1].Split(',');
                        string IP = IPQCitems[0];
                        IPQC_OPTION = IPQCitems[1];
                    }
                    if (config.RESTORE_TIME != "" && config.RESTORE_TREAD_TIMER != "")
                    {
                        //点检页面的循环记录
                        GetRestoreTimerStart();
                    }
                    if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")//20161208 edit by qy
                    {
                        InitShift2(txbCDAMONumber.Text);
                        InitWorkOrderType();
                        this.tabCheckList.Parent = null;
                        checklist_cSocket = new SocketClientHandler2(this);
                        isOK = checklist_cSocket.connect(config.CHECKLIST_IPAddress, config.CHECKLIST_Port);
                        if (isOK)
                        {
                            if (!CheckShiftChange2())
                            {
                                InitTaskData_SOCKET("开线点检;设备点检");
                                isStartLineCheck = true;
                            }
                            else
                            {
                                if (!ReadCheckListFile())//20161214 edit by qy
                                {
                                    InitTaskData_SOCKET("开线点检");
                                    isStartLineCheck = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        InitTaskData();
                        //隐藏tabCheckListTable
                        this.tabCheckListTable.Parent = null;
                    }
                    //打印模式,现在只有1这个镭雕机打印方式还有存留
                    if (config.PRINTER_MODE == "1")
                    {
                        //镭雕打印
                        this.tabPrintLabel.Parent = null;
                        cSocket = new SocketClientHandler(this);
                        //连接PFC
                        isOK = cSocket.connect(config.IPAdress, config.Port);
                        if (isOK)
                        {
                            //创建发送PING
                            GetTimerStart();
                        }
                    }
                    else if (config.PRINTER_MODE == "2")
                    {
                        this.ckbRefSmall.Visible = false;
                        this.ckbSmall.Visible = false;
                        this.rdbSerialnumber.Visible = false;
                        this.rdbRefandSN.Visible = false;
                        rdbRefSerialnumber.Checked = true;
                        rdbNoSPanel.Checked = true;
                        this.tabConnection.Parent = null;
                        InitPrinterList();
                        InitPrintLabelTemplate();
                    }
                    else if (config.PRINTER_MODE == "3")
                    {
                        this.ckbRefSmall.Visible = false;
                        this.ckbSmall.Visible = false;
                        this.rdbSerialnumber.Visible = false;
                        this.rdbRefandSN.Visible = false;
                        rdbRefSerialnumber.Checked = true;
                        rdbNoSPanel.Checked = true;
                        label11.Visible = false;
                        combLabel.Visible = false;
                        label13.Visible = false;
                        cbxPrinter.Visible = false;

                        this.label14.Location = new Point(6, 55);
                        this.txtPrintCopies.Location = new Point(81, 52);
                        btnStart.Location = new Point(7, 85);
                        btnPulse.Location = new Point(7, 119);
                        btnStop.Location = new Point(7, 153);
                        label12.Location = new Point(9, 192);
                        groupBox2.Location = new Point(8, 227);
                        btnStop.Visible = false;
                        groupBox2.Visible = false;
                        btnRePrint.Visible = false;
                        label12.Visible = false;

                        cSocket2 = new SocketClientHandler2(this);
                        bool isOK = cSocket2.connect(config.IPAdress, config.Port);
                        if (!isOK)
                        {
                            errorHandler(2, "连接服务器失败.", "");
                        }
                    }

                    if (this.txbCDAMONumber.Text == "")
                    {
                        SetTipMessage(MessageType.Error, Message("msg_No activated work order"));
                        return;
                    }
                    else
                    {
                        if (config.PRINTER_MODE == "1")
                        {
                            DialogResult dr = MessageBox.Show(Message("msg_confirm wo and select modle"), Message("msg_warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                            if (dr == DialogResult.Yes)
                            {
                                LoadYield();

                                if (config.PRINTER_MODE == "2" || config.PRINTER_MODE == "3")
                                {
                                    label8.Text = initModel.currentSettings.QuantityMO.ToString();
                                    this.label10.Text = ReadPRintCount(); //this.lblPass.Text;
                                }
                                
                                GetBCIInit();
                            }
                            else
                            {
                                this.txbCDAMONumber.Text = "";
                                this.txbCDAPartNumber.Text = "";
                                errorHandler(3, Message("msg_Please active a workorder"), "");
                                return;
                            }
                        }
                        else
                        {
                            LoadYield();

                            label8.Text = initModel.currentSettings.QuantityMO.ToString();
                            this.label10.Text = ReadPRintCount(); //this.lblPass.Text;
                        }
                    }

                    InitSetupGrid();//根据工单获取上料
                    InitEquipmentGrid();
                    //刷新工单的状态图标
                    SetWorkorderGridStatus();
                    if (!hasWarningMsg)
                    {
                        SetTipMessage(MessageType.OK, Message("msg_Initialize Success"));
                    }
                }
                catch (Exception ex)
                {
                    SetTipMessage(MessageType.Error, ex.Message);
                    LogHelper.Error(ex);
                }
            });
        }

        /// <summary>
        /// 提示窗显示
        /// </summary>
        private void ShowTopWindow()
        {
            if (topmostform == null)
            {
                topmostform = new TopMostForm(this);
                topmostHandle = new HandleInterfaceUpdateTopMostDelegate(topmostform.UpdateData);
                topmostform.Show();
            }
        }

        #region add by qy
        public void InitCintrolLanguage(Form form)
        {
            MutiLanguages lang = new MutiLanguages();
            foreach (Control ctl in this.Controls)
            {
                lang.InitLangauge(ctl);
                if (ctl is TabControl)
                {
                    lang.InitLangaugeForTabControl((TabControl)ctl);
                }
            }

            //Controls不包含ContextMenuStrip，可用以下方法获得
            System.Reflection.FieldInfo[] fieldInfo = this.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            for (int i = 0; i < fieldInfo.Length; i++)
            {
                switch (fieldInfo[i].FieldType.Name)
                {
                    case "ContextMenuStrip":
                        ContextMenuStrip contextMenuStrip = (ContextMenuStrip)fieldInfo[i].GetValue(this);
                        lang.InitLangauge(contextMenuStrip);
                        break;
                }
            }
        }

        public string Message(string messageId)
        {
            return MutiLanguages.ParserString("$" + messageId);
        }
        #endregion

        #endregion

        #region delegate
        public delegate void errorHandlerDel(int typeOfError, String logMessage, String labelMessage);
        public void errorHandler(int typeOfError, String logMessage, String labelMessage)
        {
            if (txtConsole.InvokeRequired)
            {
                errorHandlerDel errorDel = new errorHandlerDel(errorHandler);
                Invoke(errorDel, new object[] { typeOfError, logMessage, labelMessage });
            }
            else
            {
                String errorBuilder = null;
                String isSucces = null;
                switch (typeOfError)
                {
                    case 0:
                        isSucces = "SUCCESS";
                        txtConsole.SelectionColor = Color.Black;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + logMessage + "\n";
                        SetTipMessage(MessageType.OK, logMessage);
                        break;
                    case 1:
                        isSucces = "SUCCESS";
                        txtConsole.SelectionColor = Color.Black;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + logMessage + "\n";
                        break;
                    case 2:
                        isSucces = "FAIL";
                        txtConsole.SelectionColor = Color.Red;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + logMessage + "\n";
                        SetTipMessage(MessageType.Error, logMessage);
                        break;
                    case 3:
                        isSucces = "FAIL";
                        txtConsole.SelectionColor = Color.Red;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + logMessage + "\n";
                        SetTipMessage(MessageType.Error, logMessage);
                        SetTopWindowMessage("Error", logMessage);
                        break;
                    default:
                        isSucces = "FAIL";
                        txtConsole.SelectionColor = Color.Red;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + logMessage + "\n";
                        break;
                }
                LogHelper.Info(logMessage);
                SetStatusLabelText(logMessage);
                txtConsole.AppendText(errorBuilder);
                txtConsole.ScrollToCaret();
            }
        }

        public delegate void SetTipMessageDel(MessageType strType, string strMessage);
        public void SetTipMessage(MessageType strType, string strMessage)
        {
            if (this.messageControl1.InvokeRequired)
            {
                SetTipMessageDel setMsg = new SetTipMessageDel(SetTipMessage);
                Invoke(setMsg, new object[] { strType, strMessage });
            }
            else
            {
                switch (strType)
                {
                    case MessageType.OK:
                        this.messageControl1.BackColor = Color.FromArgb(184, 255, 160);
                        this.messageControl1.PicType = @"pic\ok.png";
                        this.messageControl1.Title = "OK";
                        //this.messageControl1.MFontSize = 32f;
                        this.messageControl1.Content = strMessage;
                        break;
                    case MessageType.Error:
                        this.messageControl1.BackColor = Color.Red;
                        this.messageControl1.PicType = @"pic\Close.png";
                        this.messageControl1.Title = "Error Message";
                        //this.messageControl1.MFontSize = 32f;
                        this.messageControl1.Content = strMessage;
                        break;
                    case MessageType.Instruction:
                        this.messageControl1.BackColor = Color.FromArgb(184, 255, 160);
                        this.messageControl1.PicType = @"pic\Instruction.png";
                        this.messageControl1.Title = "Instruction";
                        //this.messageControl1.MFontSize = 32f;
                        this.messageControl1.Content = strMessage;
                        break;
                    default:
                        this.messageControl1.BackColor = Color.FromArgb(184, 255, 160);
                        this.messageControl1.PicType = "";//@"pic\ok.png";
                        this.messageControl1.Title = "OK";
                        //this.messageControl1.MFontSize = 32f;
                        this.messageControl1.Content = strMessage;
                        break;
                }
                SetStatusLabelText(strMessage);
            }
        }

        public delegate void SetConnectionTextDel(int typeOfError, string strMessage);
        public void SetConnectionText(int typeOfError, string strMessage)
        {
            if (txtConnection.InvokeRequired)
            {
                SetConnectionTextDel connectDel = new SetConnectionTextDel(SetConnectionText);
                Invoke(connectDel, new object[] { typeOfError, strMessage });
            }
            else
            {
                String errorBuilder = null;
                String isSucces = null;
                switch (typeOfError)
                {
                    case 0:
                        isSucces = "SUCCESS";
                        txtConnection.SelectionColor = Color.Black;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + strMessage + "\n";
                        //LogHelper.Info(strMessage);
                        break;
                    case 1:
                        isSucces = "FAIL";
                        txtConnection.SelectionColor = Color.Red;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + strMessage + "\n";
                        LogHelper.Error(strMessage);
                        break;
                    default:
                        isSucces = "FAIL";
                        txtConnection.SelectionColor = Color.Red;
                        errorBuilder = "# " + DateTime.Now.ToString("HH:mm:ss") + " >> " + isSucces + " >< " + strMessage + "\n";
                        break;
                }

                txtConnection.AppendText(errorBuilder);
                txtConnection.ScrollToCaret();
            }
        }

        public void SetStatusLabelText(string strText)
        {
            this.InvokeEx(x => this.lblStatus.Text = strText);
        }

        public string GetWorkOrderValue()
        {
            string str = "";
            this.InvokeEx(x => str = this.txbCDAMONumber.Text);
            return str;
        }

        public string GetPartNumberValue()
        {
            //edit by qy
            string str = "";
            this.InvokeEx(x => str = this.txbCDAPartNumber.Text);
            return str;
        }

        public int GetSetupRowCount()
        {
            int icount = 0;
            this.InvokeEx(x => icount = this.gridSetup.Rows.Count);
            return icount;
        }

        public TextBox getFieldPartNumber()
        {
            return this.txbCDAPartNumber;
        }

        public TextBox getFieldWorkorder()
        {
            return this.txbCDAMONumber;
        }

        public Label getFieldLabelUser()
        {
            return lblUser;
        }

        public Label getFieldLabelTime()
        {
            return lblLoginTime;
        }
        #endregion

        #region Data process function
        public void DataRecivedHeandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            try
            {
                if (!VerifyCheckList())
                {
                    return;
                }
                Thread.Sleep(500);
                Byte[] bt = new Byte[sp.BytesToRead];
                sp.Read(bt, 0, sp.BytesToRead);
                indata = System.Text.Encoding.ASCII.GetString(bt).Trim();
                this.Invoke(new MethodInvoker(delegate
                {
                    this.txbCDADataInput.Text = indata;
                }));
                LogHelper.Info("Scan number(original): " + indata);
                //match material bin number
                Match match = Regex.Match(indata, config.MBNExtractPattern);
                if (match.Success)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.tabControl1.SelectedTab = this.tabSetup;
                    }));
                    if (config.AutoNextMaterial.ToUpper() == "ENABLE")
                        ProcessMaterialBinNo(match.ToString());
                    else
                        ProcessMaterialBinNoEXT(match.ToString());
                    return;
                }
                match = Regex.Match(indata, config.EquipmentExtractPattern);
                if (match.Success)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.tabControl1.SelectedTab = this.tabEquipment;
                    }));
                    ProcessEquipmentData(match.ToString());
                    return;
                }
                errorHandler(2, Message("msg_Error barcode"), "");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }
        #endregion

        #region Event
        private void MainView_Load(object sender, EventArgs e)
        {
            NetworkChange.NetworkAvailabilityChanged += AvailabilityChanged;
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show(Message("msg_Do you want to close the application"), Message("msg_Quit Application"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.OK)
            {
                try
                {
                    SavePRintCount();
                    if (!string.IsNullOrEmpty(this.txbCDAMONumber.Text))
                    {
                        SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
                        setupHandler.SetupStateChange(this.txbCDAMONumber.Text, 2, initModel.currentSettings.processLayer);

                        EquipmentManager eqManager = new EquipmentManager(sessionContext, initModel, this);
                        foreach (DataGridViewRow row in dgvEquipment.Rows)
                        {
                            string equipmentNo = row.Cells["EquipNo"].Value.ToString();
                            string equipmentIndex = row.Cells["EquipmentIndex"].Value.ToString();
                            if (string.IsNullOrEmpty(equipmentNo))
                                continue;
                            int errorCode = eqManager.UpdateEquipmentData(equipmentIndex, equipmentNo, 1);
                            RemoveAttributeForEquipment(equipmentNo, equipmentIndex, "attribEquipmentHasRigged");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message, ex);
                }
                finally
                {
                    LogHelper.Info("Application end...");
                    System.Environment.Exit(0);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedTab.Name == "tabSetup")
            {
                this.gridSetup.ClearSelection();
            }
            else if (this.tabControl1.SelectedTab.Name == "tabDocument")
            {
                this.gridDocument.ClearSelection();
            }
            else if (this.tabControl1.SelectedTab.Name == "tabEquipment")
            {
                this.dgvEquipment.ClearSelection();
            }
            else if (this.tabControl1.SelectedTab.Name == "tabCheckList")
            {
                this.gridCheckList.ClearSelection();
            }
            else if (this.tabControl1.SelectedTab.Name == "tabWOActived")
            {
                this.gridWorkorder.ClearSelection();
                SetWorkorderGridStatus();
            }
        }

        private void gridDocument_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            long documentID = Convert.ToInt64(gridDocument.Rows[e.RowIndex].Cells[0].Value.ToString());
            string fileName = gridDocument.Rows[e.RowIndex].Cells[1].Value.ToString();
            SetDocumentControlForDoc(documentID, fileName);
        }

        private void txbCDADataInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (!VerifyCheckList())
                {
                    this.txbCDADataInput.SelectAll();
                    this.txbCDADataInput.Focus();
                    return;
                }
                indata = this.txbCDADataInput.Text.Trim();
                LogHelper.Info("Scan number(original): " + indata);
                //match material bin number
                Match match = Regex.Match(indata, config.MBNExtractPattern);
                if (match.Success)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.tabControl1.SelectedTab = this.tabSetup;
                    }));
                    if (config.AutoNextMaterial.ToUpper() == "ENABLE")
                        ProcessMaterialBinNo(match.ToString());
                    else
                        //上料
                        ProcessMaterialBinNoEXT(match.ToString());
                    this.txbCDADataInput.SelectAll();
                    this.txbCDADataInput.Focus();
                    return;
                }
                match = Regex.Match(indata, config.EquipmentExtractPattern);
                if (match.Success)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.tabControl1.SelectedTab = this.tabEquipment;
                    }));
                    ProcessEquipmentData(match.ToString());
                    this.txbCDADataInput.SelectAll();
                    this.txbCDADataInput.Focus();
                    return;
                }
                this.txbCDADataInput.SelectAll();
                this.txbCDADataInput.Focus();
                errorHandler(2, Message("msg_Error barcode"), "");
            }
        }
        #endregion

        #region Other functions

        /// <summary>
        /// 显示基本数据
        /// </summary>
        private void LoadYield()
        {
            GetProductQuantity getProductHandler = new GetProductQuantity(sessionContext, initModel, this);
            if (!string.IsNullOrEmpty(this.txbCDAMONumber.Text))
            {
                this.lblWOQty.Text = initModel.currentSettings.QuantityMO.ToString();
                ProductEntity entity = getProductHandler.GetProductQty(initModel.currentSettings.processLayer, this.txbCDAMONumber.Text);
                if (entity != null)
                {
                    int totalQty = Convert.ToInt32(entity.QUANTITY_PASS) + Convert.ToInt32(entity.QUANTITY_FAIL) + Convert.ToInt32(entity.QUANTITY_SCRAP);
                    this.lblPass.Text = entity.QUANTITY_PASS;
                    this.lblFail.Text = entity.QUANTITY_FAIL;
                    //this.lblAllCount.Text = totalQty.ToString();
                    lblScrap.Text = entity.QUANTITY_SCRAP;
                    this.lblYield.Text = "0%";
                    if (totalQty > 0)
                    {
                        this.lblYield.Text = Math.Round(Convert.ToDecimal(lblPass.Text) / Convert.ToDecimal(totalQty) * 100, 2) + "%";
                    }
                }
            }
        }

        Dictionary<string, string> dicAttris = new Dictionary<string, string>();
        private void GetWorkOrderAttris()
        {
            if (string.IsNullOrEmpty(this.txbCDAMONumber.Text))
                return;
            GetAttributeValue getAttriHandler = new GetAttributeValue(sessionContext, initModel, this);
            dicAttris = getAttriHandler.GetAllAttributeValuesForWO(this.txbCDAMONumber.Text);
        }

        private void InitSetupGrid()
        {
            this.gridSetup.Rows.Clear();
            initModel.numberOfSingleBoards = 0;
            GetMaterialBinData getMaterial = new GetMaterialBinData(sessionContext, initModel, this);
            DataTable dt = getMaterial.GetBomMaterialData(this.txbCDAMONumber.Text);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    GetNumbersOfSingleBoards getNumBoard = new GetNumbersOfSingleBoards(sessionContext, initModel, this);
                    List<MdataGetPartData> listData = getNumBoard.GetNumbersOfSingleBoardsResultCall(row["PartNumber"].ToString());
                    if (listData != null && listData.Count > 0)
                    {
                        MdataGetPartData mData = listData[0];
                        initModel.numberOfSingleBoards = mData.quantityMultipleBoard;
                        InitScrapGUI();
                        this.gridSetup.Rows.Add(new object[] { LaserMark.Properties.Resources.Close, "", row["PartNumber"], row["PartDesc"], initModel.numberOfSingleBoards, "", "", "", row["CompName"], "" });
                    }
                }

                this.gridSetup.ClearSelection();
            }
        }

        private void ProcessMaterialBinNo(string materialBinNo)
        {
            GetMaterialBinData getMaterialHandler = new GetMaterialBinData(sessionContext, initModel, this);
            string[] values = getMaterialHandler.GetMaterialBinDataDetails(materialBinNo);
            //"MATERIAL_BIN_NUMBER", "MATERIAL_BIN_PART_NUMBER", "MATERIAL_BIN_QTY_ACTUAL", "MATERIAL_BIN_QTY_TOTAL", "PART_DESC", "SUPPLIER_CHARGE_NUMBER" 
            if (values != null && values.Length > 0)
            {
                string strPartNumber = values[1];
                string strActualQty = values[2];
                string strLotNumber = values[5];
                string lockState = values[7];
                if (lockState == "-1")
                {
                    errorHandler(2, Message("msg_TheContainerIsLocked"), "");
                    return;
                }
                bool isMatch = false;
                foreach (DataGridViewRow row in gridSetup.Rows)
                {
                    if (row.Cells["PartNumber"].Value.ToString() == strPartNumber)
                    {
                        isMatch = true;
                        if (row.Cells["MaterialBinNo"].Value == null || row.Cells["MaterialBinNo"].Value.ToString() == "")
                        {
                            row.Cells["MaterialBinNo"].Value = materialBinNo;
                            row.Cells["Qty"].Value = Convert.ToInt32(Convert.ToDouble(strActualQty));
                            row.Cells["LotNumber"].Value = strLotNumber;
                            row.Cells["ScanTime"].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            row.Cells["Checked"].Value = LaserMark.Properties.Resources.ok;
                            row.Cells["MaterialBinNo"].Style.BackColor = Color.FromArgb(0, 192, 0);

                            //setup material
                            SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
                            //上物料
                            setupHandler.UpdateMaterialSetUpByBin(initModel.currentSettings.processLayer, this.txbCDAMONumber.Text, materialBinNo, strActualQty, strPartNumber, config.StationNumber + "_01", "01");
                            //更改物料状态
                            setupHandler.SetupStateChange(this.txbCDAMONumber.Text, 0, initModel.currentSettings.processLayer);
                            break;
                        }
                        else
                        {
                            if (CheckMaterialBinHasSetup(materialBinNo))
                                return;
                            this.Invoke(new MethodInvoker(delegate
                            {
                                gridSetup.Rows.Add();
                                DataGridViewRow newRow = gridSetup.Rows[gridSetup.Rows.Count - 1];
                                newRow.Cells["Checked"].Value = LaserMark.Properties.Resources.ok;
                                newRow.Cells["MaterialBinNo"].Value = materialBinNo;
                                newRow.Cells["PartNumber"].Value = row.Cells["PartNumber"].Value;
                                newRow.Cells["PartDesc"].Value = row.Cells["PartDesc"].Value;
                                newRow.Cells["PQty"].Value = row.Cells["PQty"].Value;
                                newRow.Cells["Qty"].Value = Convert.ToInt32(Convert.ToDouble(strActualQty));
                                newRow.Cells["LotNumber"].Value = strLotNumber;
                                newRow.Cells["ScanTime"].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                                newRow.Cells["MaterialBinNo"].Style.BackColor = Color.FromArgb(0, 192, 0);
                            }));
                            break;
                        }
                    }
                }
                if (!isMatch)
                {
                    errorHandler(2, Message("msg_Material Bin number ") + materialBinNo + "(" + strPartNumber + ") " + Message("msg_is mismatch"), "");
                    return;
                }
                CheckMaterialBinQty();
                SetTipMessage(MessageType.OK, Message("msg_Material Bin number ") + materialBinNo + "(" + strPartNumber + ") " + Message("msg_is successful."));
                if (BoardCome)
                {
                    ProcessSerialNumberData();
                }
            }
        }

        private void ProcessMaterialBinNoEXT(string materialBinNo)
        {
            GetMaterialBinData getMaterialHandler = new GetMaterialBinData(sessionContext, initModel, this);
            //获取UID信息
            string[] values = getMaterialHandler.GetMaterialBinDataDetails(materialBinNo);
            //"MATERIAL_BIN_NUMBER", "MATERIAL_BIN_PART_NUMBER", "MATERIAL_BIN_QTY_ACTUAL", "MATERIAL_BIN_QTY_TOTAL", "PART_DESC", "SUPPLIER_CHARGE_NUMBER" 
            if (values != null && values.Length > 0)
            {
                string strPartNumber = values[1];
                string strActualQty = values[2];
                string strLotNumber = values[5];
                string lockState = values[7];
                if (lockState == "-1")
                {
                    errorHandler(2, Message("msg_TheContainerIsLocked"), "");
                    return;
                }
                bool isMatch = false;
                foreach (DataGridViewRow row in gridSetup.Rows)
                {
                    if (row.Cells["PartNumber"].Value.ToString() == strPartNumber)
                    {
                        isMatch = true;
                        //if (row.Cells["MaterialBinNo"].Value == null || row.Cells["MaterialBinNo"].Value.ToString() == "")
                        //{
                        row.Cells["MaterialBinNo"].Value = materialBinNo;
                        row.Cells["Qty"].Value = Convert.ToInt32(Convert.ToDouble(strActualQty));
                        row.Cells["LotNumber"].Value = strLotNumber;
                        row.Cells["ScanTime"].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                        row.Cells["Checked"].Value = LaserMark.Properties.Resources.ok;
                        row.Cells["MaterialBinNo"].Style.BackColor = Color.FromArgb(0, 192, 0);

                        //setup material
                        SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
                        //上料
                        setupHandler.UpdateMaterialSetUpByBin(initModel.currentSettings.processLayer, this.txbCDAMONumber.Text, materialBinNo, strActualQty, strPartNumber, config.StationNumber + "_01", "01");
                        //激活工单上料
                        setupHandler.SetupStateChange(this.txbCDAMONumber.Text, 0, initModel.currentSettings.processLayer);
                        break;
                        //}
                    }
                }
                if (!isMatch)
                {
                    errorHandler(2, Message("msg_Material Bin number ") + materialBinNo + "(" + strPartNumber + ") " + Message("msg_is mismatch"), "");
                    return;
                }
                 CheckMaterialBinQty();
                SetTipMessage(MessageType.OK, Message("msg_Material Bin number ") + materialBinNo + "(" + strPartNumber + ") " + Message("msg_is successful."));
                if (BoardCome)
                {
                    ProcessSerialNumberData();
                }
            }
        }

        public string Process1010Command()
        {
            if (!VerifyCheckList())
            {
                this.txbCDADataInput.SelectAll();
                this.txbCDADataInput.Focus();
                errorHandler(3, Message("$msg_checklist_first"), "");
                return "1012;NG;Check List required"; ;
            }
            if (!CheckMaterialSetUp())
            {
                return "1012;NG;Setup required";
            }
            string retureMsg = "";
            StringBuilder sb = new StringBuilder();
            //setup check
            SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
            GetNextSerialNumber getSNHandler = new GetNextSerialNumber(sessionContext, initModel, this);
            string workorder = GetWorkOrderValue();
            int resultCode = setupHandler.SetupCheck(workorder);
            if (resultCode == 0)
            {
                List<SerialNumberData> snList = new List<SerialNumberData>();
                SerialNumberData[] snData = getSNHandler.GetSerialNumber(workorder, 1);
                if (snData != null && snData.Length > 0)
                {
                    string snPanel = snData[0].serialNumber;
                    if (initModel.numberOfSingleBoards == 1)
                    {
                        SerialNumberData sn = new SerialNumberData(snPanel, "1", "-1");
                        snList.Add(sn);
                        sb.Append(snPanel);//+ "01"
                        retureMsg = "1011;;" + sb.ToString() + ";OK#";
                    }
                    else
                    {
                        for (int i = 0; i < initModel.numberOfSingleBoards; i++)
                        {
                            SerialNumberData sn = new SerialNumberData(snPanel + (i + 1).ToString().PadLeft(2, '0'), (i + 1).ToString(), "-1");
                            snList.Add(sn);
                            sb.Append(snPanel + (i + 1).ToString().PadLeft(2, '0')).Append(";");
                        }
                        retureMsg = "1011;" + snPanel + ";" + sb.ToString().TrimEnd(new char[] { ';' }) + ";OK#";
                    }
                    //assign serial number to work order
                    if (snList.Count > 0)
                    {
                        int errorCode = 0;
                        AssignSerialNumber assignHandler = new AssignSerialNumber(sessionContext, initModel, this);
                        if (initModel.numberOfSingleBoards == 1)
                        {
                            errorCode = assignHandler.AssignSerialNumberResultCallForSingle(snList.ToArray(), workorder, initModel.currentSettings.processLayer);
                        }
                        else
                        {
                            errorCode = assignHandler.AssignSerialNumberResultCallForMul(snPanel, snList.ToArray(), workorder, initModel.currentSettings.processLayer);
                        }
                        if (errorCode != 0)
                        {
                            retureMsg = "1012;" + errorCode + ";assign serial number to wo error#";
                        }
                    }
                }
            }
            else
            {
                retureMsg = "1012;" + resultCode + ";setup check error#";
            }
            return retureMsg;
        }

        public string Process2010Command(string commandText)
        {
            if (!VerifyCheckList())
            {
                this.txbCDADataInput.SelectAll();
                this.txbCDADataInput.Focus();
                errorHandler(3, Message("$msg_checklist_first"), "");
                return "2012;NG;Check List required"; ;
            }
            if (!CheckMaterialSetUp())
            {
                return "2012;NG;Setup required";
            }
            string retureMsg = "";
            SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
            string workorder = GetWorkOrderValue();
            int resultCode1 = setupHandler.SetupCheck(workorder);
            if (resultCode1 == 0)
            {
                List<string> snValueList = new List<string>();
                string[] values = commandText.Split(new char[] { ';' });
                if (values.Length == 4)
                {
                    string[] snSignalValues = values[2].Split(new char[] { ',' });
                    string serialNumber = snSignalValues[0];
                    string snState = snSignalValues[1];
                    int iState = snState == "OK" ? 0 : 1;
                    snValueList.Add("0");
                    snValueList.Add(serialNumber);
                    snValueList.Add("01");
                    snValueList.Add(iState + "");
                }
                else
                {
                    for (int i = 2; i < values.Length - 1; i++)
                    {
                        string[] snMulitValues = values[i].Split(new char[] { ',' });
                        string serialNumber = snMulitValues[0];
                        string snState = snMulitValues[1];
                        int iState = snState == "OK" ? 0 : 1;
                        snValueList.Add("0");
                        snValueList.Add(serialNumber);
                        snValueList.Add((i - 1).ToString());
                        snValueList.Add(iState + "");
                    }
                }
                if (snValueList.Count > 0)
                {
                    UploadProcessResult uploadHandler = new UploadProcessResult(sessionContext, initModel, this);
                    int resultCode = uploadHandler.UploadProcessResultCall(snValueList.ToArray(), initModel.currentSettings.processLayer);
                    if (resultCode == 0)
                    {
                        //consumption material
                        string[] setupMBNs = setupHandler.GetSetupMaterialByStation(initModel.configHandler.StationNumber, initModel.currentSettings.processLayer);
                        foreach (var item in setupMBNs)
                        {
                            LogHelper.Info("Has setup material bin number :" + item);
                        }
                        string findMBN = FindMaterialBinNumber();
                        LogHelper.Info("Find material bin number :" + findMBN);
                        if (setupMBNs.Contains(findMBN))
                        {
                            setupHandler.UpdateMaterialBinBooking(initModel.configHandler.StationNumber, findMBN, -initModel.numberOfSingleBoards);
                            this.Invoke(new MethodInvoker(delegate
                            {
                                //update UI data
                                UpdateMaterialGridData(findMBN, initModel.numberOfSingleBoards, snValueList[1]);
                                //verify material qty is less then min value
                                CheckMaterialBinQty();
                                LoadYield();
                            }));
                        }
                        else
                        {

                        }
                        retureMsg = "2011;OK#";
                        return retureMsg;
                    }
                }
                retureMsg = "2012;NG;Error";
            }
            else
            {
                retureMsg = "2012;" + resultCode1 + ";setup check error#";
            }
            return retureMsg;
        }

        private string FindMaterialBinNumber()
        {
            string mbn = "";
            foreach (DataGridViewRow row in gridSetup.Rows)
            {
                mbn = row.Cells["MaterialBinNo"].Value.ToString();
                string qty = row.Cells["Qty"].Value.ToString();
                if (Convert.ToInt32(qty) > 0)
                {
                    break;
                }
            }
            LogHelper.Info("Get material bin number for consumption " + mbn);
            return mbn;
        }

        private void UpdateMaterialGridData(string materialBinNumber, int qty, string serialNumber)
        {
            ProcessMaterialBinData materialHandler = new ProcessMaterialBinData(sessionContext, initModel, this);
            for (int i = 0; i < this.gridSetup.Rows.Count; i++)
            {
                if (gridSetup.Rows[i].Cells["MaterialBinNo"].Value.ToString() == materialBinNumber)
                {
                    int iQty = Convert.ToInt32(gridSetup.Rows[i].Cells["Qty"].Value);
                    LogHelper.Info("Consumption material:" + materialBinNumber + ",quantity:" + iQty + ",subQty:" + qty);
                    if (iQty >= qty)
                    {
                        gridSetup.Rows[i].Cells["Qty"].Value = iQty - qty;
                        gridSetup.Rows[i].Cells["Mark"].Value = "Last Laser Marked SN is " + serialNumber;
                        int errorMaterial = materialHandler.UpdateMaterialBinBooking(materialBinNumber, this.txbCDAMONumber.Text, -qty);
                        if (iQty == qty)//update 2015/6/24
                        {
                            if (i + 1 < this.gridSetup.Rows.Count)
                            {
                                string nextMaterialBinNo = gridSetup.Rows[i + 1].Cells["MaterialBinNo"].Value.ToString();
                                string nextPartNumber = gridSetup.Rows[i + 1].Cells["PartNumber"].Value.ToString();
                                string nextQty = gridSetup.Rows[i + 1].Cells["Qty"].Value.ToString();
                                //setup material
                                SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
                                setupHandler.UpdateMaterialSetUpByBin(initModel.currentSettings.processLayer, this.txbCDAMONumber.Text, nextMaterialBinNo, nextQty, nextPartNumber, config.StationNumber + "_01", "01");
                                setupHandler.SetupStateChange(this.txbCDAMONumber.Text, 0, initModel.currentSettings.processLayer);
                            }
                            else
                            {
                                errorHandler(3, Message("msg_Please setup material."), "");
                            }
                        }
                        break;
                    }
                    else
                    {
                        gridSetup.Rows[i].Cells["Qty"].Value = 0;
                        int errorMaterial = materialHandler.UpdateMaterialBinBooking(materialBinNumber, this.txbCDAMONumber.Text, -iQty);
                        if (i + 1 < this.gridSetup.Rows.Count)
                        {
                            string nextMaterialBinNo = gridSetup.Rows[i + 1].Cells["MaterialBinNo"].Value.ToString();
                            string nextPartNumber = gridSetup.Rows[i + 1].Cells["PartNumber"].Value.ToString();
                            string nextQty = gridSetup.Rows[i + 1].Cells["Qty"].Value.ToString();
                            //setup material
                            SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
                            setupHandler.UpdateMaterialSetUpByBin(initModel.currentSettings.processLayer, this.txbCDAMONumber.Text, nextMaterialBinNo, nextQty, nextPartNumber, config.StationNumber + "_01", "01");
                            setupHandler.SetupStateChange(this.txbCDAMONumber.Text, 0, initModel.currentSettings.processLayer);
                            UpdateMaterialGridData(nextMaterialBinNo, qty - iQty, serialNumber);
                        }
                        else
                        {
                            //warming no material
                            errorHandler(3, Message("msg_Please setup material."), "");
                        }
                    }
                }
            }
        }


        private void CheckMaterialBinQty()
        {
            int iTotalQty = 0;
            string partNumber = "";
            for (int i = 0; i < gridSetup.Rows.Count; i++)
            {
                DataGridViewRow row = gridSetup.Rows[i];
                if (row.Cells["Qty"].Value != null && row.Cells["Qty"].Value.ToString() == "0")
                {
                    if (this.gridSetup.Rows.Count > 1)
                    {
                        gridSetup.Rows.Remove(row);
                    }
                    else
                    {
                        //LaserMark.Properties.Resources.Close, "", row["PartNumber"], row["PartDesc"], "", "", "", row["CompName"], ""
                        row.Cells[0].Value = LaserMark.Properties.Resources.Close;
                        row.Cells[1].Value = "";
                        row.Cells[4].Value = "";
                        row.Cells[5].Value = "";
                        row.Cells[6].Value = "";
                        row.Cells[8].Value = "";
                        row.Cells["MaterialBinNo"].Style.BackColor = Color.FromArgb(255, 255, 255);
                    }
                }
            }
            foreach (DataGridViewRow item in gridSetup.Rows)
            {
                partNumber = item.Cells["PartNumber"].Value.ToString();
                if (item.Cells["Qty"].Value != null && item.Cells["Qty"].Value.ToString().Length > 0)
                {
                    iTotalQty += Convert.ToInt32(item.Cells["Qty"].Value);
                }
            }
            if (iTotalQty <= Convert.ToInt32(config.MaterialWarningQty))
            {
                errorHandler(2, Message("msg_Material Qty is less, Please scan new Material bin."), "");
                this.SetTopWindowMessage(partNumber, Message("msg_Material Qty is less, Please scan new Material bin."));
            }
            else
            {
                this.SetTopWindowMessage("", "");
            }
        }

        private bool CheckMaterialBinHasSetup(string materailBinNo)
        {
            bool isExist = false;
            foreach (DataGridViewRow row in gridSetup.Rows)
            {
                if (row.Cells["MaterialBinNo"].Value.ToString() == materailBinNo)
                {
                    isExist = true;
                    errorHandler(3, Message("msg_The material bin number") + materailBinNo + Message("msg_has setup yet"), "");
                    break;
                }
            }
            return isExist;
        }

        private string ConvertDateFromStamp(string timeStamp)
        {
            double d = Convert.ToDouble(timeStamp);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(d).ToLocalTime();
            return date.ToString();
        }

        private string ConverToHourAndMin(int number)
        {
            int iHour = number / 60;
            int iMin = number % 60;
            return iHour + "hr " + iMin + "min";
        }

        private bool CheckMaterialSetUp()
        {
            bool isValid = true;
            if (this.gridSetup.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in gridSetup.Rows)
                {
                    if (row.Cells["MaterialBinNo"].Value == null || row.Cells["MaterialBinNo"].Value.ToString().Length == 0)
                    {
                        errorHandler(3, Message("msg_Material setup required."), "");
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }

        public void SetTopWindowMessage(string text, string errorMsg)
        {
            if (topmostform != null)
            {
                this.Invoke(topmostHandle, new string[] { text, errorMsg });
            }
            else
            {
                topmostform = new TopMostForm(this);
                topmostHandle = new HandleInterfaceUpdateTopMostDelegate(topmostform.UpdateData);
                topmostform.Show();
                this.Invoke(topmostHandle, new string[] { text, errorMsg });
            }
        }

        private bool GenerateBJIFile(string fileName)
        {
            try
            {
                string fristRow = @"job|program|variant|batch|jobquantity|batchquantity";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(fristRow);
                sb.Append(GetWorkOrderValue()).Append("|");
                //program name edit by qy
                sb.Append(GetPCBPartNumber() + "|||");//GetPartNumberValue() + "|||"
                //number of pcb edit by qy
                sb.Append(initModel.currentSettings.QuantityMO + "|");//10000000

                string path = config.BJIPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filePath = path + @"/" + fileName + ".bji";
                //if exist delete first
                //if (File.Exists(filePath))
                //{
                //    string backupPath = path + @"/Complete/";
                //    string backupFilePath = backupPath + fileName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bji";
                //    if (!Directory.Exists(backupPath))
                //        Directory.CreateDirectory(backupPath);
                //    File.Copy(filePath, backupFilePath, true);
                //    File.Delete(filePath);
                //}
                FileStream fs = new FileStream(filePath, FileMode.Create);
                byte[] bt = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bt, 0, bt.Length);
                fs.Flush();
                fs.Close();
                errorHandler(0, "Generate BJI file success", "");
                SetTopWindowMessage("Generate BJI file success", "");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }

        Dictionary<string, string> dicSN = null;
        Dictionary<string, Dictionary<string, string>> dicSNEXT = new Dictionary<string, Dictionary<string, string>>();
        string panelSN = null;
        private bool GenerateBCIFile(string fileName, int qtyFlag, int ActnumberofBoards)
        {
            try
            {
                string path = config.BCIPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filePath = path + @"/" + fileName + ".bci";
                if (File.Exists(filePath))
                {
                    //errorHandler(3, Message("msg_BCI is exist"), "");
                    LogHelper.Error("BCI file has not deleted.");
                    return true;
                }

                dicSN = new Dictionary<string, string>();
                //string fristRow = string.Format(@"job||{0}||panelcode", GetWorkOrderValue());
                string fristRow = "panel||||panelcode";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(fristRow);
                panelSN = GenerateSerialNumber();
                for (int i = 1; i <= initModel.numberOfSingleBoards; i++)
                {
                    string tempSN = panelSN + i.ToString().PadLeft(3, '0');
                    if (qtyFlag == 1)
                    {
                        if (i <= ActnumberofBoards)
                            sb.AppendLine(tempSN + "|" + panelSN + "|");
                    }
                    else
                    {
                        sb.AppendLine(tempSN + "|" + panelSN + "|");
                    }

                    dicSN[tempSN] = i.ToString();
                    dicSNEXT[panelSN] = dicSN;
                }
                //string path = config.BCIPath;
                //if (!Directory.Exists(path))
                //    Directory.CreateDirectory(path);
                //string filePath = path + @"/" + fileName + ".bci";
                //if exist delete first
                //if (File.Exists(filePath))
                //{
                //    string backupPath = path + @"/Complete/";
                //    string backupFilePath = backupPath + fileName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bci";
                //    if (!Directory.Exists(backupPath))
                //        Directory.CreateDirectory(backupPath);
                //    File.Copy(filePath, backupFilePath, true);
                //    File.Delete(filePath);
                //}
                //delete file in finished folder 
                DirectoryInfo di = Directory.GetParent(path);
                string deletePath = di.FullName + @"\finished\";
                string deleteFilePath = deletePath + fileName + ".bci";
                if (File.Exists(deleteFilePath))
                {
                    string backupPath = di.FullName + @"\Complete\";
                    string backupFilePath = backupPath + fileName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bci";
                    if (!Directory.Exists(backupPath))
                        Directory.CreateDirectory(backupPath);
                    File.Copy(deleteFilePath, backupFilePath, true);
                    File.Delete(deleteFilePath);
                }

                FileStream fs = new FileStream(filePath, FileMode.Create);
                byte[] bt = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bt, 0, bt.Length);
                fs.Flush();
                fs.Close();
                errorHandler(0, "Generate BCI file success", "");
                SetTopWindowMessage("Generate BCI file success", "");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }

        private string GenerateSerialNumber()
        {
            string serialNumber = "";
            //GetNextSerialNumber getSNHandler = new GetNextSerialNumber(sessionContext, initModel, this);
            //SerialNumberData[] snData = getSNHandler.GetSerialNumber(GetWorkOrderValue(), 1);
            //serialNumber = snData[0].serialNumber;
            //get serial number by attribute
            string workorder = GetWorkOrderValue();
            GetAttributeValue getAttribHandler = new GetAttributeValue(sessionContext, initModel, this);
            AppendAttribute appendAttribHandler = new AppendAttribute(sessionContext, initModel, this);
            string[] attribValues = getAttribHandler.GetAttributeValueForAll(1, workorder, "-1", "WOAttrib_SN_SEQ");
            if (attribValues == null || attribValues.Length == 0)
            {
                serialNumber = workorder + "0001";
                appendAttribHandler.AppendAttributeForAll(1, workorder, "-1", "WOAttrib_SN_SEQ", "1");
            }
            else
            {
                string attribValue = attribValues[1];
                int seq = Convert.ToInt32(attribValue) + 1;
                serialNumber = workorder + seq.ToString().PadLeft(4, '0');
                appendAttribHandler.AppendAttributeForAll(1, workorder, "-1", "WOAttrib_SN_SEQ", seq + "");
            }
            LogHelper.Info("Generate serial number :" + serialNumber);
            return serialNumber;
        }

        public void ListenFile(string filepath)
        {
            if (!VerifyCheckList())
            {
                return;
            }
            if (!File.Exists(filepath))
                return;
            string strExtension = Path.GetExtension(filepath);
            string strFileName = Path.GetFileName(filepath);
            if (strExtension != ".trace")
            {
                return;
            }
            string[] lines = File.ReadAllLines(filepath);
            try
            {
                ////delete file in finished folder 
                //string path = config.BCIPath;
                //DirectoryInfo di = Directory.GetParent(path);
                //string deletePath = di.FullName + @"\finished/";
                //string deleteFilePath = deletePath + GetWorkOrderValue() + ".bci";
                //if (File.Exists(deleteFilePath))
                //{
                //    string backupPath = di.FullName + @"\Complete\";
                //    string backupFilePath = backupPath + GetWorkOrderValue() + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bci";
                //    if (!Directory.Exists(backupPath))
                //        Directory.CreateDirectory(backupPath);
                //    File.Copy(deleteFilePath, backupFilePath, true);
                //    File.Delete(deleteFilePath);
                //}

                //get reference serial number
                string[] values = lines[lines.Length - 1].Split(new char[] { '|' });
                string refSerialNumber = values[1];
                string processLayer = values[3];
                string pcbPartNumber = values[4];
                string refStatus = values[5].ToUpper() == "OK" ? "0" : config.UPLOAD_NG_MODE;

                int iProcessLayer = -1;
                CommonFunction commonHandler = new CommonFunction(sessionContext, initModel, this);
                iProcessLayer = commonHandler.GetProcessLayerByWO(GetWorkOrderValue(), config.StationNumber);

                //if (GetSetupRowCount() > 0 && pcbPartNumber != GetPCBPartNumber())
                //{
                //    errorHandler(3, "Wrong part number(PCB part number not equal which has setup)", "");
                //    MoveFileToErrorFolder(filepath, "wrong part number");
                //    return;
                //}

                Dictionary<string, string> dicSNStatus = new Dictionary<string, string>();
                //get small panel serial number
                List<SerialNumberData> snList = new List<SerialNumberData>();
                List<string> serialNumberArray = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] subValues = lines[i].Split(new char[] { '|' });
                    string serialNumber = subValues[1];
                    string status = subValues[5].ToUpper() == "OK" ? "0" : config.UPLOAD_NG_MODE;
                    if (dicSNStatus.ContainsKey(serialNumber))
                    {
                        if (status != "0")//status=="2"
                        {
                            dicSNStatus[serialNumber] = status;
                        }
                    }
                    else
                    {
                        dicSNStatus[serialNumber] = status;
                    }
                }

                string refSN = "";

                foreach (var panleSN in dicSNEXT.Keys)
                {
                    if (refSerialNumber.Contains(panleSN))
                    {
                        refSN = panleSN;
                        LogHelper.Info("Match refSN:" + panleSN);
                        Dictionary<string, string> dicSN2 = dicSNEXT[panleSN];
                        foreach (var itemSN in dicSN2.Keys)
                        {
                            LogHelper.Info("Match itemSN:" + itemSN);
                            SerialNumberData snData = new SerialNumberData(itemSN, dicSN2[itemSN], "");
                            snList.Add(snData);
                            serialNumberArray.Add("0");//"ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE"
                            serialNumberArray.Add(itemSN);
                            serialNumberArray.Add(dicSN2[itemSN]);
                            //edit by qy
                            int lqty = initModel.currentSettings.QuantityMO - Convert.ToInt32(this.lblPass.Text);
                            if (lqty < initModel.numberOfSingleBoards && Convert.ToInt32(dicSN2[itemSN]) > lqty)
                            {
                                serialNumberArray.Add("2");
                            }
                            else
                            {
                                if (this.rdbNoSPanel.Checked == false)
                                {
                                    if (manualScraplist.Contains(dicSN2[itemSN]))
                                        //serialNumberArray.Add("2");
                                        serialNumberArray.Add(config.UPLOAD_NG_MODE);
                                    else
                                        serialNumberArray.Add(dicSNStatus[itemSN]);
                                }
                                else
                                {
                                    if (manualScraplist.Contains(dicSN2[itemSN]))
                                        //serialNumberArray.Add("2");
                                        serialNumberArray.Add(config.UPLOAD_NG_MODE);
                                    else
                                        serialNumberArray.Add(dicSNStatus[itemSN.Substring(0, itemSN.Length - 3)]);
                                }
                            }
                        }

                        dicSNEXT.Remove(panleSN);
                        break;
                    }
                }

                //if logfile isn't match bci
                if (serialNumberArray.Count == 0)
                {
                    errorHandler(3, Message("msg_The logfile not match BCI"), "");
                    MoveFileToErrorFolder(filepath, Message("msg_The logfile not match BCI"));
                    return;
                }

                //assign serial numebr to work order
                AssignSerialNumber assignHandler = new AssignSerialNumber(sessionContext, initModel, this);
                int assignCode = -1;

                //分配流水号
                if (dicSN.Count == 1)
                {
                    assignCode = assignHandler.AssignSerialNumberResultCallForSingle(snList.ToArray(), GetWorkOrderValue(), iProcessLayer);
                }
                else
                {
                    assignCode = assignHandler.AssignSerialNumberResultCallForMul(refSN, snList.ToArray(), GetWorkOrderValue(), iProcessLayer);
                }
                if (assignCode == 0 || assignCode == -206)
                {
                    //update serial numer state & pass station
                    UploadProcessResult uploadHandler = new UploadProcessResult(sessionContext, initModel, this);
                    //上传状态
                    int uploadCode = uploadHandler.UploadProcessResultCall(serialNumberArray.ToArray(), iProcessLayer);
                    if (uploadCode == 0 || uploadCode == 210)
                    {
                        //consume material after upload/上传后消耗材料
                        UpdateGridDataAfterUploadState(refSerialNumber);
                        errorHandler(0, Message("msg_Process file success") + filepath, "");
                        if (!hasWarningMsg)
                        {
                            SetTopWindowMessage(filepath, "");
                        }
                        if (config.BackupsOKFile == "Y")
                        {
                            MoveFileToOKFolder(filepath);
                        }
                        else
                        {
                            if (File.Exists(filepath))
                                File.Delete(filepath);
                        }

                        this.Invoke(new MethodInvoker(delegate
                        {
                            LoadYield();
                            if (config.BAD_BOARD_AUTO_RESET.ToUpper() == "ENABLE")
                                ResetScrapGUI();
                        }));
                    }
                    else
                    {
                        errorHandler(3, Message("msg_Upload serial numebr state error"), "");
                        MoveFileToErrorFolder(filepath, Message("msg_Upload serial numebr state error"));
                    }
                }
                else
                {
                    errorHandler(3, Message("msg_Assign serial numebr error"), "");
                    MoveFileToErrorFolder(filepath, Message("msg_Assign serial numebr error"));
                }
                //dicSN.Clear();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                MoveFileToErrorFolder(filepath, ex.Message);
            }
        }

        private void UpdateGridDataAfterUploadState(string serialNumber)
        {
            foreach (DataGridViewRow row in dgvEquipment.Rows)
            {
                if (row.Cells["UsCount"].Value != null && row.Cells["UsCount"].Value.ToString().Length > 0)
                {
                    int iQty = Convert.ToInt32(row.Cells["UsCount"].Value.ToString());
                    row.Cells["UsCount"].Value = iQty - initModel.numberOfSingleBoards;//
                }
            }

            //consumption material
            SetUpManager setupHandler = new SetUpManager(sessionContext, initModel, this);
            string[] setupMBNs = setupHandler.GetSetupMaterialByStation(initModel.configHandler.StationNumber, initModel.currentSettings.processLayer);
            string findMBN = FindMaterialBinNumber();
            if (setupMBNs.Contains(findMBN))
            {
                int iPPNQty = initModel.numberOfSingleBoards;
                //setupHandler.UpdateMaterialBinBooking(initModel.configHandler.StationNumber, findMBN, -iPPNQty);
                this.Invoke(new MethodInvoker(delegate
                {
                    //update UI data/扣料
                    UpdateMaterialGridData(findMBN, iPPNQty, serialNumber);
                    LoadYield();
                    //verify material qty is less then min value
                    CheckMaterialBinQty();
                }));
            }
        }

        private void MoveFileToOKFolder(string filepath)
        {
            string OkFolder = config.LogTransOK;
            string strDir = Path.GetDirectoryName(filepath) + @"\";
            string strDirCopy = Path.GetDirectoryName(filepath);
            string strDestDir = "";
            try
            {
                if (strDir == config.LogFileFolder)//move file to ok folder
                {
                    FileInfo fInfo = new FileInfo(@"" + filepath);
                    string fileNameOnly = Path.GetFileNameWithoutExtension(filepath);
                    string extension = Path.GetExtension(filepath);
                    string newFullPath = null;
                    if (config.ChangeFileName.ToUpper() == "ENABLE")
                    {
                        newFullPath = Path.Combine(OkFolder, fileNameOnly + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension);
                    }
                    else
                    {
                        newFullPath = Path.Combine(OkFolder, fileNameOnly + extension);
                    }
                    if (!Directory.Exists(OkFolder)) Directory.CreateDirectory(OkFolder);
                    if (File.Exists(newFullPath))
                    {
                        File.Delete(newFullPath);
                    }

                    fInfo.MoveTo(@"" + newFullPath);
                }
                else//move Directory to ok folder
                {
                    string strDirName = strDirCopy.Substring(strDirCopy.LastIndexOf(@"\") + 1);
                    strDestDir = config.LogTransOK + strDirName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (!Directory.Exists(OkFolder)) Directory.CreateDirectory(OkFolder);
                    if (Directory.Exists(strDestDir))
                    {
                        Directory.Delete(strDestDir, true);
                    }
                    Directory.Move(strDir, strDestDir);
                }
                errorHandler(1, "Move file:" + filepath + " to OK folder success.", "");
            }
            catch (Exception e)
            {
                errorHandler(2, "move file error " + e.Message, "");
            }
        }

        private void MoveFileToErrorFolder(string filepath, string errorMsg)
        {
            string errorFolder = config.LogTransError;
            string strDir = Path.GetDirectoryName(filepath) + @"\";
            string strDirCopy = Path.GetDirectoryName(filepath);
            string strDestDir = "";
            try
            {
                if (strDir == config.LogFileFolder)//move file to error folder
                {
                    FileInfo fInfo = new FileInfo(@"" + filepath);
                    string fileNameOnly = Path.GetFileNameWithoutExtension(filepath);
                    string extension = Path.GetExtension(filepath);
                    string newFullPath = null;
                    if (config.ChangeFileName.ToUpper() == "ENABLE")
                    {
                        newFullPath = Path.Combine(errorFolder, fileNameOnly + "_" + errorMsg + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + extension);
                    }
                    else
                    {
                        newFullPath = Path.Combine(errorFolder, fileNameOnly + extension);
                    }
                    if (!Directory.Exists(errorFolder)) Directory.CreateDirectory(errorFolder);
                    if (File.Exists(newFullPath))
                    {
                        File.Delete(newFullPath);
                    }
                    fInfo.MoveTo(@"" + newFullPath);
                }
                else//move Directory to error folder
                {
                    string strDirName = strDirCopy.Substring(strDirCopy.LastIndexOf(@"\") + 1);
                    strDestDir = errorFolder + strDirName + "_" + errorMsg + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (!Directory.Exists(errorFolder)) Directory.CreateDirectory(errorFolder);
                    if (Directory.Exists(strDestDir))
                    {
                        Directory.Delete(strDestDir, true);
                    }
                    Directory.Move(strDir, strDestDir);
                }
                errorHandler(1, "Move file:" + filepath + " to error folder success.", "");
            }
            catch (Exception e)
            {
                errorHandler(2, "move file error " + e.Message, "");
            }
        }

        private string GetPCBPartNumber()
        {
            string pcbPN = "";
            if (gridSetup.Rows.Count > 0)
            {
                pcbPN = gridSetup.Rows[0].Cells["PartNumber"].Value.ToString();
            }
            return pcbPN;
        }

        #region Compare
        private class RowComparer : System.Collections.IComparer
        {
            private static int sortOrderModifier = 1;

            public RowComparer(SortOrder sortOrder)
            {
                if (sortOrder == SortOrder.Descending)
                {
                    sortOrderModifier = -1;
                }
                else if (sortOrder == SortOrder.Ascending) { sortOrderModifier = 1; }
            }
            public int Compare(object x, object y)
            {
                DataGridViewRow DataGridViewRow1 = (DataGridViewRow)x;
                DataGridViewRow DataGridViewRow2 = (DataGridViewRow)y;
                // Try to sort based on the Scan time column.
                string value1 = DataGridViewRow1.Cells["colItemCode"].Value.ToString();
                string value2 = DataGridViewRow2.Cells["colItemCode"].Value.ToString();
                string type1 = DataGridViewRow1.Cells["colType"].Value.ToString();
                string type2 = DataGridViewRow2.Cells["colType"].Value.ToString();
                int CompareResult = 0;
                if (type1 == type2)
                {
                    CompareResult = value1.CompareTo(value2);
                }
                else
                {
                    CompareResult = type1.CompareTo(type2);
                }
                return CompareResult * sortOrderModifier;
            }
        }
        #endregion

        #region Document
        static string cachePN = "";
        private void InitDocumentGrid()
        {
            if (config.FilterByFileName == "disable") //by station
            {
                if (gridDocument.Rows.Count <= 0)
                {
                    GetDocumentData getDocument = new GetDocumentData(sessionContext, initModel, this);
                    List<DocumentEntity> listDoc = getDocument.GetDocumentDataByStation();
                    if (listDoc != null && listDoc.Count > 0)
                    {
                        foreach (DocumentEntity item in listDoc)
                        {
                            gridDocument.Rows.Add(new object[2] { item.MDA_DOCUMENT_ID, item.MDA_FILE_NAME });
                        }
                    }
                }
            }
            else //by station & filename(partno)
            {
                if (this.txbCDAPartNumber.Text == "" || cachePN == this.txbCDAPartNumber.Text)
                    return;
                cachePN = this.txbCDAPartNumber.Text;
                gridDocument.Rows.Clear();
                this.Invoke(new MethodInvoker(delegate
                {
                    webBrowser1.Navigate("about:blank");
                }));
                GetDocumentData getDocument = new GetDocumentData(sessionContext, initModel, this);
                List<DocumentEntity> listDoc = getDocument.GetDocumentDataByStation();
                if (listDoc != null && listDoc.Count > 0)
                {
                    foreach (DocumentEntity item in listDoc)
                    {
                        string filename = item.MDA_FILE_NAME;
                        Match name = Regex.Match(filename, config.FileNamePattern);
                        if (name.Success)
                        {
                            if (name.Groups.Count > 1)
                            {
                                string partno = name.Groups[1].ToString();
                                if (partno == this.txbCDAPartNumber.Text)
                                {
                                    gridDocument.Rows.Add(new object[2] { item.MDA_DOCUMENT_ID, item.MDA_FILE_NAME });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void GetDocumentCollections()
        {
            GetDocumentData getDocument = new GetDocumentData(sessionContext, initModel, this);
            //get advice id
            Advice[] adviceArray = getDocument.GetAdviceByStationAndPN(this.txbCDAPartNumber.Text);
            if (adviceArray != null && adviceArray.Length > 0)
            {
                int iAdviceID = adviceArray[0].id;
                List<DocumentEntity> list = getDocument.GetDocumentDataByAdvice(iAdviceID);
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        string docID = item.MDA_DOCUMENT_ID;
                        string fileName = item.MDA_FILE_NAME;
                        SetDocumentControl(docID, fileName);
                        //FillGridData(fileName);
                        break;
                    }
                }
            }
        }

        private void SetDocumentControl(string docID, string fileName)
        {
            GetDocumentData documentHandler = new GetDocumentData(sessionContext, initModel, this);
            byte[] content = documentHandler.GetDocumnetContentByID(Convert.ToInt64(docID));
            if (content != null)
            {
                string path = config.MDAPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filePath = path + @"/" + fileName;
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
                Encoding.GetEncoding("gb2312");
                fs.Write(content, 0, content.Length);
                fs.Flush();
                fs.Close();
            }
        }

        private void SetDocumentControlForDoc(long documentID, string fileName)
        {
            string path = config.MDAPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string filePath = path + @"/" + fileName;
            if (!File.Exists(filePath))
            {
                GetDocumentData documentHandler = new GetDocumentData(sessionContext, initModel, this);
                byte[] content = documentHandler.GetDocumnetContentByID(documentID);
                if (content != null)
                {
                    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
                    fs.Write(content, 0, content.Length);
                    fs.Flush();
                    fs.Close();
                }
            }
            this.webBrowser1.Navigate(filePath);
        }
        #endregion

        #region Equipment
        private void InitEquipmentGrid()
        {
            if (string.IsNullOrEmpty(this.txbCDAMONumber.Text))
                return;

            EquipmentManager eqManager = new EquipmentManager(sessionContext, initModel, this);
            foreach (DataGridViewRow row in dgvEquipment.Rows)
            {
                string equipmentNo = row.Cells["EquipNo"].Value.ToString();
                string equipmentIndex = row.Cells["EquipmentIndex"].Value.ToString();
                if (string.IsNullOrEmpty(equipmentNo))
                    continue;
                int errorCode = eqManager.UpdateEquipmentData(equipmentIndex, equipmentNo, 1);
                RemoveAttributeForEquipment(equipmentNo, equipmentIndex, "attribEquipmentHasRigged");
            }

            this.dgvEquipment.Rows.Clear();
            List<EquipmentEntity> listEntity = eqManager.GetRequiredEquipmentData(this.txbCDAMONumber.Text);
            if (listEntity != null)
            {
                foreach (var item in listEntity)
                {
                    this.dgvEquipment.Rows.Add(new object[8] { LaserMark.Properties.Resources.Close, item.PART_NUMBER, item.EQUIPMENT_DESCRIPTION, "", "", "", "", "0" });
                }
            }
            this.dgvEquipment.ClearSelection();
        }

        public bool CheckEquipmentSetup()
        {
            foreach (DataGridViewRow row in this.dgvEquipment.Rows)
            {
                if (row.Cells["UsCount"].Value != null && row.Cells["UsCount"].Value.ToString().Length == 0)
                {
                    errorHandler(3, Message("msg_Equipment setup required."), "");
                    return false;
                }
            }
            return true;
        }

        public void ProcessEquipmentData(string equipmentNo)
        {
            if (!VerifyActivatedWO())
                return;
            EquipmentManager eqManager = new EquipmentManager(sessionContext, initModel, this);
            string[] values = eqManager.GetEquipmentDetailData(equipmentNo);
            string ePartNumber = "";
            string eIndex = "0";
            if (!CheckEquipmentDuplication(values, ref ePartNumber, ref eIndex))
            {
                errorHandler(3, Message("msg_The equipment") + equipmentNo + Message("msg_ has more Available states."), "");
                return;
            }
            if (!CheckEquipmentValid(ePartNumber))
            {
                errorHandler(3, Message("msg_The equipment is invalid"), "");
                return;
            }
            //check equipment number  whether need to setup?
            if (!CheckEquipmentIsExist(ePartNumber))
                return;
            //check equipment number has rigged on others station
            if (CheckEquipmentHasSetup(equipmentNo, eIndex, "attribEquipmentHasRigged"))
            {
                errorHandler(3, Message("msg_The equipment has rigged on others station."), "");
                return;
            }
            string strEquipmentIndex = eIndex;
            int errorCode = eqManager.UpdateEquipmentData(strEquipmentIndex, equipmentNo, 0);
            if (errorCode == 0)//1301 Equipment is already set up
            {
                //add attribue command the equipment is uesd
                AppendAttributeForEquipment(equipmentNo, strEquipmentIndex, "attribEquipmentHasRigged");
                EquipmentEntityExt entityExt = eqManager.GetSetupEquipmentData(equipmentNo);
                if (entityExt != null)
                {
                    entityExt.PART_NUMBER = ePartNumber;
                    entityExt.EQUIPMENT_INDEX = strEquipmentIndex;
                    SetEquipmentGridData(entityExt);
                    SetTipMessage(MessageType.OK, Message("msg_Process equipment number ") + equipmentNo + Message("msg_SUCCESS"));
                }
                if (BoardCome)
                {
                    ProcessSerialNumberData();
                }
            }
        }

        private void SetEquipmentGridData(EquipmentEntityExt entityExt)
        {
            foreach (DataGridViewRow row in this.dgvEquipment.Rows)
            {
                if (row.Cells["eqPartNumber"].Value != null && row.Cells["eqPartNumber"].Value.ToString() == entityExt.PART_NUMBER
                    && (row.Cells["EquipNo"].Value == null || row.Cells["EquipNo"].Value.ToString() == ""))
                {
                    row.Cells["NextMaintenance"].Value = DateTime.Now.AddSeconds(Convert.ToDouble(entityExt.SECONDS_BEFORE_EXPIRATION)).ToString("yyyy/MM/dd HH:mm:ss");
                    row.Cells["eqScanTime"].Value = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    row.Cells["UsCount"].Value = entityExt.USAGES_BEFORE_EXPIRATION;
                    row.Cells["EquipNo"].Value = entityExt.EQUIPMENT_NUMBER;
                    row.Cells["EquipmentIndex"].Value = entityExt.EQUIPMENT_INDEX;
                    row.Cells["eqStatus"].Value = LaserMark.Properties.Resources.ok;
                    row.Cells["eqPartNumber"].Style.BackColor = Color.FromArgb(0, 192, 0);
                }
            }
        }

        private bool CheckEquipmentIsExist(string partNumber)
        {
            foreach (DataGridViewRow row in this.dgvEquipment.Rows)
            {
                if (row.Cells["eqPartNumber"].Value != null && row.Cells["eqPartNumber"].Value.ToString() == partNumber
                    && row.Cells["EquipNo"].Value.ToString() != "")
                {
                    errorHandler(3, Message("msg_The equipment already exist."), "");
                    return false;
                }
            }
            return true;
        }

        private bool CheckEquipmentValid(string ePartNumber)
        {
            bool isValid = false;
            if (string.IsNullOrEmpty(ePartNumber))
                return false;
            foreach (DataGridViewRow item in this.dgvEquipment.Rows)
            {
                if (item.Cells["eqPartNumber"].Value.ToString() == ePartNumber)// "EQUIPMENT_STATE", "ERROR_CODE", "PART_NUMBER"
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }

        private bool CheckEquipmentDuplication(string[] values, ref string ePartNumber, ref string eIndex)
        {
            int iCount = 0;
            ePartNumber = "";
            eIndex = "0";
            for (int i = 0; i < values.Length; i += 4)
            {
                if (values[i] == "0")
                {
                    ePartNumber = values[i + 2];
                    eIndex = values[i + 3];
                    iCount++;
                }

            }
            if (iCount > 1)
                return false;
            else
                return true;
        }

        private bool CheckEquipmentHasSetup(string equipmentNumber, string equipmentIndex, string attributeCode)
        {
            bool hasSetup = false;
            GetAttributeValue getAttributeHandler = new GetAttributeValue(sessionContext, initModel, this);
            string[] values = getAttributeHandler.GetAttributeValueForAll(15, equipmentNumber, equipmentIndex, attributeCode);
            if (values != null && values.Length > 0)
            {
                hasSetup = true;
            }
            return hasSetup;
        }

        private void AppendAttributeForEquipment(string equipmentNumber, string equipmentIndex, string attributeCode)
        {
            AppendAttribute appendAttriHandler = new AppendAttribute(sessionContext, initModel, this);
            appendAttriHandler.AppendAttributeForAll(15, equipmentNumber, equipmentIndex, attributeCode, "Y");
        }

        private void RemoveAttributeForEquipment(string equipmentNumber, string equipmentIndex, string attributeCode)
        {
            RemoveAttributeValue removeAttriHandler = new RemoveAttributeValue(sessionContext, initModel, this);
            removeAttriHandler.RemoveAttributeForAll(15, equipmentNumber, equipmentIndex, attributeCode);
        }

        private bool VerifyMaterial()
        {
            bool isValid = true;
            if (this.gridSetup.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in gridSetup.Rows)
                {
                    if (row.Cells["MaterialBinNo"].Value == null || row.Cells["MaterialBinNo"].Value.ToString().Length == 0)
                    {
                        errorHandler(3, Message("msg_Material setup required"), "");
                        isValid = false;
                        break;
                    }
                    else if ((row.Cells["Qty"].Value != null && row.Cells["Qty"].Value.ToString() == "0"))
                    {
                        errorHandler(3, Message("msg_Material setup required"), "");
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }

        private bool VerifyModel()
        {
            bool isValid = true;
            if (this.ckbSmall.Checked == false && this.ckbRefSmall.Checked == false && this.rdbNoSPanel.Checked == false)
            {
                errorHandler(3, Message("msg_Please select the modle"), "");
                MessageBox.Show(this, Message("msg_Please select the modle"));
                isValid = false;
            }

            return isValid;
        }

        private bool VerifyEquipment()
        {
            bool isValid = true;
            EquipmentManager equipmentHandler = new EquipmentManager(sessionContext, initModel, this);
            int errorCode = equipmentHandler.CheckEquipmentData(this.txbCDAMONumber.Text);
            if (errorCode != 0)
            {
                errorHandler(3, Message("msg_Check equipment data error") + errorCode, "");
                return false;
            }
            foreach (DataGridViewRow item in this.dgvEquipment.Rows)
            {
                if (Convert.ToInt32(item.Cells["UsCount"].Value) <= 0)
                {
                    isValid = false;
                    item.Cells["eqPartNumber"].Style.BackColor = Color.FromArgb(255, 255, 255);
                    errorHandler(3, Message("msg_Equip_usage count cann't less then 0"), "");
                    break;
                }
                else if (Convert.ToDateTime(item.Cells["NextMaintenance"].Value) <= DateTime.Now)
                {
                    string equipmentNo = item.Cells["EquipNo"].Value.ToString();
                }
            }
            if (isValid)//continue check material expiry date
            {
                //foreach (DataGridViewRow itemM in this.gridSetup.Rows)
                //{
                //    DateTime dtExpiry = Convert.ToDateTime(itemM.Cells["ExpiryTime"].Value);
                //    if (DateTime.Now > dtExpiry)
                //    {
                //        isValid = false;
                //        errorHandler(3, Message("msg_The solder paste has expiry."), "");
                //        break;
                //    }
                //}
            }
            return isValid;
        }

        private bool VerifyActivatedWO()
        {
            bool isValid = true;
            GetCurrentWorkorder getActivatedWOHandler = new GetCurrentWorkorder(sessionContext, initModel, this);
            GetStationSettingModel stationSetting = getActivatedWOHandler.GetCurrentWorkorderResultCall();
            if (stationSetting != null && stationSetting.workorderNumber != null)
            {
                if (stationSetting.workorderNumber == this.txbCDAMONumber.Text)
                {
                    isValid = true;
                }
                else
                {
                    isValid = false;
                    errorHandler(3, Message("msg_The current activated work order has changed, please refresh"), "");
                }
            }
            return isValid;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (this.dgvEquipment.SelectedRows.Count > 0)
            {
                DataGridViewRow row = this.dgvEquipment.SelectedRows[0];
                string equipmentNo = row.Cells["EquipNo"].Value.ToString();
                string equipmentIndex = row.Cells["EquipmentIndex"].Value.ToString();
                row.Cells["NextMaintenance"].Value = "";
                row.Cells["UsCount"].Value = "";
                row.Cells["EquipNo"].Value = "";
                row.Cells["EquipmentIndex"].Value = "";
                row.Cells["eqStatus"].Value = LaserMark.Properties.Resources.Close;
                row.Cells["eqPartNumber"].Style.BackColor = Color.FromArgb(255, 255, 255);

                //Strip down equipment
                if (string.IsNullOrEmpty(equipmentNo))
                    return;
                EquipmentManager eqManager = new EquipmentManager(sessionContext, initModel, this);
                int errorCode = eqManager.UpdateEquipmentData(equipmentIndex, equipmentNo, 1);
                //remove attribute "attribEquipmentHasRigged"
                RemoveAttributeForEquipment(equipmentNo, equipmentIndex, "attribEquipmentHasRigged");
                this.dgvEquipment.ClearSelection();
            }
        }

        int iIndexItem = -1;
        private void dgvEquipment_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.dgvEquipment.Rows.Count == 0)
                    return;
                this.dgvEquipment.ContextMenuStrip = contextMenuStrip1;
                iIndexItem = ((DataGridView)sender).CurrentRow.Index;
            }
        }

        private void removeEquipment_Click(object sender, EventArgs e)
        {
            if (iIndexItem > -1)
            {
                DataGridViewRow row = this.dgvEquipment.Rows[iIndexItem];
                string equipmentNo = row.Cells["EquipNo"].Value.ToString();
                string equipmentIndex = row.Cells["EquipmentIndex"].Value.ToString();
                row.Cells["NextMaintenance"].Value = "";
                row.Cells["ScanTime"].Value = "";
                row.Cells["UsCount"].Value = "";
                row.Cells["EquipNo"].Value = "";
                row.Cells["Status"].Value = LaserMark.Properties.Resources.Close;
                row.Cells["eqPartNumber"].Style.BackColor = Color.FromArgb(255, 255, 255);

                //Strip down equipment
                if (string.IsNullOrEmpty(equipmentNo))
                    return;
                EquipmentManager eqManager = new EquipmentManager(sessionContext, initModel, this);
                int errorCode = eqManager.UpdateEquipmentData(equipmentIndex, equipmentNo, 1);
                RemoveAttributeForEquipment(equipmentNo, equipmentIndex, "attribEquipmentHasRigged");
                this.dgvEquipment.ClearSelection();
            }
        }
        #endregion
        #endregion

        #region Network status
        private string strNetMsg = "Network Connected";
        private void picNet_MouseHover(object sender, EventArgs e)
        {
            this.toolTip1.Show(strNetMsg, this.picNet);
        }

        private void AvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                this.picNet.Image = LaserMark.Properties.Resources.NetWorkConnectedGreen24x24;
                this.toolTip1.Show("Network Connected", this.picNet);
                strNetMsg = "Network Connected";
            }
            else
            {
                this.picNet.Image = LaserMark.Properties.Resources.NetWorkDisconnectedRed24x24;
                this.toolTip1.Show("Network Disconnected", this.picNet);
                strNetMsg = "Network Disconnected";
            }
        }
        #endregion

        #region Activated Wo

        /// <summary>
        /// 初始化工单类表显示
        /// </summary>
        private void InitWorkOrderList()
        {
            GetWorkOrder getWOHandler = new GetWorkOrder(sessionContext, initModel, this);

            //获取工单类表    
            DataTable dt = getWOHandler.GetAllWorkordersExt();

            DataView dv = dt.DefaultView;
            dv.Sort = "Info desc";
            dt = dv.Table;
            if (dt != null)
            {
                this.gridWorkorder.DataSource = dt;
                this.gridWorkorder.ClearSelection();
            }
            for (int i = 0; i < gridWorkorder.Rows.Count; i++)
            {
                gridWorkorder.Rows[i].Cells["columnRunId"].Value = i + 1 + "";
            }
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            try
            {
                string workorder = "";
                strShiftChecklist = "";
                bool isInitChecklist = false;
                string WorkorderPre = txbCDAMONumber.Text;
                if (this.gridWorkorder.SelectedRows.Count > 0)
                {
                    //检验是否已经做过点检
                    //if (!CheckCheckList())
                    //{
                    //    return;
                    //}
                    workorder = this.gridWorkorder.SelectedRows[0].Cells["columnWoNumber"].Value.ToString();

                    #region 镭雕机打印模式,现在都是只有1,用镭雕机打印
                    if (config.PRINTER_MODE == "1")
                    {
                        //激活工单前如果存在前面还有文本框工单号的BCI文件会删掉
                        if (workorder != txbCDAMONumber.Text)
                        {
                            string path = config.BCIPath;
                            string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }
                    else
                    {
                        if (workorder != txbCDAMONumber.Text)
                        {
                            dgvPrintSN.Rows.Clear();
                            dicSNEXT.Clear();
                            isPrintComplete = false;
                        }
                        SavePRintCount();
                    }
                    #endregion


                    ActivateWorkorder activateHandler = new ActivateWorkorder(sessionContext, initModel, this);
                    //激活工单
                    int error = activateHandler.ActivateWorkorderExt(initModel.configHandler.StationNumber, workorder, 1);//1 = Activate work order for the station only
                    if (error == 0)
                    {
                        this.txbCDAMONumber.Text = this.gridWorkorder.SelectedRows[0].Cells["columnWoNumber"].Value.ToString();
                        this.txbCDAPartNumber.Text = this.gridWorkorder.SelectedRows[0].Cells["columnPn"].Value.ToString();
                        GetCurrentWorkorder getCurrentHandler = new GetCurrentWorkorder(sessionContext, initModel, this);
                        //刷新工单
                        GetStationSettingModel model = getCurrentHandler.GetCurrentWorkorderResultCall();
                        initModel.currentSettings = model;
                        if (model != null && model.workorderNumber != null)
                        {
                            //GetNumbersOfSingleBoards getNumBoard = new GetNumbersOfSingleBoards(sessionContext, initModel, this);
                            //List<MdataGetPartData> listData = getNumBoard.GetNumbersOfSingleBoardsResultCall(model.partNumber);
                            //if (listData != null && listData.Count > 0)
                            //{
                            //    MdataGetPartData mData = listData[0];
                            //    initModel.numberOfSingleBoards = mData.quantityMultipleBoard;
                            //}
                        }

                        LoadYield();

                        //更改工单的时候触发,isInitChecklist更改会导致后面就不会去触发检查点检
                        if (workorder != WorkorderPre)
                        {
                            strShiftChecklist = "";
                            InitWorkOrderType();
                            InitShift2(WorkorderPre);
                            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                            {
                                if (!CheckShiftChange2())
                                {
                                    InitTaskData_SOCKET("开线点检;设备点检");
                                    isStartLineCheck = true;
                                }
                                else
                                {
                                    InitTaskData_SOCKET("开线点检");
                                    isStartLineCheck = true;
                                }

                            }
                            isInitChecklist = true;
                        }

                        InitSetupGrid();
                        InitWorkOrderList();
                        InitEquipmentGrid();
                        SetWorkorderGridStatus();
                        InitDocumentGrid();
                        ClearModel();
                        GetBCIInit();
                        if (config.PRINTER_MODE == "2" || config.PRINTER_MODE == "3")
                        {
                            label8.Text = initModel.currentSettings.QuantityMO.ToString();
                            this.label10.Text = ReadPRintCount(); //this.lblPass.Text;
                            rdbNoSPanel.Checked = true;
                        }

                        if (!isInitChecklist)
                        {
                            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                            {
                                InitShift2(WorkorderPre);
                                if (!CheckShiftChange2())
                                {
                                    InitTaskData_SOCKET("开线点检;设备点检");
                                }
                                else
                                {
                                    if (!ReadCheckListFile())
                                    {
                                        InitTaskData_SOCKET("开线点检");
                                        isStartLineCheck = true;
                                    }
                                }
                            }
                            else
                            {
                                InitTaskData();
                            }
                        }
                        SetTipMessage(MessageType.OK, Message("msg_Activated work order success."));
                        SetTopWindowMessage(Message("msg_Activated work order success."), "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex.Message, ex);
            }
        }

        private void btnActivateExt_Click(object sender, EventArgs e)
        {
            try
            {
                string workorder = "";
                strShiftChecklist = "";
                bool isInitChecklist = false;
                string WorkorderPre = txbCDAMONumber.Text;
                if (this.gridWorkorder.SelectedRows.Count > 0)
                {
                    //if (!CheckCheckList())
                    //{
                    //    return;
                    //}
                    workorder = this.gridWorkorder.SelectedRows[0].Cells["columnWoNumber"].Value.ToString();
                    if (config.PRINTER_MODE == "1")
                    {
                        if (workorder != txbCDAMONumber.Text)
                        {
                            string path = config.BCIPath;
                            string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }
                    else
                    {
                        if (workorder != txbCDAMONumber.Text)
                        {
                            dgvPrintSN.Rows.Clear();
                            dicSNEXT.Clear();
                            isPrintComplete = false;
                        }
                        SavePRintCount();
                    }
                    ActivateWorkorder activateHandler = new ActivateWorkorder(sessionContext, initModel, this);
                    //激活产线
                    int error = activateHandler.ActivateWorkorderExt(initModel.configHandler.StationNumber, workorder, 2);//2 = Activate work order for entire line
                    if (error == 0)
                    {
                        this.txbCDAMONumber.Text = this.gridWorkorder.SelectedRows[0].Cells["columnWoNumber"].Value.ToString();
                        this.txbCDAPartNumber.Text = this.gridWorkorder.SelectedRows[0].Cells["columnPn"].Value.ToString();
                        GetCurrentWorkorder getCurrentHandler = new GetCurrentWorkorder(sessionContext, initModel, this);
                        GetStationSettingModel model = getCurrentHandler.GetCurrentWorkorderResultCall();
                        initModel.currentSettings = model;
                        if (model != null && model.workorderNumber != null)
                        {
                            //GetNumbersOfSingleBoards getNumBoard = new GetNumbersOfSingleBoards(sessionContext, initModel, this);
                            //List<MdataGetPartData> listData = getNumBoard.GetNumbersOfSingleBoardsResultCall(model.partNumber);
                            //if (listData != null && listData.Count > 0)
                            //{
                            //    MdataGetPartData mData = listData[0];
                            //    initModel.numberOfSingleBoards = mData.quantityMultipleBoard;
                            //}
                        }
                        LoadYield();
                        if (workorder != WorkorderPre)
                        {
                            strShiftChecklist = "";
                            InitWorkOrderType();
                            InitShift2(WorkorderPre);
                            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                            {
                                if (!CheckShiftChange2())
                                {
                                    InitTaskData_SOCKET("开线点检;设备点检");
                                    isStartLineCheck = true;
                                }
                                else
                                {
                                    InitTaskData_SOCKET("开线点检");
                                    isStartLineCheck = true;
                                }
                            }
                            isInitChecklist = true;
                        }
                        InitSetupGrid();
                        InitWorkOrderList();
                        InitEquipmentGrid();
                        SetWorkorderGridStatus();
                        InitDocumentGrid();
                        ClearModel();
                        GetBCIInit();
                        if (config.PRINTER_MODE == "2" || config.PRINTER_MODE == "3")
                        {
                            label8.Text = initModel.currentSettings.QuantityMO.ToString();
                            this.label10.Text = ReadPRintCount(); //this.lblPass.Text;
                            rdbNoSPanel.Checked = true;
                        }
                        if (!isInitChecklist)
                        {
                            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                            {
                                InitShift2(WorkorderPre);
                                if (!CheckShiftChange2())
                                {
                                    InitTaskData_SOCKET("开线点检;设备点检");
                                    isStartLineCheck = true;
                                }
                                else
                                {
                                    if (!ReadCheckListFile())
                                    {
                                        InitTaskData_SOCKET("开线点检");
                                        isStartLineCheck = true;
                                    }
                                }
                            }
                            else
                            {
                                InitTaskData();
                            }
                        }
                        SetTipMessage(MessageType.OK, Message("msg_Activated work order success."));
                        SetTopWindowMessage(Message("msg_Activated work order success."), "");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex.Message, ex);
            }

        }


        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                //if (!CheckCheckList())
                //{
                //    return;
                //}
                string WorkorderPre = txbCDAMONumber.Text;
                strShiftChecklist = "";//20161215 add by qy
                GetCurrentWorkorder getCurrentHandler = new GetCurrentWorkorder(sessionContext, initModel, this);
                GetStationSettingModel model = getCurrentHandler.GetCurrentWorkorderResultCall();
                initModel.currentSettings = model;
                if (model != null && model.workorderNumber != null)
                {
                    //GetNumbersOfSingleBoards getNumBoard = new GetNumbersOfSingleBoards(sessionContext, initModel, this);
                    //List<MdataGetPartData> listData = getNumBoard.GetNumbersOfSingleBoardsResultCall(model.partNumber);
                    //if (listData != null && listData.Count > 0)
                    //{
                    //    MdataGetPartData mData = listData[0];
                    //    initModel.numberOfSingleBoards = mData.quantityMultipleBoard;
                    //}
                    bool isInitChecklist = false;
                    if (model.workorderNumber != WorkorderPre)
                    {
                        strShiftChecklist = "";
                        InitWorkOrderType();
                        InitShift2(WorkorderPre);//20161215 add by qy
                        if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                        {
                            if (!CheckShiftChange2())
                            {
                                InitTaskData_SOCKET("开线点检;设备点检");
                                isStartLineCheck = true;
                            }
                            else
                            {
                                InitTaskData_SOCKET("开线点检");
                                isStartLineCheck = true;
                            }
                        }
                        isInitChecklist = true;
                    }
                    if (config.PRINTER_MODE == "1")
                    {
                        if (model.workorderNumber != txbCDAMONumber.Text)
                        {
                            string path = config.BCIPath;
                            string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                        }
                    }
                    else
                    {
                        if (model.workorderNumber != txbCDAMONumber.Text)
                        {
                            dgvPrintSN.Rows.Clear();
                            dicSNEXT.Clear();
                            isPrintComplete = false;
                        }
                        SavePRintCount();
                    }
                    this.txbCDAMONumber.Text = model.workorderNumber;
                    this.txbCDAPartNumber.Text = model.partNumber;
                    LoadYield();
                    InitSetupGrid();
                    InitWorkOrderList();
                    SetWorkorderGridStatus();
                    InitEquipmentGrid();
                    InitDocumentGrid();
                    ShowTopWindow();
                    ClearModel();
                    GetBCIInit();
                    if (config.PRINTER_MODE == "2" || config.PRINTER_MODE == "3")
                    {
                        label8.Text = initModel.currentSettings.QuantityMO.ToString();
                        this.label10.Text = ReadPRintCount(); //this.lblPass.Text;
                        rdbNoSPanel.Checked = true;
                    }

                    if (!isInitChecklist)
                    {
                        if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                        {
                            InitShift2(WorkorderPre);
                            if (!CheckShiftChange2())
                            {
                                InitTaskData_SOCKET("开线点检;设备点检");
                                isStartLineCheck = true;
                            }
                            else
                            {
                                if (!ReadCheckListFile())
                                {
                                    InitTaskData_SOCKET("开线点检");
                                    isStartLineCheck = true;
                                }
                            }
                        }
                        else
                        {
                            InitTaskData();
                        }
                    }
                }
                else
                {
                    this.txbCDAMONumber.Text = "";
                    this.txbCDAPartNumber.Text = "";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex.Message, ex);
            }

        }

        private delegate void SetWorkorderGridStatusHandle();
        private void SetWorkorderGridStatus()
        {
            if (this.gridWorkorder.InvokeRequired)
            {
                SetWorkorderGridStatusHandle setStatusDel = new SetWorkorderGridStatusHandle(SetWorkorderGridStatus);
                Invoke(setStatusDel, new object[] { });
            }
            else
            {
                for (int i = 0; i < this.gridWorkorder.Rows.Count; i++)
                {
                    if (this.txbCDAMONumber.Text.Trim() == this.gridWorkorder.Rows[i].Cells["columnWoNumber"].Value.ToString())
                    {
                        ((DataGridViewImageCell)gridWorkorder.Rows[i].Cells["Activated"]).Value = LaserMark.Properties.Resources.ok;
                    }
                    else
                    {
                        ((DataGridViewImageCell)gridWorkorder.Rows[i].Cells["Activated"]).Value = LaserMark.Properties.Resources.Close;
                    }
                }
            }
        }
        #endregion

        #region CheckList
        private void btnAddTask_Click(object sender, EventArgs e)
        {
            int iHour = DateTime.Now.Hour;
            if (8 <= iHour && iHour <= 18)
            {
                gridCheckList.Rows.Add(new object[] { this.gridCheckList.Rows.Count + 1, DateTime.Now.ToString("yyyy/MM/dd"), "白班", "", "", "", "", "", "", "", "" });
            }
            else
            {
                gridCheckList.Rows.Add(new object[] { this.gridCheckList.Rows.Count + 1, DateTime.Now.ToString("yyyy/MM/dd"), "晚班", "", "", "", "", "", "", "", "" });
            }
            gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clResult1"].ReadOnly = true;
            gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clSeq"].ReadOnly = true;
            gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clDate"].ReadOnly = true;
            gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clShift"].ReadOnly = true;
            gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clStatus"].ReadOnly = true;
            gridCheckList.ClearSelection();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            //if (!VerifyCheckList())
            //{
            //    // errorHandler(2, Message("$msg_checklist_first"), "");
            //    return;
            //}
            CheckListsCreate();
            #region
            //if (gridCheckList.Rows.Count > 0)
            //{
            //    string targetFileName = "";
            //    string shortFileName = config.StationNumber + "_" + this.gridCheckList.Rows[0].Cells["clShift"].Value.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            //    bool isOK = CreateTemplate(shortFileName, ref targetFileName);
            //    if (isOK)
            //    {
            //        Excel.Application xlsApp = null;
            //        Excel._Workbook xlsBook = null;
            //        Excel._Worksheet xlsSheet = null;
            //        try
            //        {
            //            GC.Collect();
            //            xlsApp = new Excel.Application();
            //            xlsApp.DisplayAlerts = false;
            //            xlsApp.Workbooks.Open(targetFileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
            //            xlsBook = xlsApp.ActiveWorkbook;
            //            xlsSheet = (Excel._Worksheet)xlsBook.ActiveSheet;

            //            int iBeginIndex = 7;
            //            Excel.Range range = null;
            //            foreach (DataGridViewRow row in gridCheckList.Rows)
            //            {
            //                range = (Excel.Range)xlsSheet.Rows[iBeginIndex, Missing.Value];
            //                range.Rows.Insert(Excel.XlDirection.xlDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
            //                string strSeq = row.Cells["clSeq"].Value.ToString();
            //                string strItemName = row.Cells["clItemName"].Value.ToString();
            //                string strItemPoint = row.Cells["clPoint"].Value.ToString();
            //                string strItemStandard = row.Cells["clStandard"].Value.ToString();
            //                string strItemMethod = row.Cells["clMethod"].Value.ToString();
            //                string strItemResult = GetCheckItemResult(row.Cells["clResult1"].Value.ToString(), row.Cells["clResult2"].Value.ToString());
            //                string strCheckDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            //                string strException = row.Cells["clException"].Value == null ? "" : row.Cells["clException"].Value.ToString();
            //                string strHappendTime = row.Cells["clChangeDate"].Value == null ? "" : row.Cells["clChangeDate"].Value.ToString();
            //                string strProcessContent = row.Cells["clContent"].Value == null ? "" : row.Cells["clContent"].Value.ToString();
            //                string strProcessPersion = row.Cells["clPersion"].Value == null ? "" : row.Cells["clPersion"].Value.ToString();
            //                string strOperator = row.Cells["clOperator"].Value == null ? "" : row.Cells["clOperator"].Value.ToString();
            //                string strLeader = row.Cells["clLeader"].Value == null ? "" : row.Cells["clLeader"].Value.ToString();
            //                xlsSheet.Cells[iBeginIndex, 1] = strSeq;
            //                xlsSheet.Cells[iBeginIndex, 2] = strItemName;
            //                xlsSheet.Cells[iBeginIndex, 3] = strItemPoint;
            //                xlsSheet.Cells[iBeginIndex, 4] = strItemStandard;
            //                xlsSheet.Cells[iBeginIndex, 5] = strItemMethod;
            //                xlsSheet.Cells[iBeginIndex, 6] = strItemResult;
            //                xlsSheet.Cells[iBeginIndex, 7] = strCheckDate;
            //                xlsSheet.Cells[iBeginIndex, 8] = strException;
            //                xlsSheet.Cells[iBeginIndex, 9] = strHappendTime;
            //                xlsSheet.Cells[iBeginIndex, 10] = strProcessContent;
            //                xlsSheet.Cells[iBeginIndex, 11] = strProcessPersion;
            //                xlsSheet.Cells[iBeginIndex, 12] = strOperator;
            //                xlsSheet.Cells[iBeginIndex, 13] = strLeader;
            //                iBeginIndex++;
            //            }
            //            xlsBook.Save();
            //            errorHandler(0, "Save Production Check List success.(" + targetFileName + ")", "");
            //        }
            //        catch (Exception ex)
            //        {
            //            LogHelper.Error(ex);
            //        }
            //        finally
            //        {
            //            xlsBook.Close(false, Type.Missing, Type.Missing);
            //            xlsApp.Quit();
            //            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlsApp);
            //            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlsBook);
            //            System.Runtime.InteropServices.Marshal.ReleaseComObject(xlsSheet);

            //            xlsSheet = null;
            //            xlsBook = null;
            //            xlsApp = null;

            //            GC.Collect();
            //            GC.WaitForPendingFinalizers();
            //        }
            //    }
            //}
            #endregion
        }

        #region add by qy
        private void CheckListsCreate()
        {
            if (gridCheckList.Rows.Count > 0)
            {
                string targetFileName = "";
                string shortFileName = config.StationNumber + "_ICT_" + DateTime.Now.ToString("yyyyMM");
                bool isOK = CreateTemplate(shortFileName, ref targetFileName);
                if (isOK)
                {
                    Excel.Application xlsApp = null;
                    Excel._Workbook xlsBook = null;
                    Excel._Worksheet xlsSheet = null;
                    try
                    {
                        GC.Collect();
                        xlsApp = new Excel.Application();
                        xlsApp.DisplayAlerts = false;
                        xlsApp.Workbooks.Open(targetFileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                        xlsBook = xlsApp.ActiveWorkbook;
                        xlsSheet = (Excel._Worksheet)xlsBook.ActiveSheet;
                        int count = xlsSheet.UsedRange.Cells.Rows.Count;

                        int iBeginIndex = count;
                        Excel.Range range = null;
                        foreach (DataGridViewRow row in gridCheckList.Rows)
                        {
                            range = (Excel.Range)xlsSheet.Rows[iBeginIndex, Missing.Value];
                            range.Rows.Insert(Excel.XlDirection.xlDown, Excel.XlInsertFormatOrigin.xlFormatFromLeftOrAbove);
                            string strSeq = row.Cells["clSeq"].Value.ToString();
                            string strItemName = row.Cells["clItemName"].Value.ToString();
                            string strItemPoint = row.Cells["clPoint"].Value.ToString();
                            string strItemStandard = row.Cells["clStandard"].Value.ToString();
                            string strItemMethod = row.Cells["clMethod"].Value.ToString();
                            string strItemResult = GetCheckItemResult(row.Cells["clResult1"].Value.ToString(), row.Cells["clResult2"].Value.ToString());
                            string strCheckDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            string strShift = row.Cells["clShift"].Value.ToString();
                            string strException = row.Cells["clException"].Value == null ? "" : row.Cells["clException"].Value.ToString();
                            string strHappendTime = row.Cells["clChangeDate"].Value == null ? "" : row.Cells["clChangeDate"].Value.ToString();
                            string strProcessContent = row.Cells["clContent"].Value == null ? "" : row.Cells["clContent"].Value.ToString();
                            string strProcessPersion = row.Cells["clPersion"].Value == null ? "" : row.Cells["clPersion"].Value.ToString();
                            string strOperator = row.Cells["clOperator"].Value == null ? "" : row.Cells["clOperator"].Value.ToString();
                            string strLeader = row.Cells["clLeader"].Value == null ? "" : row.Cells["clLeader"].Value.ToString();

                            xlsSheet.Cells[iBeginIndex, 1] = iBeginIndex - 7;
                            xlsSheet.Cells[iBeginIndex, 2] = strItemName;
                            xlsSheet.Cells[iBeginIndex, 3] = strItemPoint;
                            xlsSheet.Cells[iBeginIndex, 4] = strItemStandard;
                            xlsSheet.Cells[iBeginIndex, 5] = strItemMethod;
                            xlsSheet.Cells[iBeginIndex, 6] = strItemResult;
                            xlsSheet.Cells[iBeginIndex, 7] = strShift;
                            xlsSheet.Cells[iBeginIndex, 8] = strCheckDate;
                            xlsSheet.Cells[iBeginIndex, 9] = strException;
                            xlsSheet.Cells[iBeginIndex, 10] = strHappendTime;
                            xlsSheet.Cells[iBeginIndex, 11] = strProcessContent;
                            xlsSheet.Cells[iBeginIndex, 12] = strProcessPersion;
                            xlsSheet.Cells[iBeginIndex, 13] = strOperator;
                            xlsSheet.Cells[iBeginIndex, 14] = strLeader;

                            iBeginIndex++;
                        }
                        xlsBook.Save();
                        errorHandler(0, Message("msg_Save_CheckList_Success") + ".(" + targetFileName + ")", "");
                        SetTopWindowMessage(Message("msg_Save_CheckList_Success") + ".(" + targetFileName + ")", "");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                    finally
                    {
                        xlsBook.Close(false, Type.Missing, Type.Missing);
                        xlsApp.Quit();
                        KillSpecialExcel(xlsApp);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlsApp);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlsBook);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlsSheet);
                        xlsSheet = null;
                        xlsBook = null;
                        xlsApp = null;

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
        }

        private bool CheckCheckList()
        {
            bool result = true;
            foreach (DataGridViewRow row in gridCheckList.Rows)
            {
                string status = row.Cells["clStatus"].Value.ToString();
                if (status != "OK")
                {
                    result = false;
                    errorHandler(2, Message("msg_Verify_CheckList"), "");
                    break;
                }
            }
            return result;
        }
        #endregion

        private string GetCheckItemResult(string result1, string result2)
        {
            if (string.IsNullOrEmpty(result1))
                return result2;
            if (string.IsNullOrEmpty(result2))
                return result1;
            else
                return "NA";
        }

        private void InitTaskData()
        {
            try
            {
                gridCheckList.Rows.Clear();
                Dictionary<string, List<CheckListItemEntity>> dicTask = new Dictionary<string, List<CheckListItemEntity>>();
                XDocument xdc = XDocument.Load("TaskFile.xml");
                var stationNodes = from item in xdc.Descendants("StationNumber")
                                   where item.Attribute("value").Value == config.StationNumber
                                   select item;
                XElement stationNode = stationNodes.FirstOrDefault();
                var tasks = from item in stationNode.Descendants("shift")
                            select item;
                foreach (XElement node in tasks.ToList())
                {
                    string shiftValue = GetNoteAttributeValues(node, "value");
                    List<CheckListItemEntity> itemList = new List<CheckListItemEntity>();
                    var items = from item in node.Descendants("Item")
                                select item;
                    foreach (XElement subItem in items.ToList())
                    {
                        CheckListItemEntity entity = new CheckListItemEntity();
                        entity.ItemName = GetNoteAttributeValues(subItem, "name");
                        entity.ItemPoint = GetNoteAttributeValues(subItem, "point");
                        entity.ItemStandard = GetNoteAttributeValues(subItem, "standard");
                        entity.ItemMethod = GetNoteAttributeValues(subItem, "method");
                        entity.ItemInputType = GetNoteAttributeValues(subItem, "inputType");
                        itemList.Add(entity);
                    }
                    if (!dicTask.ContainsKey(shiftValue))
                    {
                        dicTask[shiftValue] = itemList;
                    }
                }
                //init check list grid
                string strInputValue = GetNoteDescendantsValues(stationNode, "DataInputType");
                string[] strInputValues = strInputValue.Split(new char[] { ',' });
                DataTable dtInput = new DataTable();
                dtInput.Columns.Add("name");
                dtInput.Columns.Add("value");
                DataRow rowEmpty = dtInput.NewRow();
                rowEmpty["name"] = "";
                rowEmpty["value"] = "";
                dtInput.Rows.Add(rowEmpty);
                foreach (var strValues in strInputValues)
                {
                    DataRow row = dtInput.NewRow();
                    row["name"] = strValues;
                    row["value"] = strValues;
                    dtInput.Rows.Add(row);
                }
                ((DataGridViewComboBoxColumn)this.gridCheckList.Columns["clResult2"]).DataSource = dtInput;
                ((DataGridViewComboBoxColumn)this.gridCheckList.Columns["clResult2"]).DisplayMember = "Name";
                ((DataGridViewComboBoxColumn)this.gridCheckList.Columns["clResult2"]).ValueMember = "Value";

                int iHour = DateTime.Now.Hour;
                int seq = 1;
                if (8 <= iHour && iHour <= 18)
                {
                    if (dicTask.ContainsKey("白班"))
                    {
                        List<CheckListItemEntity> itemList = dicTask["白班"];
                        if (itemList != null && itemList.Count > 0)
                        {
                            foreach (var item in itemList)
                            {
                                object[] objValues = new object[11] { seq, DateTime.Now.ToString("yyyy/MM/dd"), "白班", item.ItemName, item.ItemPoint, item.ItemStandard, item.ItemMethod, "", "", "", item.ItemInputType };
                                this.gridCheckList.Rows.Add(objValues);
                                seq++;
                            }
                            SetCheckListInputStatus();
                            this.gridCheckList.ClearSelection();
                        }
                    }
                }
                else
                {
                    if (dicTask.ContainsKey("晚班"))
                    {
                        List<CheckListItemEntity> itemList = dicTask["晚班"];
                        if (itemList != null && itemList.Count > 0)
                        {
                            foreach (var item in itemList)
                            {
                                object[] objValues = new object[11] { seq, DateTime.Now.ToString("yyyy/MM/dd"), "晚班", item.ItemName, item.ItemPoint, item.ItemStandard, item.ItemMethod, "", "", "", item.ItemInputType };
                                this.gridCheckList.Rows.Add(objValues);
                                seq++;
                            }
                            SetCheckListInputStatus();
                            this.gridCheckList.ClearSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private string GetNoteAttributeValues(XElement node, string attributename)
        {
            string strValue = "";
            try
            {
                strValue = node.Attribute(attributename).Value;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return strValue;
        }

        private string GetNoteDescendantsValues(XElement node, string attributename)
        {
            string strValue = "";
            try
            {
                strValue = node.Descendants(attributename).First().Value;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            return strValue;
        }

        private string GetNoteDescendantsAttributeValues(XElement node, string nodeName, string attributeName)
        {
            string strValue = "";
            try
            {
                strValue = node.Descendants(nodeName).First().Attribute(attributeName).Value;
            }
            catch (Exception ex)
            {
                //MeasuredOctet 
                strValue = node.Descendants("RepairAction").First().Attribute("repairKey").Value;
                LogHelper.Info(node.ToString());
                LogHelper.Info("Node Name: " + nodeName);
                LogHelper.Info("Attribute Name: " + attributeName);
                LogHelper.Error(ex);
            }
            return strValue;
        }

        private void SetCheckListInputStatus()
        {
            foreach (DataGridViewRow row in this.gridCheckList.Rows)
            {
                if (row.Cells["clInputType"].Value.ToString() == "1")
                {
                    row.Cells["clResult1"].ReadOnly = true;
                }
                else if (row.Cells["clInputType"].Value.ToString() == "2")
                {
                    row.Cells["clResult2"].ReadOnly = true;
                }
                row.Cells["clSeq"].ReadOnly = true;
                row.Cells["clDate"].ReadOnly = true;
                row.Cells["clShift"].ReadOnly = true;
                row.Cells["clStatus"].ReadOnly = true;
            }
        }

        private void gridCheckList_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            if (this.gridCheckList.Columns[e.ColumnIndex].Name == "clResult1" && this.gridCheckList.Rows[e.RowIndex].Cells["clResult1"].Value != null && this.gridCheckList.Rows[e.RowIndex].Cells["clResult1"].Value.ToString() != "")
            {
                //verify the input value
                string strRegex = @"^(\d{0,9}.\d{0,9})-(\d{0,9}.\d{0,9}).*$";
                string strResult1 = this.gridCheckList.Rows[e.RowIndex].Cells["clResult1"].Value.ToString();
                string strStandard = this.gridCheckList.Rows[e.RowIndex].Cells["clStandard"].Value.ToString();
                Match match = Regex.Match(strStandard, strRegex);
                if (match.Success)
                {
                    if (match.Groups.Count > 2)
                    {
                        double iMin = Convert.ToDouble(match.Groups[1].Value);
                        double iMax = Convert.ToDouble(match.Groups[2].Value);
                        double iResult = Convert.ToDouble(strResult1);
                        if (iResult >= iMin && iResult <= iMax)
                        {
                            this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Style.BackColor = Color.FromArgb(0, 192, 0);
                            this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Value = "OK";
                        }
                        else
                        {
                            this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Style.BackColor = Color.Red;
                            this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Value = "NG";
                        }
                    }
                }
                else
                {
                    this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Style.BackColor = Color.Red;
                    this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Value = "NG";
                }
            }
            else if (this.gridCheckList.Columns[e.ColumnIndex].Name == "clResult1" && this.gridCheckList.Rows[e.RowIndex].Cells["clResult1"].Value == null)
            {
                this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Style.BackColor = Color.White;
                this.gridCheckList.Rows[e.RowIndex].Cells["clStatus"].Value = "";
            }
        }

        #region Grid ComboBox
        int iRowIndex = -1;
        private void gridCheckList_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv.CurrentCell.GetType().Name == "DataGridViewComboBoxCell" && dgv.CurrentCell.RowIndex != -1)
            {
                iRowIndex = dgv.CurrentCell.RowIndex;
                (e.Control as ComboBox).SelectedIndexChanged += new EventHandler(ComboBox_SelectedIndexChanged);
            }
        }

        public void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combox = sender as ComboBox;
            combox.Leave += new EventHandler(combox_Leave);
            try
            {
                if (combox.SelectedItem != null && combox.Text != "")
                {
                    if (OKlist.Contains(combox.Text))
                    {
                        this.gridCheckList.Rows[iRowIndex].Cells["clStatus"].Style.BackColor = Color.FromArgb(0, 192, 0);
                        this.gridCheckList.Rows[iRowIndex].Cells["clStatus"].Value = "OK";
                    }
                    else
                    {
                        this.gridCheckList.Rows[iRowIndex].Cells["clStatus"].Style.BackColor = Color.Red;
                        this.gridCheckList.Rows[iRowIndex].Cells["clStatus"].Value = "NG";
                    }
                }
                else
                {
                    this.gridCheckList.Rows[iRowIndex].Cells["clStatus"].Style.BackColor = Color.White;
                    this.gridCheckList.Rows[iRowIndex].Cells["clStatus"].Value = "";
                }
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void combox_Leave(object sender, EventArgs e)
        {
            ComboBox combox = sender as ComboBox;
            combox.SelectedIndexChanged -= new EventHandler(ComboBox_SelectedIndexChanged);
        }
        #endregion

        int iIndexCheckList = -1;
        private void gridCheckList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.gridCheckList.Rows.Count == 0)
                    return;
                this.gridCheckList.ContextMenuStrip = contextMenuStrip2;
                iIndexCheckList = ((DataGridView)sender).CurrentRow.Index;
                ((DataGridView)sender).CurrentRow.Selected = true;
            }
        }

        private bool CreateTemplate(string strFileName, ref string targetFileName)
        {
            bool bFlag = true;
            targetFileName = "";
            string filePath = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string _appDir = Path.GetDirectoryName(filePath);
            string strExportPath = _appDir + @"\CheckListFiles\";
            //临时文件目录
            if (Directory.Exists(strExportPath) == false)
            {
                Directory.CreateDirectory(strExportPath);
            }
            string strSourceFileName = strExportPath + @"CheckListTemplate.xls";
            string strTargetFileName = config.CheckListFolder + strFileName + ".xls";
            targetFileName = strTargetFileName;
            if (!Directory.Exists(config.CheckListFolder))
                Directory.CreateDirectory(config.CheckListFolder);
            if (File.Exists(targetFileName))
            {
                return true;
            }
            if (System.IO.File.Exists(strSourceFileName))
            {
                try
                {
                    System.IO.File.Copy(strSourceFileName, strTargetFileName, true);
                    //去掉文件Readonly,避免不可写
                    FileInfo file = new FileInfo(strTargetFileName);
                    if ((file.Attributes & FileAttributes.ReadOnly) > 0)
                    {
                        file.Attributes ^= FileAttributes.ReadOnly;
                    }
                }
                catch (Exception ex)
                {
                    bFlag = false;
                    LogHelper.Error(ex);
                    throw ex;
                }
            }
            else
            {
                bFlag = false;
            }

            return bFlag;
        }

        private bool VerifyCheckList()
        {
            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
            {
                //if (!CheckShiftChange2())
                //{
                //    if (this.dgvCheckListTable.Rows.Count <= 0 && dgvCheckListTable.Rows[0].Cells["tabdjclass"].Value.ToString() != "开线点检")
                //    {
                //        InitTaskData_SOCKET("开线点检");
                //    }

                //}
                foreach (DataGridViewRow row in this.dgvCheckListTable.Rows)
                {
                    if (row.Cells["tabStatus"].Value.ToString() != "OK")
                    {
                        errorHandler(2, Message("msg_Verify_CheckList"), "");
                        return false;
                    }
                }
                if (this.dgvCheckListTable.Rows.Count > 0)
                {
                    if (!Supervisor)
                    {
                        errorHandler(2, Message("msg_Superivisor_check_fail"), "");
                        return false;
                    }
                    if (!IPQC)
                    {

                        errorHandler(2, Message("msg_IPQC_check_fail"), "");
                        return false;
                    }
                }

                return true;
            }
            else
            {
                foreach (DataGridViewRow row in gridCheckList.Rows)
                {
                    if (row.Cells["clStatus"].Value.ToString() != "OK")
                    {

                        errorHandler(2, Message("msg_Verify_CheckList"), "");
                        return false;
                    }
                }
                //if (this.gridCheckList.Rows.Count > 0)
                //{
                //    if (!Supervisor)
                //    {

                //        errorHandler(2, Message("msg_Superivisor_check_fail"), "");
                //        return false;
                //    }
                //    if (!IPQC)
                //    {

                //        errorHandler(2, Message("msg_IPQC_check_fail"), "");
                //        return false;
                //    }
                //}
                return true;
            }
        }

        private void checkListAdd_Click(object sender, EventArgs e)
        {
            if (iIndexCheckList > -1)
            {
                int iHour = DateTime.Now.Hour;
                if (8 <= iHour && iHour <= 18)
                {
                    gridCheckList.Rows.Add(new object[] { this.gridCheckList.Rows.Count + 1, DateTime.Now.ToString("yyyy/MM/dd"), "白班", "", "", "", "", "", "", "", "" });
                }
                else
                {
                    gridCheckList.Rows.Add(new object[] { this.gridCheckList.Rows.Count + 1, DateTime.Now.ToString("yyyy/MM/dd"), "晚班", "", "", "", "", "", "", "", "" });
                }
                gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clResult1"].ReadOnly = true;
                gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clSeq"].ReadOnly = true;
                gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clDate"].ReadOnly = true;
                gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clShift"].ReadOnly = true;
                gridCheckList.Rows[this.gridCheckList.Rows.Count - 1].Cells["clStatus"].ReadOnly = true;
                gridCheckList.ClearSelection();
            }
        }

        private void checkListDelete_Click(object sender, EventArgs e)
        {
            if (iIndexCheckList > -1)
            {
                this.gridCheckList.Rows.RemoveAt(iIndexCheckList);
                int seq = 1;
                foreach (DataGridViewRow row in this.gridCheckList.Rows)
                {
                    row.Cells["clSeq"].Value = seq;
                    seq++;
                }
                this.gridCheckList.ClearSelection();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        private static void KillSpecialExcel(Excel.Application m_objExcel)
        {
            try
            {
                if (m_objExcel != null)
                {
                    int lpdwProcessId;
                    GetWindowThreadProcessId(new IntPtr(m_objExcel.Hwnd), out lpdwProcessId);
                    System.Diagnostics.Process.GetProcessById(lpdwProcessId).Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region PFC
        private bool ProcessSerialNumberData()
        {
            bool isValid = true;
            if (!VerifyModel())
            {
                return false;
            }

            if (!VerifyActivatedWO() || !VerifyCheckList() || !VerifyEquipment() || !VerifyMaterial())
            {
                return false;
            }
            else
            {
                int qtyFlag = 0; //1 剩余数量小于拼板数量
                //add by qy(check pass>wo qty)
                int lqty = initModel.currentSettings.QuantityMO - Convert.ToInt32(this.lblPass.Text);
                if (lqty <= 0)
                {
                    errorHandler(3, Message("msg_Pass can not more than woqty"), "");
                    return false;
                }
                if (lqty < initModel.numberOfSingleBoards)
                {
                    //errorHandler(3, Message("msg_less than the number of boards"), "");
                    //return false;
                    DialogResult dr = MessageBox.Show(this, Message("msg_Current qty less than numberOfSingleBoards_do you want to continue"), Message("msg_warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.OK)
                    {
                        qtyFlag = 1;
                    }
                    else
                    {
                        return false;
                    }

                }

                string wo = GetWorkOrderValue();
                bool b1 = true;// GenerateBJIFile(wo);
                bool b2 = true;
                bool b3 = true;
                //edit by qy
                if (rdbNoSPanel.Checked || ckbSmall.Checked) //只小板或大板
                    b3 = GenerateBCIFileExt(wo, qtyFlag, lqty);
                else  //有大板有小板
                    b2 = GenerateBCIFile(wo, qtyFlag, lqty);

                if (!b1 || !b2 || !b3)
                    return false;
            }
            return isValid;
        }

        bool BoardCome = false;
        private object _lock = new Object();
        DateTime PFCStartTime = DateTime.Now;
        public void ProcessPFCMessage(string pfcMsg)
        {
            //#!PONGCRLF
            //#!BCTOPBARCODECRLF
            //#!BCBOTTOMBARCODECRLF
            //#!BOARDAVCRLF
            //#!TRANSFERBARCODECRLF
            lock (_lock)
            {
                if (!VerifyCheckList())
                {
                    return;
                }
                //errorHandler(0, "Receive message from PFC " + pfcMsg.TrimEnd(), "");
                SetConnectionText(0, "Receive message from PFC " + pfcMsg.TrimEnd());
                if (pfcMsg.TrimEnd() != "#!PONG")
                    LogHelper.Info("Receive message from PFC " + pfcMsg.TrimEnd());
                if (pfcMsg.Length >= 10)
                {
                    bool isOK = true;
                    string messageType = pfcMsg.Substring(2, 8).TrimEnd();
                    switch (messageType)
                    {
                        case "PONG":
                            PFCStartTime = DateTime.Now;
                            break;
                        case "BCTOP":
                            //string serialNumber = pfcMsg.Substring(10).TrimEnd();
                            ////isOK = ProcessSerialNumberForSunLG(serialNumber);
                            //if (isOK)
                            //{
                            //    SendMsessageToPFC(PFCMessage.GO, serialNumber);
                            //}
                            break;
                        case "BCBOTTOM":
                            //string serialNumber1 = pfcMsg.Substring(10).TrimEnd();
                            ////isOK = ProcessSerialNumberForSunLG(serialNumber1);
                            //if (isOK)
                            //{
                            //    SendMsessageToPFC(PFCMessage.GO, serialNumber1);
                            //}
                            break;
                        case "BOARDAV"://todo
                            BoardCome = true;
                            if (config.BAD_BOARD_AUTO_RESET.ToUpper() == "ENABLE")
                            {
                                if (ckbIsNeedScrap.Checked)
                                {
                                    errorHandler(3, Message("msg_manual scrap please click ok button"), "");
                                    return;
                                }
                            }
                            else
                            {
                                if (ckbIsNeedScrap.Checked)
                                {
                                    if (!isConfirmScrap)
                                    {
                                        errorHandler(3, Message("msg_manual scrap please click ok button"), "");
                                        return;
                                    }
                                }
                            }

                            this.Invoke(new MethodInvoker(delegate
                            {
                                if (initModel.numberOfSingleBoards == 0)
                                {
                                    errorHandler(3, Message("msg_Material setup required"), "");
                                    //return;
                                }

                                isOK = ProcessSerialNumberData();//更新流水号
                            }));
                            if (isOK)
                            {
                                SendMsessageToPFC(PFCMessage.GO, "");
                                BoardCome = false;
                            }
                            break;
                        case "TRANSFER":
                            string serialNumber2 = ""; //pfcMsg.Substring(10).TrimEnd();
                            SendMsessageToPFC(PFCMessage.COMPLETE, serialNumber2);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        MessageBox.Show(this, "Receive message length less then 10.");
                    }));
                }
            }
        }

        private object _lockSend = new Object();
        public void SendMsessageToPFC(PFCMessage msgType, string serialNumber)
        {
            lock (_lockSend)
            {
                //#!PINGCRLF
                //#!GOBARCODECRLF
                //#!COMPLETEBARCODECRLF
                string prefix = "#!";
                string suffix = HexToStr1("0D") + HexToStr1("0A");
                string sendMessage = "";
                switch (msgType)
                {
                    case PFCMessage.PING:
                        sendMessage = prefix + PFCMessage.PING.ToString().PadRight(8, ' ') + suffix;
                        break;
                    case PFCMessage.GO:
                        sendMessage = prefix + PFCMessage.GO.ToString().PadRight(8, ' ') + serialNumber + suffix;
                        break;
                    case PFCMessage.COMPLETE:
                        sendMessage = prefix + PFCMessage.COMPLETE.ToString().PadRight(8, ' ') + serialNumber + suffix;
                        break;
                    case PFCMessage.CONFIRM:
                        sendMessage = prefix + PFCMessage.CONFIRM.ToString().PadRight(8, ' ') + serialNumber + suffix;
                        break;
                    default:
                        sendMessage = prefix + PFCMessage.PING.ToString().PadRight(8, ' ') + suffix;
                        break;
                }
                //send message through socket
                try
                {
                    if (DateTime.Now.Subtract(PFCStartTime).Seconds >= 20)
                    {
                        cSocket.send(prefix + PFCMessage.PING.ToString().PadRight(8, ' ') + suffix);
                        PFCStartTime = DateTime.Now;
                        Thread.Sleep(1000);
                    }
                    bool isOK = cSocket.send(sendMessage);
                    if (isOK)
                    {
                        //errorHandler(1, "Send message to PFC:" + sendMessage.TrimEnd(), "");
                        SetConnectionText(0, "Send message to PFC:" + sendMessage.TrimEnd());
                    }
                    else
                    {
                        //errorHandler(2, "Send message to PFC:" + sendMessage.TrimEnd(), "");
                        SetConnectionText(1, "Send message to PFC:" + sendMessage.TrimEnd());
                        bool isConnectOK = cSocket.connect(config.IPAdress, config.Port);
                        if (isConnectOK)
                        {
                            isOK = cSocket.send(sendMessage);
                            if (isOK)
                            {
                                SetConnectionText(0, "Send message to PFC:" + sendMessage.TrimEnd());
                            }
                            else
                            {
                                SetConnectionText(1, "Send message to PFC:" + sendMessage.TrimEnd());
                            }
                        }
                        else
                        {
                            SetConnectionText(1, "Conncet to PFC error");
                        }
                    }
                }
                catch (Exception ex)
                {
                    cSocket.send(prefix + PFCMessage.PING.ToString().PadRight(8, ' ') + suffix);
                    bool isOK = cSocket.send(sendMessage);
                    if (isOK)
                    {
                        //errorHandler(0, "Send message to PFC:" + sendMessage.TrimEnd(), "");
                        SetConnectionText(1, "Send message to PFC:" + sendMessage.TrimEnd());
                    }
                    else
                    {
                        SetConnectionText(1, "Send message to PFC:" + sendMessage.TrimEnd());
                    }
                    LogHelper.Error(ex.Message, ex);
                }
            }
        }

        public static string HexToStr1(string mHex) // 返回十六进制代表的字符串
        {
            mHex = mHex.Replace(" ", "");
            if (mHex.Length <= 0) return "";
            byte[] vBytes = new byte[mHex.Length / 2];
            for (int i = 0; i < mHex.Length; i += 2)
                if (!byte.TryParse(mHex.Substring(i, 2), NumberStyles.HexNumber, null, out vBytes[i / 2]))
                    vBytes[i / 2] = 0;
            return ASCIIEncoding.Default.GetString(vBytes);
        }

        /// <summary>
        /// 持续向服务器发送PING连接请求
        /// </summary>
        public void GetTimerStart()
        {
            // 循环间隔时间(1分钟)
            CheckConnectTimer.Interval = 60 * 1000;
            // 允许Timer执行
            CheckConnectTimer.Enabled = true;
            // 定义回调
            CheckConnectTimer.Elapsed += new ElapsedEventHandler(CheckConnectTimer_Elapsed);
            // 定义多次循环
            CheckConnectTimer.AutoReset = true;
        }

        private void CheckConnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendMsessageToPFC(PFCMessage.PING, "");
        }
        #endregion

        #region add by qy
        private bool GenerateBCIFileExt(string fileName, int qtyFlag, int ActnumberofBoards)
        {
            try
            {
                string path = config.BCIPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filePath = path + @"/" + fileName + ".bci";
                if (File.Exists(filePath))
                {
                    //errorHandler(3, Message("msg_BCI is exist"), "");
                    LogHelper.Error("BCI file has not deleted.");
                    return true;
                }
                dicSN = new Dictionary<string, string>();
                //string fristRow = string.Format(@"job||{0}||panelcode", GetWorkOrderValue());
                string fristRow = "panel||||";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(fristRow);
                panelSN = GenerateSerialNumber();
                if (this.rdbNoSPanel.Checked == true)
                {
                    sb.AppendLine(panelSN);
                }

                //测试组装AA没有上料,先模拟连扳数为30      郑培聪     20170917
                initModel.numberOfSingleBoards = 30;

                for (int i = 1; i <= initModel.numberOfSingleBoards; i++)
                {
                    string tempSN = panelSN + i.ToString().PadLeft(3, '0');

                    if (this.ckbSmall.Checked == true)
                    {
                        if (qtyFlag == 1)
                        {
                            if (i <= ActnumberofBoards)
                                sb.AppendLine(tempSN); //+ "|" + panelSN + "|"
                        }
                        else
                        {
                            sb.AppendLine(tempSN); //+ "|" + panelSN + "|"
                        }
                    }

                    dicSN[tempSN] = i.ToString();
                    dicSNEXT[panelSN] = dicSN;
                }

                //if exist delete first
                //if (File.Exists(filePath))
                //{
                //    string backupPath = path + @"/Complete/";
                //    string backupFilePath = backupPath + fileName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bci";
                //    if (!Directory.Exists(backupPath))
                //        Directory.CreateDirectory(backupPath);
                //    File.Copy(filePath, backupFilePath, true);
                //    File.Delete(filePath);
                //}
                //delete file in finished folder 
                DirectoryInfo di = Directory.GetParent(path);
                string deletePath = di.FullName + @"\finished\";
                string deleteFilePath = deletePath + fileName + ".bci";
                if (File.Exists(deleteFilePath))
                {
                    string backupPath = di.FullName + @"\Complete\";
                    string backupFilePath = backupPath + fileName + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bci";
                    if (!Directory.Exists(backupPath))
                        Directory.CreateDirectory(backupPath);
                    File.Copy(deleteFilePath, backupFilePath, true);
                    File.Delete(deleteFilePath);
                }

                FileStream fs = new FileStream(filePath, FileMode.Create);
                byte[] bt = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bt, 0, bt.Length);
                fs.Flush();
                fs.Close();
                errorHandler(0, "Generate BCI file success", "");
                SetTopWindowMessage("Generate BCI file success", "");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }

        private void rdbNoSPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (config.PRINTER_MODE == "1")
            {
                if (rdbNoSPanel.Checked == true)
                {
                    this.ckbSmall.Checked = false;
                    this.ckbRefSmall.Checked = false;
                    DialogResult dr = MessageBox.Show(Message("msg_Do you confirm no small plate"), Message("msg_warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.OK)
                    {
                        string path = config.BCIPath;
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                        if (File.Exists(filePath))
                        {
                            string[] lines = File.ReadAllLines(filePath);
                            if (lines.Count() > 2)
                                File.Delete(filePath);
                        }
                    }
                    else
                    {
                        this.rdbNoSPanel.Checked = false;
                    }
                }

            }
        }
        private void ckbSmall_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbSmall.Checked == true)
            {
                this.rdbNoSPanel.Checked = false;
                this.ckbRefSmall.Checked = false;
                DialogResult dr = MessageBox.Show(Message("msg_Do you confirm no ref plate"), Message("msg_warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.OK)
                {
                    string path = config.BCIPath;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                    if (File.Exists(filePath))
                    {
                        string[] lines = File.ReadAllLines(filePath);
                        if (lines.Count() <= 2 || lines[1].Contains("|"))
                            File.Delete(filePath);
                    }
                }
                else
                {
                    this.ckbSmall.Checked = false;
                }
            }
        }

        private void ckbRefSmall_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbRefSmall.Checked == true)
            {
                this.rdbNoSPanel.Checked = false;
                this.ckbSmall.Checked = false;
                DialogResult dr = MessageBox.Show(Message("msg_Do you confirm all plate"), Message("msg_warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.OK)
                {
                    string path = config.BCIPath;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                    if (File.Exists(filePath))
                    {
                        string[] lines = File.ReadAllLines(filePath);
                        if (!lines[1].Contains("|"))
                            File.Delete(filePath);
                    }
                }
                else
                {
                    this.ckbRefSmall.Checked = false;
                }
            }
        }

        private void ClearModel()
        {
            this.rdbNoSPanel.Checked = false;
            this.ckbRefSmall.Checked = false;
            this.ckbSmall.Checked = false;
        }

        /// <summary>
        /// 根据BCI文件将BCI文件的数据读取到内存中,以便后面解析trace文件的时候可以读取到
        /// </summary>
        private void GetBCIInit()
        {
            if (config.PRINTER_MODE == "1")
            {
                string path = config.BCIPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filePath = path + @"/" + this.txbCDAMONumber.Text + ".bci";
                if (File.Exists(filePath))
                {
                    dicSN = new Dictionary<string, string>();
                    string[] lines = File.ReadAllLines(filePath);
                    for (int i = 1; i <= lines.Count() - 1; i++)
                    {
                        if (lines[i].Contains("|")) //大小板都有
                        {
                            string[] datas = lines[i].Split('|');
                            string spanel = datas[0];
                            string refpanel = datas[1];
                            dicSN[spanel] = i.ToString();
                            dicSNEXT[refpanel] = dicSN;
                            continue;
                        }
                        if (lines[i].Length > this.txbCDAMONumber.Text.Length + 4)//小板
                        {
                            string spanel = lines[i];
                            string refpanle = spanel.Substring(0, spanel.Length - 3);
                            dicSN[spanel] = i.ToString();
                            dicSNEXT[refpanle] = dicSN;
                            continue;
                        }
                        if (lines[i].Length == this.txbCDAMONumber.Text.Length + 4)//大板
                        {
                            string refpanel = lines[i];

                            for (int j = 1; j <= initModel.numberOfSingleBoards; j++)
                            {
                                string tempSN = refpanel + j.ToString().PadLeft(3, '0');

                                dicSN[tempSN] = j.ToString();
                                dicSNEXT[refpanel] = dicSN;
                            }
                            continue;
                        }
                    }
                }
            }

        }
        #endregion

        #region
        string OKlist = "";
        string NGlist = "";
        private void InitCheckResultMapping()
        {
            string[] LineList = File.ReadAllLines("CheckResultMappingFile.txt", Encoding.Default);
            foreach (var line in LineList)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    string[] strs = line.Split(new char[] { ';' });
                    if (strs[0] == "OK")
                    {
                        OKlist = OKlist + "," + strs[1];
                    }
                    else
                    {
                        NGlist = NGlist + "," + strs[1];
                    }
                }
            }
        }
        #endregion

        #region printer ZPL
        private bool UploadSerialnumber()
        {
            bool isValid = true;
            //if (!VerifyModel())
            //{
            //    return false;
            //}

            //if (!VerifyActivatedWO() || !VerifyCheckList() || !VerifyEquipment() || !VerifyMaterial())
            //{
            //    return false;
            //}
            //else
            //{
            int qtyFlag = 0; //1 剩余数量小于拼板数量
            //add by qy(check pass>wo qty)
            int lqty = Convert.ToInt32(label8.Text) - Convert.ToInt32(this.lblPass.Text);
            if (lqty <= 0)
            {
                errorHandler(3, Message("msg_Pass can not more than woqty"), "");
                return false;
            }
            if (lqty < initModel.numberOfSingleBoards)
            {
                //errorHandler(3, Message("msg_less than the number of boards"), "");
                //return false;
                DialogResult dr = MessageBox.Show(this, Message("msg_Current qty less than numberOfSingleBoards_do you want to continue"), Message("msg_warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.OK)
                {
                    qtyFlag = 1;
                }
                else
                {
                    return false;
                }

            }

            string wo = GetWorkOrderValue();
            bool b1 = true;
            //edit by qy
            b1 = GeneratePrint(wo, qtyFlag, lqty);
            if (b1)
            {
                if (!AssingAndUploadSerialnumber())
                    isValid = false;
            }
            //}
            return isValid;
        }
        private bool GeneratePrint(string fileName, int qtyFlag, int ActnumberofBoards)
        {
            try
            {
                dicSNEXT = new Dictionary<string, Dictionary<string, string>>();
                dicSN = new Dictionary<string, string>();
                panelSN = GenerateSerialNumber();
                for (int i = 1; i <= initModel.numberOfSingleBoards; i++)
                {
                    string tempSN = panelSN + i.ToString().PadLeft(3, '0');

                    dicSN[tempSN] = i.ToString();
                    dicSNEXT[panelSN] = dicSN;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }
        private bool AssingAndUploadSerialnumber()
        {
            try
            {
                string refSN = "";
                List<SerialNumberData> snList = new List<SerialNumberData>();
                List<string> serialNumberArray = new List<string>();
                foreach (var panleSN in dicSNEXT.Keys)
                {
                    refSN = panleSN;
                    LogHelper.Info("refSN:" + panleSN);
                    if (dicSN.Count == 1)
                    {
                        SerialNumberData snData = new SerialNumberData(refSN, "1", "");
                        snList.Add(snData);
                        serialNumberArray.Add("0");//"ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE"
                        serialNumberArray.Add(refSN);
                        serialNumberArray.Add("1");
                        //edit by qy
                        serialNumberArray.Add("0");
                    }
                    else
                    {
                        Dictionary<string, string> dicSN2 = dicSNEXT[panleSN];
                        foreach (var itemSN in dicSN2.Keys)
                        {
                            LogHelper.Info("itemSN:" + itemSN);
                            SerialNumberData snData = new SerialNumberData(itemSN, dicSN2[itemSN], "");
                            snList.Add(snData);
                            serialNumberArray.Add("0");//"ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE"
                            serialNumberArray.Add(itemSN);
                            serialNumberArray.Add(dicSN2[itemSN]);
                            //edit by qy
                            int lqty = initModel.currentSettings.QuantityMO - Convert.ToInt32(this.lblPass.Text);
                            if (lqty < initModel.numberOfSingleBoards && Convert.ToInt32(dicSN2[itemSN]) > lqty)
                            {
                                serialNumberArray.Add("2");
                            }
                            else
                            {
                                serialNumberArray.Add("0");
                            }
                        }
                    }
                    break;
                }
                int iProcessLayer = -1;
                CommonFunction commonHandler = new CommonFunction(sessionContext, initModel, this);
                iProcessLayer = commonHandler.GetProcessLayerByWO(GetWorkOrderValue(), config.StationNumber);

                AssignSerialNumber assignHandler = new AssignSerialNumber(sessionContext, initModel, this);
                int assignCode = -1;

                if (dicSN.Count == 1)
                {
                    //assignCode = assignHandler.AssignSerialNumberResultCallForSingle(snList.ToArray(), GetWorkOrderValue(), iProcessLayer);
                    assignCode = assignHandler.AssignSerialNumberResultCallForSingle2(new SerialNumberData[] { }, GetWorkOrderValue(), iProcessLayer, refSN);
                }
                else
                {
                    assignCode = assignHandler.AssignSerialNumberResultCallForMul(refSN, snList.ToArray(), GetWorkOrderValue(), iProcessLayer);
                }
                if (assignCode == 0 || assignCode == -206)
                {
                    //update serial numer state & pass station
                    UploadProcessResult uploadHandler = new UploadProcessResult(sessionContext, initModel, this);
                    int uploadCode = uploadHandler.UploadProcessResultCall(serialNumberArray.ToArray(), iProcessLayer);
                    if (uploadCode == 0 || uploadCode == 210)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            LoadYield();
                        }));
                        //consume material after upload
                        UpdateGridDataAfterUploadState(refSN);
                        errorHandler(0, Message("msg_Process file success") + refSN, "");
                    }
                    else
                    {
                        errorHandler(3, Message("msg_Upload serial numebr state error") + refSN, "");
                        return false;
                    }
                }
                else
                {
                    errorHandler(3, Message("msg_Assign serial numebr error") + refSN, "");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return false;
            }
        }

        #region PRINT
        private void PrintLabelExt(List<string> serialnumberlist)
        {
            //ZblLabBuild LabBuild = new ZblLabBuild();
            //string content = LabBuild.BuildLabContent(this.txtPartNumber.Text, this.txtLotNumber.Text, this.txtDateCode.Text, this.txtQty.Text);
            this.Invoke(new MethodInvoker(delegate
            {
                if (string.IsNullOrEmpty(TemplateText))
                {
                    TemplateText = ReadLabelTemplate(this.combLabel.SelectedValue.ToString());
                }
                string content = GeneratePrintString(TemplateText, serialnumberlist);
                Byte[] bytes = Encoding.Default.GetBytes(content);
                try
                {
                    if (config.PrintSerialPort != "")
                    {
                        USBControl.comPrint(config.PrintSerialPort, bytes);
                    }
                    else
                    {
                        //斑马打印机
                        ZebraPrintHelper.SendBytesToPrinter(this.cbxPrinter.Text, bytes);
                        //USBControl.SendStringToPrinter(this.cbxPrinter.Text, content);
                    }

                    errorHandler(0, Message("msg_Print success"), "");
                }
                catch (Exception ex)
                {
                    errorHandler(0, Message("msg_Print fail"), "");
                    LogHelper.Error(ex);
                }
            }));
        }

        string TemplateText = "";
        private void cmbLabelTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.combLabel.SelectedIndex > -1)
            {
                string filePath = this.combLabel.SelectedValue.ToString();
                if (filePath != "System.Data.DataRowView")
                {
                    TemplateText = ReadLabelTemplate(filePath);
                }
            }
        }

        private void InitPrinterList()
        {
            //获取本地连接打印机列表加载到下拉框中
            PrinterSettings.StringCollection list = PrinterSettings.InstalledPrinters;
            foreach (string pkInstalledPrinters in list)
            {
                cbxPrinter.Items.Add(pkInstalledPrinters);
                //本地默认的打印机为默认选择项
                PrintDocument prtdoc = new PrintDocument();
                string strDefaultPrinter = prtdoc.PrinterSettings.PrinterName;//获取默认的打印机名 
                if (pkInstalledPrinters == strDefaultPrinter)
                //把本地默认打印机设为缺省值 
                {
                    cbxPrinter.SelectedIndex = cbxPrinter.Items.IndexOf(pkInstalledPrinters);
                }
            }
        }

        private void InitPrintLabelTemplate()
        {
            string filePath = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string _appDir = config.LABEL_TEMPLATE_PATH; //Path.GetDirectoryName(filePath) + @"\LabelTemplate\";
            if (Directory.Exists(_appDir))
            {
                string[] filePaths = Directory.GetFiles(_appDir);
                if (filePaths != null && filePaths.Length > 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("name");
                    dt.Columns.Add("value");
                    foreach (var subFilePath in filePaths)
                    {
                        DataRow row = dt.NewRow();
                        string fileName = Path.GetFileName(subFilePath);
                        Match match = Regex.Match(fileName, config.LABEL_TEMPLATE_FILE);
                        if (match.Success)
                        {
                            row["name"] = fileName;
                            row["value"] = subFilePath;
                            dt.Rows.Add(row);
                        }
                    }
                    this.combLabel.DataSource = dt;
                    this.combLabel.DisplayMember = "name";
                    this.combLabel.ValueMember = "value";

                    this.combLabel.Text = config.DEFAULT_LABLE;
                }
            }
        }

        private string ReadLabelTemplate(string filePath)
        {
            string[] contents = File.ReadAllLines(filePath, Encoding.Default);
            StringBuilder sb = new StringBuilder();
            if (contents != null && contents.Length > 0)
            {
                foreach (var item in contents)
                {
                    sb.AppendLine(item);
                }
            }
            return sb.ToString();
        }

        private string GeneratePrintString(string text, List<string> serialnumberlist)
        {
            for (int i = 0; i < serialnumberlist.Count; i++)
            {
                if (text.Contains(@"" + "{RefSN" + (i + 1) + "}"))//serialnumber
                {
                    text = text.Replace(@"" + "{RefSN" + (i + 1) + "}", serialnumberlist[i]);
                }
            }
            if (serialnumberlist.Count < Convert.ToInt32(config.LABEL_QTY))
            {
                int left = Convert.ToInt32(config.LABEL_QTY) - serialnumberlist.Count;
                for (int j = 0; j < left; j++)
                {
                    int snpos = Convert.ToInt32(config.LABEL_QTY) + left - j + 1;
                    if (text.Contains(@"" + "{RefSN" + snpos + "}"))//serialnumber
                    {
                        text = text.Replace(@"" + "{RefSN" + snpos + "}", "");
                    }
                }
            }
            return text;
        }
        #endregion
        private bool VerifyRefSNListExist(string serilanumber)
        {
            bool isValid = false;
            foreach (var item in RefSNlist)
            {
                if (config.PRINTER_MODE == "2")
                {
                    if (serilanumber == item.ToString())
                    {
                        isValid = true;
                        break;
                    }
                }
                else if (config.PRINTER_MODE == "3")
                {
                    if (serilanumber == item.Split(';')[1])
                    {
                        isValid = true;
                        break;
                    }
                }
            }
            return isValid;
        }

        private bool VerifygridPrintSNExist(string serilanumber)
        {
            bool isValid = false;
            foreach (DataGridViewRow row in this.dgvPrintSN.Rows)
            {
                if (serilanumber == row.Cells["PSerialNumber"].Value.ToString())
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }
        List<string> RefSNlist = new List<string>();
        private void m_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate
           {
               //验证是否勾选大板,小板
               if (!VerifyModel())
               {
                   isPrintComplete = false;
                   return;
               }
               //验证工单,点检,设备,上料的状态
               if (!VerifyActivatedWO() || !VerifyCheckList() || !VerifyEquipment() || !VerifyMaterial())
               {
                   isPrintComplete = false;
                   return;
               }
           }));
            string endsendmessage = "";
            try
            {
                if (!isPrintComplete)
                    return;
                endsendmessage = "";
                int boardquatity = initModel.numberOfSingleBoards;//连扳数

                #region 暂停以后如果还有未打印完的先接着打印
                if (dicSNEXT != null && dicSNEXT.Keys.Count > 0)
                {
                    #region 打印大板和小板
                    if (ckbRefSmall.Checked)
                    {
                        foreach (var refSN in new List<string>(dicSNEXT.Keys))
                        {
                            if (!isPrintComplete)//停止就不打印剩下的标签
                                break;
                            if (config.PRINTER_MODE == "2")
                            {
                                if (!VerifyRefSNListExist(refSN.ToString()))
                                    RefSNlist.Add(refSN.ToString());
                            }
                            else if (config.PRINTER_MODE == "3")
                            {
                                if (boardquatity > 1)
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;0;1;1]");
                                }
                                else
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;1;1;1]");
                                }

                            }
                            if (!VerifygridPrintSNExist(refSN))
                                AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, refSN });
                            Dictionary<string, string> dicSN2 = dicSNEXT[refSN];
                            foreach (var itemSN in new List<string>(dicSN2.Keys))
                            {
                                if (!isPrintComplete)//停止就不打印剩下的标签
                                    break;
                                if (config.PRINTER_MODE == "2")
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add(itemSN.ToString());
                                }
                                else if (config.PRINTER_MODE == "3")
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + itemSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";1;" + dicSN2[itemSN.ToString()] + ";1;1]");
                                }
                                if (!VerifygridPrintSNExist(itemSN))
                                    AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, itemSN });
                                dicSNEXT[refSN.ToString()].Remove(itemSN);
                            }
                        }
                    }
                    #endregion

                    #region 打印小板
                    if (ckbSmall.Checked)
                    {
                        foreach (var refSN in new List<string>(dicSNEXT.Keys))
                        {
                            if (!isPrintComplete)//停止就不打印剩下的标签
                                break;
                            Dictionary<string, string> dicSN2 = dicSNEXT[refSN];
                            foreach (var itemSN in new List<string>(dicSN2.Keys))
                            {
                                if (!isPrintComplete)//停止就不打印剩下的标签
                                    break;
                                if (config.PRINTER_MODE == "2")
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add(itemSN.ToString());
                                }

                                else if (config.PRINTER_MODE == "3")
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + itemSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";1;" + dicSN2[itemSN.ToString()] + ";1;1]");
                                }
                                if (!VerifygridPrintSNExist(itemSN))
                                    AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, itemSN });
                                dicSNEXT[refSN.ToString()].Remove(itemSN);
                            }
                        }
                    }
                    #endregion

                    #region 打印大板
                    if (rdbNoSPanel.Checked)
                    {
                        foreach (var refSN in new List<string>(dicSNEXT.Keys))
                        {
                            if (!isPrintComplete) //停止就不打印剩下的标签
                                break;
                            if (config.PRINTER_MODE == "2")
                            {
                                if (!VerifyRefSNListExist(refSN.ToString()))
                                    RefSNlist.Add(refSN.ToString());
                            }
                            else if (config.PRINTER_MODE == "3")
                            {
                                if (boardquatity > 1)
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;0;1;1]");
                                }
                                else
                                {
                                    if (!VerifyRefSNListExist(refSN.ToString()))
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;1;1;1]");
                                }
                            }
                            if (!VerifygridPrintSNExist(refSN))
                                AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, refSN });
                            dicSNEXT[refSN.ToString()].Clear();
                        }
                    }
                    #endregion
                }

                if (RefSNlist.Count == Convert.ToInt32(config.LABEL_QTY))
                {
                    string regular = @"^[1-9]\d*$";
                    Match match = Regex.Match(this.txtPrintCopies.Text, regular);
                    if (match.Success)
                    {
                        for (int j = 0; j < Convert.ToInt32(match.ToString()); j++)
                        {
                            if (config.PRINTER_MODE == "2")
                            {
                                PrintLabelExt(RefSNlist);
                            }
                            else if (config.PRINTER_MODE == "3")
                            {
                                string bodymessage = "";
                                foreach (var outputdata in RefSNlist)
                                {
                                    if (bodymessage == "")
                                        bodymessage = outputdata;
                                    else
                                        bodymessage = bodymessage + ";" + outputdata;
                                }
                                endsendmessage = "{outputSNToTable;" + bodymessage + "}";
                                string returnmessage = cSocket2.SendData(endsendmessage);
                                if (returnmessage == null)
                                {
                                    bool isOK = cSocket2.connect(config.IPAdress, config.Port);
                                    if (!isOK)
                                    {
                                        isPrintComplete = false;
                                        return;
                                    }
                                    else
                                    {
                                        returnmessage = cSocket2.SendData(endsendmessage);
                                        if (returnmessage != null && returnmessage != "")
                                        {
                                            string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                                            string result = splits[1];
                                            string message = splits[2];
                                            if (result == "0")
                                                errorHandler(0, message, "");
                                            else
                                                errorHandler(2, message, "");
                                        }
                                    }
                                }
                                else if (returnmessage != null && returnmessage != "")
                                {
                                    string[] splits = returnmessage.Split(';');
                                    string result = splits[1];
                                    string message = splits[2];
                                    if (result == "0")
                                        errorHandler(0, message, "");
                                    else
                                        errorHandler(2, message, "");
                                }
                            }
                            this.Invoke(new MethodInvoker(delegate
                            {
                                this.label10.Text = Convert.ToString((Convert.ToInt32(this.label10.Text) + RefSNlist.Count));
                            }));
                        }
                        RefSNlist.Clear();
                    }
                    else
                    {
                        errorHandler(2, Message("msg_print copies format is not correct"), "");
                        isPrintComplete = false;
                    }
                }
                #endregion

                int panelQty = 0;
                foreach (DataGridViewRow row in this.gridSetup.Rows)
                {
                    if (string.IsNullOrEmpty(row.Cells["QTY"].Value.ToString()))
                        return;
                    panelQty += Convert.ToInt32(row.Cells["QTY"].Value.ToString());
                }

                //this.BeginInvoke(new Action(() =>
                //{
                if (panelQty < boardquatity)
                {
                    errorHandler(2, Message("msg_Material Qty is less, Please scan new Material bin."), "");
                    isPrintComplete = false;
                    return;
                }
                //后台打印程序是按照当前上料上料的数量满足多少个大板码
                for (int i = 0; i < panelQty / boardquatity; i++)
                {
                    if (!isPrintComplete)
                        break;
                    if (panelQty < boardquatity)
                    {
                        errorHandler(2, Message("msg_Material Qty is less, Please scan new Material bin."), "");
                        isPrintComplete = false;
                        return;
                    }
                    if (UploadSerialnumber())
                    {
                        //剩余不足,但是仍旧打印一个大板

                        #region 打印大板和小板
                        if (ckbRefSmall.Checked)
                        {
                            foreach (var refSN in new List<string>(dicSNEXT.Keys))
                            {
                                if (!isPrintComplete)//停止就不打印剩下的标签
                                    break;
                                if (config.PRINTER_MODE == "2")
                                    RefSNlist.Add(refSN.ToString());
                                else if (config.PRINTER_MODE == "3")
                                {
                                    if (boardquatity > 1)
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;0;1;1]");
                                    else
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;1;1;1]");
                                }

                                AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, refSN });
                                Dictionary<string, string> dicSN2 = dicSNEXT[refSN];
                                foreach (var itemSN in new List<string>(dicSN2.Keys))
                                {
                                    if (!isPrintComplete)//停止就不打印剩下的标签
                                        break;
                                    if (config.PRINTER_MODE == "2")
                                        RefSNlist.Add(itemSN.ToString());
                                    else if (config.PRINTER_MODE == "3")
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";1;" + dicSN2[itemSN.ToString()] + ";1;1]");
                                    AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, itemSN });
                                    dicSNEXT[refSN.ToString()].Remove(itemSN);
                                }
                            }
                        }
                        #endregion

                        #region 打印小板码
                        if (ckbSmall.Checked)
                        {
                            foreach (var refSN in new List<string>(dicSNEXT.Keys))
                            {
                                if (!isPrintComplete)//停止就不打印剩下的标签
                                    break;
                                Dictionary<string, string> dicSN2 = dicSNEXT[refSN];
                                foreach (var itemSN in new List<string>(dicSN2.Keys))
                                {
                                    if (!isPrintComplete)//停止就不打印剩下的标签
                                        break;
                                    if (config.PRINTER_MODE == "2")
                                        RefSNlist.Add(itemSN.ToString());
                                    else if (config.PRINTER_MODE == "3")
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + itemSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";1;" + dicSN2[itemSN.ToString()] + ";1;1]");
                                    AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, itemSN });
                                    dicSN2.Remove(itemSN);
                                    dicSNEXT[refSN.ToString()].Remove(itemSN);
                                }
                            }
                        }
                        #endregion

                        #region 打印大板码
                        if (rdbNoSPanel.Checked)
                        {
                            foreach (var refSN in new List<string>(dicSNEXT.Keys))
                            {
                                if (!isPrintComplete) //停止就不打印剩下的标签
                                    break;
                                if (config.PRINTER_MODE == "2")
                                    RefSNlist.Add(refSN.ToString());
                                else if (config.PRINTER_MODE == "3")
                                {
                                    if (boardquatity > 1)
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;0;1;1]");
                                    else
                                        RefSNlist.Add("[" + txbCDAMONumber.Text + ";" + refSN.ToString() + ";" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ";0;1;1;1]");
                                }

                                AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, refSN });
                                if (dicSNEXT != null && dicSNEXT.Keys.Count > 0 && dicSNEXT.Keys.Contains(refSN.ToString()))
                                    dicSNEXT[refSN.ToString()].Clear();
                            }
                        }
                        #endregion

                    }
                    else
                    {
                        if (dicSNEXT.Keys.Count > 0)
                        {
                            string regular = @"^[1-9]\d*$";
                            Match match = Regex.Match(this.txtPrintCopies.Text, regular);
                            if (match.Success)
                            {
                                for (int j = 0; j < Convert.ToInt32(match.ToString()); j++)
                                {
                                    if (config.PRINTER_MODE == "2")
                                    {
                                        PrintLabelExt(RefSNlist);
                                    }
                                    else if (config.PRINTER_MODE == "3")
                                    {
                                        string bodymessage = "";
                                        foreach (var outputdata in RefSNlist)
                                        {
                                            if (bodymessage == "")
                                                bodymessage = outputdata;
                                            else
                                                bodymessage = bodymessage + ";" + outputdata;
                                        }
                                        endsendmessage = "{outputSNToTable;" + bodymessage + "}";
                                        string returnmessage = cSocket2.SendData(endsendmessage);
                                        if (returnmessage == null)
                                        {
                                            bool isOK = cSocket2.connect(config.IPAdress, config.Port);
                                            if (!isOK)
                                            {
                                                isPrintComplete = false;
                                                return;
                                            }
                                            else
                                            {
                                                returnmessage = cSocket2.SendData(endsendmessage);
                                                if (returnmessage != null && returnmessage != "")
                                                {
                                                    string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                                                    string result = splits[1];
                                                    string message = splits[2];
                                                    if (result == "0")
                                                        errorHandler(0, message, "");
                                                    else
                                                        errorHandler(2, message, "");
                                                }
                                            }
                                        }
                                        if (returnmessage != null && returnmessage != "")
                                        {
                                            string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                                            string result = splits[1];
                                            string message = splits[2];
                                            if (result == "0")
                                                errorHandler(0, message, "");
                                            else
                                                errorHandler(2, message, "");
                                        }
                                    }
                                    this.Invoke(new MethodInvoker(delegate
                                    {
                                        this.label10.Text = Convert.ToString((Convert.ToInt32(this.label10.Text) + RefSNlist.Count));
                                    }));
                                }
                                dicSNEXT.Clear();
                                RefSNlist.Clear();
                            }
                            else
                            {
                                errorHandler(2, Message("msg_print copies format is not correct"), "");
                                isPrintComplete = false;
                            }
                        }
                        dicSNEXT.Clear();
                        break;
                    }

                    if (RefSNlist.Count == Convert.ToInt32(config.LABEL_QTY) || i == panelQty / boardquatity - 1)
                    {
                        string regular = @"^[1-9]\d*$";
                        Match match = Regex.Match(this.txtPrintCopies.Text, regular);
                        if (match.Success)
                        {
                            for (int j = 0; j < Convert.ToInt32(match.ToString()); j++)
                            {
                                if (config.PRINTER_MODE == "2")
                                {
                                    PrintLabelExt(RefSNlist);
                                }
                                else if (config.PRINTER_MODE == "3")
                                {
                                    string bodymessage = "";
                                    foreach (var outputdata in RefSNlist)
                                    {
                                        if (bodymessage == "")
                                            bodymessage = outputdata;
                                        else
                                            bodymessage = bodymessage + ";" + outputdata;
                                    }
                                    endsendmessage = "{outputSNToTable;" + bodymessage + "}";
                                    string returnmessage = cSocket2.SendData(endsendmessage);
                                    if (returnmessage == null)
                                    {
                                        bool isOK = cSocket2.connect(config.IPAdress, config.Port);
                                        if (!isOK)
                                        {
                                            isPrintComplete = false;
                                            return;
                                        }
                                        else
                                        {
                                            returnmessage = cSocket2.SendData(endsendmessage);
                                            if (returnmessage != null && returnmessage != "")
                                            {
                                                string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                                                string result = splits[1];
                                                string message = splits[2];
                                                if (result == "0")
                                                    errorHandler(0, message, "");
                                                else
                                                    errorHandler(2, message, "");
                                            }
                                        }
                                    }
                                    else if (returnmessage != null && returnmessage != "")
                                    {
                                        string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                                        string result = splits[1];
                                        string message = splits[2];
                                        if (result == "0")
                                            errorHandler(0, message, "");
                                        else
                                            errorHandler(2, message, "");
                                    }
                                }
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    this.label10.Text = Convert.ToString((Convert.ToInt32(this.label10.Text) + RefSNlist.Count));
                                }));
                            }
                            dicSNEXT.Clear();
                            RefSNlist.Clear();
                        }
                        else
                        {
                            errorHandler(2, Message("msg_print copies format is not correct"), "");
                            isPrintComplete = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bool isOK = cSocket2.connect(config.IPAdress, config.Port);
                if (!isOK)
                {
                    isPrintComplete = false;
                    return;
                }
                else
                {
                    string returnmessage = cSocket2.SendData(endsendmessage);
                    if (returnmessage != null && returnmessage != "")
                    {
                        string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                        string result = splits[1];
                        string message = splits[2];
                        if (result == "0")
                            errorHandler(0, message, "");
                        else
                            errorHandler(2, message, "");
                    }
                }
            }
            //}));
        }

        bool isPrintComplete;
        BackgroundWorker m_worker;
        private void btnStart_Click(object sender, EventArgs e)
        {
            m_worker = new BackgroundWorker();
            isPrintComplete = true;
            m_worker.DoWork += new DoWorkEventHandler(m_Worker_DoWork);
            m_worker.RunWorkerAsync();
        }
        private void btnPulse_Click(object sender, EventArgs e)
        {
            isPrintComplete = false;
            errorHandler(0, Message("msg_pulse print"), "");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isPrintComplete = false;
            dicSNEXT.Clear();
            RefSNlist.Clear();
            errorHandler(0, Message("msg_Stop print"), "");
        }
        public List<string> list = null;
        private void btnRePrint_Click(object sender, EventArgs e)
        {
            if (!rdbRefSerialnumber.Checked && !rdbSerialnumber.Checked && !rdbRefandSN.Checked)
            {
                MessageBox.Show(this, Message("msg_please select reprint model"));
                return;
            }
            if (isPrintComplete)
            {
                DialogResult dr = MessageBox.Show(Message("msg_please pulse print"), Message("msg_warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.OK)
                {
                    isPrintComplete = false;
                }
                else
                {
                    return;
                }
            }
            if (this.txbCDAMONumber.Text == "")
            {
                return;
            }
            GetCurrentWorkorder getCurrentHandler = new GetCurrentWorkorder(sessionContext, initModel, this);

            string[] serialnumberValues = getCurrentHandler.GetSerialnumberForWorkOrder(this.txbCDAMONumber.Text, this.label8.Text);
            if (serialnumberValues != null && serialnumberValues.Length > 0)
            {
                list = new List<string>();
                Reprint reprintForm = new Reprint(serialnumberValues, this);
                reprintForm.ShowDialog(this);

                if (reprintForm.ListWreprintSN != null && reprintForm.ListWreprintSN.Count > 0)
                {
                    foreach (var serilanumber in reprintForm.ListWreprintSN)
                    {
                        if (config.PRINTER_MODE == "2")
                            AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, serilanumber });
                        else if (config.PRINTER_MODE == "3")
                        {
                            AddDataToPCBGrid(new object[2] { dgvPrintSN.RowCount + 1, serilanumber.Split(';')[1] });
                        }
                    }
                    //PrintLabelExt(reprintForm.ListWreprintSN);

                    if (config.PRINTER_MODE == "2")
                    {
                        PrintLabelExt(reprintForm.ListWreprintSN);
                    }
                    else if (config.PRINTER_MODE == "3")
                    {
                        string bodymessage = "";
                        foreach (var outputdata in reprintForm.ListWreprintSN)
                        {
                            if (bodymessage == "")
                                bodymessage = outputdata;
                            else
                                bodymessage = bodymessage + ";" + outputdata;
                        }
                        string endsendmessage = "{outputSNToTable;" + bodymessage + "}";
                        string returnmessage = cSocket2.SendData(endsendmessage);
                        if (returnmessage != null && returnmessage != "")
                        {
                            string[] splits = returnmessage.Replace("{", "").Replace("}", "").Split(';');
                            string result = splits[1];
                            string message = splits[2];
                            if (result == "0")
                                errorHandler(0, message, "");
                            else
                                errorHandler(2, message, "");
                        }
                    }
                }
            }

        }

        private delegate void AddDataToPCBGridHandle(object[] values);
        private void AddDataToPCBGrid(object[] values)
        {
            try
            {
                if (this.dgvPrintSN.InvokeRequired)
                {
                    AddDataToPCBGridHandle addDataDel = new AddDataToPCBGridHandle(AddDataToPCBGrid);
                    Invoke(addDataDel, new object[] { values });
                }
                else
                {
                    this.dgvPrintSN.Rows.Insert(0, values);
                    this.dgvPrintSN.ClearSelection();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        private void SavePRintCount()
        {
            try
            {
                string path = @"PrintQty.txt";

                StringBuilder sb = new StringBuilder();
                if (File.Exists(path))
                {
                    string[] allLine = File.ReadAllLines(path);
                    int count = 0;
                    foreach (var line in allLine)
                    {
                        sb.AppendLine(line);
                        if (this.txbCDAMONumber.Text == line.Split(';')[0])
                        {
                            sb.Replace(line.ToString(), txbCDAMONumber.Text + ";" + this.label10.Text);
                        }
                        else
                        {
                            count++;
                        }
                    }
                    if (count == allLine.Length)
                        sb.AppendLine(txbCDAMONumber.Text + ";" + this.label10.Text);
                }
                else
                {
                    sb.AppendLine(txbCDAMONumber.Text + ";" + this.label10.Text);
                }
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                byte[] bt = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bt, 0, bt.Length);
                fs.Flush();
                fs.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }
        private string ReadPRintCount()
        {
            try
            {
                string printqty = "0";
                string path = @"PrintQty.txt";
                if (File.Exists(path))
                {
                    string[] allLine = File.ReadAllLines(path);
                    foreach (var line in allLine)
                    {
                        if (this.txbCDAMONumber.Text == line.Split(';')[0])
                        {
                            printqty = line.Split(';')[1];
                            break;
                        }
                    }
                }

                return printqty;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return "0";
            }
        }
        #endregion
        #region Panel Position and Direction graphics
        PosGraghicsForm frm = null;
        private void btnShowPCB_Click(object sender, EventArgs e)
        {
            if (frm != null && frm.pictureBox1.Image != null)
            {
                frm.Hide();
            }
            if (txbCDAPartNumber.Text == "")
            {
                errorHandler(2, Message("msg_No activated work order"), "");
                return;
            }
            frm = new PosGraghicsForm(this, sessionContext, initModel, txbCDAPartNumber.Text, initModel.currentSettings == null ? "-1" : initModel.currentSettings.processLayer.ToString());
            frm.Show();
            if (frm.pictureBox1.Image == null)
                frm.Hide();
        }
        #endregion

        #region checklist from OA
        bool Supervisor = false;
        bool IPQC = true;
        private void InitTaskData_SOCKET(string djclass)
        {
            try
            {
                string PartNumber = this.txbCDAPartNumber.Text;
                if (PartNumber == "")
                {
                    errorHandler(2, Message("msg_no active wo"), "");
                    return;
                }
                this.Invoke(new MethodInvoker(delegate
                {
                    try
                    {
                        this.dgvCheckListTable.Rows.Clear();

                        Supervisor = false;
                        IPQC = true;
                        GetWorkPlanData handle = new GetWorkPlanData(sessionContext, initModel, this);
                        int firstSN = int.Parse(this.lblPass.Text) + int.Parse(this.lblFail.Text) + int.Parse(this.lblScrap.Text);
                        if (firstSN == 0)
                        {
                            djclass = djclass + ";首末件点检";
                        }

                        string workstep_text = handle.GetWorkStepInfobyWorkPlan(this.txbCDAMONumber.Text, initModel.currentSettings.processLayer);
                        if (workstep_text != "")
                        {
                            GetAttributeValue getAttriHandler = new GetAttributeValue(sessionContext, initModel, this);
                            string[] processCode = getAttriHandler.GetAttributeValueForAll(1, this.txbCDAMONumber.Text, "-1", "TTE_PROCESS_CODE");
                            if (processCode != null && processCode.Length > 0)
                            {
                                string process = processCode[1];
                                string sedmessage = "{getCheckListItem;" + PartNumber + ";" + process + ";[" + workstep_text + "];[" + djclass + "];" + "}";
                                string returnMsg = checklist_cSocket.SendData(sedmessage);

                                if (returnMsg != "" && returnMsg != null)
                                {
                                    string[] values = returnMsg.TrimEnd(';').Replace("{", "").Replace("}", "").Replace("#", "").Split(new string[] { ";" }, StringSplitOptions.None);
                                    string status = values[1];
                                    if (status == "0")//“0” , or “-1” (error)  
                                    {
                                        int seq = 1;
                                        string itemregular = @"\{[^\{\}]+\}"; //@"\[[^\[\]]+\]";
                                        MatchCollection match = Regex.Matches(returnMsg.TrimStart('{').Substring(0, returnMsg.Length - 2), itemregular);
                                        if (match.Count <= 0)
                                        {
                                            errorHandler(2, Message("msg_No checklist data"), "");
                                            return;
                                        }
                                        else
                                            SetTipMessage(MessageType.OK, "");
                                        for (int i = 0; i < match.Count; i++)
                                        {
                                            string data = match[i].ToString().TrimStart('{').TrimEnd('}');
                                            //string[] datas = data.Split(';');
                                            string[] datas = Regex.Split(data, "#!#", RegexOptions.IgnoreCase);
                                            string sourceclass = datas[4];//数据来源
                                            string formno = datas[0];//对应单号
                                            string itemno = datas[1];//机种品号
                                            string itemnname = datas[2];//机种品名
                                            string sbno = datas[5];//设备编号
                                            string sbname = datas[6];//设备名称
                                            string gcno = datas[7];//过程编号
                                            string gcname = datas[8];//过程名称
                                            string lbclass = datas[9];//类别
                                            string djxmname = datas[10];//点检项目
                                            string specvalue = datas[11];//规格值
                                            string djkind = datas[12];//点检类型
                                            string maxvalue = datas[14];//上限值
                                            string minvalue = datas[13];//下限值
                                            string djclase = datas[15];//点检类别
                                            string djversion = datas[3];//版本
                                            string dataclass = datas[16];//状态

                                            object[] objValues = new object[] { seq, djclase, djxmname, gcname, specvalue, "", "", "", djkind, gcno, maxvalue, minvalue, lbclass, sourceclass, formno, itemno, itemnname, sbno, sbname, djversion, dataclass, "" };
                                            this.dgvCheckListTable.Rows.Add(objValues);
                                            seq++;
                                            SetCheckListInputStatusTable();

                                            if (djkind == "判断值")
                                            {
                                                string[] strInputValues = new string[] { "Y", "N" };
                                                DataTable dtInput = new DataTable();
                                                dtInput.Columns.Add("name");
                                                dtInput.Columns.Add("value");
                                                DataRow rowEmpty = dtInput.NewRow();
                                                rowEmpty["name"] = "";
                                                rowEmpty["value"] = "";
                                                dtInput.Rows.Add(rowEmpty);
                                                foreach (var strValues in strInputValues)
                                                {
                                                    DataRow row = dtInput.NewRow();
                                                    row["name"] = strValues;
                                                    row["value"] = strValues;
                                                    dtInput.Rows.Add(row);
                                                }

                                                DataGridViewComboBoxCell ComboBoxCell = new DataGridViewComboBoxCell();
                                                ComboBoxCell.DataSource = dtInput;
                                                ComboBoxCell.DisplayMember = "Name";
                                                ComboBoxCell.ValueMember = "Value";
                                                dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabResult2"] = ComboBoxCell;
                                            }

                                            this.dgvCheckListTable.ClearSelection();
                                        }
                                    }
                                    else
                                    {
                                        string errormsg = values[1];
                                        errorHandler(2, errormsg, "");
                                    }
                                }
                                else
                                {
                                    isOK = checklist_cSocket.connect(config.CHECKLIST_IPAddress, config.CHECKLIST_Port);
                                    returnMsg = checklist_cSocket.SendData(sedmessage);

                                    if (returnMsg != "" && returnMsg != null)
                                    {
                                        string[] values = returnMsg.TrimEnd(';').Replace("{", "").Replace("}", "").Replace("#", "").Split(new string[] { ";" }, StringSplitOptions.None);
                                        string status = values[1];
                                        if (status == "0")//“0” , or “-1” (error)  
                                        {
                                            int seq = 1;
                                            string itemregular = @"\{[^\{\}]+\}";
                                            MatchCollection match = Regex.Matches(returnMsg.TrimStart('{').Substring(0, returnMsg.Length - 2), itemregular);
                                            if (match.Count <= 0)
                                            {
                                                errorHandler(2, Message("msg_No checklist data"), "");
                                                return;
                                            }
                                            else
                                                SetTipMessage(MessageType.OK, "");
                                            for (int i = 0; i < match.Count; i++)
                                            {
                                                string data = match[i].ToString().TrimStart('{').TrimEnd('}');
                                                //string[] datas = data.Split(';');
                                                string[] datas = Regex.Split(data, "#!#", RegexOptions.IgnoreCase);
                                                string sourceclass = datas[4];//数据来源
                                                string formno = datas[0];//对应单号
                                                string itemno = datas[1];//机种品号
                                                string itemnname = datas[2];//机种品名
                                                string sbno = datas[5];//设备编号
                                                string sbname = datas[6];//设备名称
                                                string gcno = datas[7];//过程编号
                                                string gcname = datas[8];//过程名称
                                                string lbclass = datas[9];//类别
                                                string djxmname = datas[10];//点检项目
                                                string specvalue = datas[11];//规格值
                                                string djkind = datas[12];//点检类型
                                                string maxvalue = datas[14];//上限值
                                                string minvalue = datas[13];//下限值
                                                string djclase = datas[15];//点检类别
                                                string djversion = datas[3];//版本
                                                string dataclass = datas[16];//状态

                                                object[] objValues = new object[] { seq, djclase, djxmname, gcname, specvalue, "", "", "", djkind, gcno, maxvalue, minvalue, lbclass, sourceclass, formno, itemno, itemnname, sbno, sbname, djversion, dataclass, "" };
                                                this.dgvCheckListTable.Rows.Add(objValues);
                                                seq++;
                                                SetCheckListInputStatusTable();

                                                if (djkind == "判断值")
                                                {
                                                    string[] strInputValues = new string[] { "Y", "N" };
                                                    DataTable dtInput = new DataTable();
                                                    dtInput.Columns.Add("name");
                                                    dtInput.Columns.Add("value");
                                                    DataRow rowEmpty = dtInput.NewRow();
                                                    rowEmpty["name"] = "";
                                                    rowEmpty["value"] = "";
                                                    dtInput.Rows.Add(rowEmpty);
                                                    foreach (var strValues in strInputValues)
                                                    {
                                                        DataRow row = dtInput.NewRow();
                                                        row["name"] = strValues;
                                                        row["value"] = strValues;
                                                        dtInput.Rows.Add(row);
                                                    }

                                                    DataGridViewComboBoxCell ComboBoxCell = new DataGridViewComboBoxCell();
                                                    ComboBoxCell.DataSource = dtInput;
                                                    ComboBoxCell.DisplayMember = "Name";
                                                    ComboBoxCell.ValueMember = "Value";
                                                    dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabResult2"] = ComboBoxCell;
                                                }

                                                this.dgvCheckListTable.ClearSelection();
                                            }
                                        }
                                        else
                                        {
                                            string errormsg = values[1];
                                            errorHandler(2, errormsg, "");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                errorHandler(2, Message("msg_no TTE_PROCESS_CODE"), "");//20161213 edit by qy
                                return;
                            }
                        }
                        else
                        {
                            errorHandler(2, Message("msg_no workstep text"), "");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex.Message, ex);
                    }
                }));

            }
            catch (Exception ex)
            {
                //20161208 edit by qy
                LogHelper.Error(ex.Message, ex);
            }
        }

        private void SetCheckListInputStatusTable()
        {
            foreach (DataGridViewRow row in this.dgvCheckListTable.Rows)
            {
                if (row.Cells["tabdjkind"].Value.ToString() == "判断值")
                {
                    row.Cells["tabResult1"].ReadOnly = true;
                }
                else if (row.Cells["tabdjkind"].Value.ToString() == "输入值" || row.Cells["tabdjkind"].Value.ToString() == "范围值")
                {
                    row.Cells["tabResult2"].ReadOnly = true;
                }
                row.Cells["tabNo"].ReadOnly = true;
                row.Cells["tabStatus"].ReadOnly = true;
            }
        }

        private void btnSupervisor_Click(object sender, EventArgs e)
        {
            if (gridCheckList.RowCount <= 0)
            {
                return;
            }
            if (config.LogInType == "COM" && initModel.scannerHandler.heandler().IsOpen)
                initModel.scannerHandler.heandler().Close();

            LoginForm LogForm = new LoginForm(4, this, "");
            LogForm.ShowDialog();
        }

        private void btnIPQC_Click(object sender, EventArgs e)
        {
            if (gridCheckList.RowCount <= 0)
            {
                return;
            }
            if (config.LogInType == "COM" && initModel.scannerHandler.heandler().IsOpen)
                initModel.scannerHandler.heandler().Close();

            LoginForm LogForm = new LoginForm(5, this, "");
            LogForm.ShowDialog();
        }

        public void SupervisorConfirm(string user)//班长确认
        {
            DialogResult dr = MessageBox.Show(Message("msg_produtc or not"), Message("msg_Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                Supervisor = true;
                errorHandler(0, Message("msg_supervisor confirm OK"), "");
            }
            else
            {
                Supervisor = false;
                errorHandler(2, Message("msg_supervisor confirm NG"), "");
            }
            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
            {
                SaveCheckList();
                string result = "N";
                if (Supervisor)
                    result = "Y";
                string endsendmessage = "{updateCheckListResult;1;" + user + ";" + result + ";" + sequece + "}";
                checklist_cSocket.SendData(endsendmessage);
            }

        }

        public void IPQCConfirm(string user)//IPQC巡检
        {
            DialogResult dr = MessageBox.Show(Message("msg_IPQC produtc or not"), Message("msg_Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                IPQC = true;
                errorHandler(0, Message("msg_IPQC confirm OK"), "");
            }
            else
            {
                IPQC = false;
                errorHandler(2, Message("msg_IPQC confirm NG"), "");
            }
            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
            {
                SaveCheckList();
                string result = "N";
                if (Supervisor)
                    result = "Y";
                string endsendmessage = "{updateCheckListResult;2;" + user + ";" + result + ";" + sequece + "}";
                checklist_cSocket.SendData(endsendmessage);
            }
        }

        private void btnAddCheckListTable_Click(object sender, EventArgs e)
        {
            dgvCheckListTable.Rows.Add(new object[] { this.dgvCheckListTable.Rows.Count + 1, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" });

            dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabResult1"].ReadOnly = true;
            dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabNo"].ReadOnly = true;
            dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabStatus"].ReadOnly = true;
            dgvCheckListTable.ClearSelection();
        }
        string sequece = "";
        private void btnConfirmTable_Click(object sender, EventArgs e)
        {
            try
            {

                string PartNumber = this.txbCDAPartNumber.Text;
                if (PartNumber == "")
                {
                    errorHandler(2, Message("msg_no active wo"), "");
                    return;
                }
                foreach (DataGridViewRow row in this.dgvCheckListTable.Rows)
                {
                    if (row.Cells["tabStatus"].Value == null || row.Cells["tabStatus"].Value.ToString() == "")
                    {
                        errorHandler(2, Message("msg_Verify_CheckList"), "");
                        return;
                    }
                }

                string headmessage = "{appendCheckListResult;" + PartNumber;
                string sedmessage = "";
                string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                foreach (DataGridViewRow row in this.dgvCheckListTable.Rows)
                {
                    string gdcode = this.txbCDAMONumber.Text;
                    string itemno = PartNumber;
                    string itemname = initModel.currentSettings.partdesc;
                    string gczcode = initModel.configHandler.StationNumber;
                    string gczname = "";
                    string lineclass = "";
                    string lbclass = row.Cells["tablbclass"].Value.ToString();
                    string djxmname = row.Cells["tabdjxmname"].Value.ToString();
                    string specvalue = "";
                    if (row.Cells["tabResult1"].Value.ToString() != "")
                        specvalue = row.Cells["tabResult1"].Value.ToString();
                    else
                        specvalue = row.Cells["tabResult2"].Value.ToString();
                    string djkind = row.Cells["tabdjkind"].Value.ToString();
                    string maxvalues = row.Cells["tabmaxvalue"].Value.ToString();
                    string minvalues = row.Cells["tabminvalue"].Value.ToString();
                    string djclass = row.Cells["tabdjclass"].Value.ToString();
                    string djversion = row.Cells["tabdjversion"].Value.ToString();
                    string djuser = lblUser.Text;
                    string djremark = "";
                    string djdate = date;
                    string jcuser = lblUser.Text;
                    string qruser = "";
                    string pguser = "";

                    string msgrow = "{" + gdcode + "#!#" + itemno + "#!#" + itemname + "#!#" + gczcode + "#!#" + gczname + "#!#" + lineclass + "#!#" + lbclass + "#!#" + djxmname + "#!#" + specvalue + "#!#" + djkind + "#!#" + maxvalues + "#!#" + minvalues + "#!#" + djclass + "#!#" + djversion + "#!#" + djuser + "#!#" + djremark + "#!#" + djdate + "#!#" + jcuser + "#!#" + qruser + "#!#" + pguser + "}";
                    if (sedmessage == "")
                        sedmessage = msgrow;
                    else
                        sedmessage = sedmessage + ";" + msgrow;
                }
                if (sedmessage == "")
                {
                    errorHandler(2, Message("msg_No checklist data"), "");
                    return;
                }
                string endsendmessage = headmessage + ";" + sedmessage + "}";
                string returnMsg = checklist_cSocket.SendData(endsendmessage);
                if (returnMsg != null && returnMsg != "")
                {
                    returnMsg = returnMsg.TrimStart('{').TrimEnd('}');
                    string[] Msgs = returnMsg.Split(';');
                    if (Msgs[1] == "0")
                    {
                        if (Supervisor_OPTION == "1")
                        {
                            Supervisor = true;
                            errorHandler(0, Message("msg_Send_CheckList_Success"), "");
                        }
                        else
                        {
                            errorHandler(0, Message("msg_Send_CheckList_Success,please supervisor confirm"), "");
                        }

                        sequece = Msgs[3];
                        SaveCheckList();
                        WriteIntoShift2();
                        InitShift2(txbCDAMONumber.Text);
                    }
                    else
                    {
                        errorHandler(2, Message("msg_Send_CheckList_fail"), "");
                    }
                }
                else
                {
                    isOK = checklist_cSocket.connect(config.CHECKLIST_IPAddress, config.CHECKLIST_Port);
                    returnMsg = checklist_cSocket.SendData(endsendmessage);
                    if (returnMsg != null && returnMsg != "")
                    {
                        returnMsg = returnMsg.TrimStart('{').TrimEnd('}');
                        string[] Msgs = returnMsg.Split(';');
                        if (Msgs[1] == "0")
                        {
                            if (Supervisor_OPTION == "1")
                            {
                                Supervisor = true;
                                errorHandler(0, Message("msg_Send_CheckList_Success"), "");
                            }
                            else
                            {
                                errorHandler(0, Message("msg_Send_CheckList_Success,please supervisor confirm"), "");
                            }

                            sequece = Msgs[3];
                            SaveCheckList();
                            WriteIntoShift2();
                            InitShift2(txbCDAMONumber.Text);
                        }
                        else
                        {
                            errorHandler(2, Message("msg_Send_CheckList_fail"), "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //20161208 edit by qy
                LogHelper.Error(ex.Message, ex);
            }
        }

        private void btnSupervisorTable_Click(object sender, EventArgs e)
        {
            if (sequece == "")
            {
                return;
            }
            if (config.LogInType == "COM" && initModel.scannerHandler.heandler().IsOpen)
                initModel.scannerHandler.heandler().Close();

            LoginForm LogForm = new LoginForm(4, this, "");
            LogForm.ShowDialog();
        }

        private void btnIPQCTable_Click(object sender, EventArgs e)
        {
            if (sequece == "")
            {
                return;
            }
            if (config.LogInType == "COM" && initModel.scannerHandler.heandler().IsOpen)
                initModel.scannerHandler.heandler().Close();

            LoginForm LogForm = new LoginForm(5, this, "");
            LogForm.ShowDialog();
        }
        private void dgvCheckListTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //20161208 edit by qy
            try
            {
                if (e.RowIndex == -1)
                    return;
                if (this.dgvCheckListTable.Columns[e.ColumnIndex].Name == "tabResult1" && this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabResult1"].Value.ToString() != "")
                {
                    if (this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabdjkind"].Value.ToString() == "范围值")
                    {
                        //verify the input value
                        string strRegex = @"^\d{0,9}\.\d{0,9}|-\d{0,9}\.\d{0,9}";//@"^(\d{0,9}.\d{0,9})～(\d{0,9}.\d{0,9}).*$";"^(\-|\+?\d{0,9}.\d{0,9})～(\-|\+?\d{0,9}.\d{0,9})$"
                        string strResult1 = this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabResult1"].Value.ToString();
                        string strStandard = this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabspecname"].Value.ToString().Replace("（", "").Replace("）", "");
                        string strMax = this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabmaxvalue"].Value.ToString();
                        string strMin = this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabminvalue"].Value.ToString();
                        Match match1 = Regex.Match(strMax, strRegex);
                        Match match2 = Regex.Match(strMin, strRegex);
                        if (match1.Success && match2.Success)
                        {
                            //if (match.Groups.Count > 2)
                            //{
                            //double iMin = Convert.ToDouble(match.Groups[1].Value);
                            //double iMax = Convert.ToDouble(match.Groups[2].Value);
                            double iMin = Convert.ToDouble(match2.ToString());
                            double iMax = Convert.ToDouble(match1.ToString());
                            double iResult = Convert.ToDouble(strResult1);
                            if (iResult >= iMin && iResult <= iMax)
                            {
                                this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Style.BackColor = Color.FromArgb(0, 192, 0);
                                this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Value = "OK";
                            }
                            else
                            {
                                this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Style.BackColor = Color.Red;
                                this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Value = "NG";
                            }
                            //}
                        }
                        else
                        {
                            this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Style.BackColor = Color.Red;
                            this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Value = "NG";
                        }
                    }
                    else if (this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabdjkind"].Value.ToString() == "输入值")
                    {
                        if (this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabResult1"].Value.ToString() == this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabspecname"].Value.ToString())
                        {
                            this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Style.BackColor = Color.FromArgb(0, 192, 0);
                            this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Value = "OK";
                        }
                        else
                        {
                            this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Style.BackColor = Color.Red;
                            this.dgvCheckListTable.Rows[e.RowIndex].Cells["tabStatus"].Value = "NG";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex.Message, ex);
            }

        }

        private void dgvCheckListTable_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv.CurrentCell.GetType().Name == "DataGridViewComboBoxCell" && dgv.CurrentCell.RowIndex != -1)
            {
                iRowIndex = dgv.CurrentCell.RowIndex;
                (e.Control as ComboBox).SelectedIndexChanged += new EventHandler(ComboBoxTable_SelectedIndexChanged);
            }
        }

        public void ComboBoxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combox = sender as ComboBox;
            combox.Leave += new EventHandler(comboxtable_Leave);
            try
            {
                if (combox.SelectedItem != null && combox.Text != "")
                {
                    if (OKlist.Contains(combox.Text))
                    {
                        this.dgvCheckListTable.Rows[iRowIndex].Cells["tabStatus"].Style.BackColor = Color.FromArgb(0, 192, 0);
                        this.dgvCheckListTable.Rows[iRowIndex].Cells["tabStatus"].Value = "OK";
                    }
                    else
                    {
                        this.dgvCheckListTable.Rows[iRowIndex].Cells["tabStatus"].Style.BackColor = Color.Red;
                        this.dgvCheckListTable.Rows[iRowIndex].Cells["tabStatus"].Value = "NG";
                    }
                }
                else
                {
                    this.dgvCheckListTable.Rows[iRowIndex].Cells["tabStatus"].Style.BackColor = Color.White;
                    this.dgvCheckListTable.Rows[iRowIndex].Cells["tabStatus"].Value = "";
                }
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void comboxtable_Leave(object sender, EventArgs e)
        {
            ComboBox combox = sender as ComboBox;
            combox.SelectedIndexChanged -= new EventHandler(ComboBoxTable_SelectedIndexChanged);
        }

        int iIndexCheckListTable = -1;
        private void dgvCheckListTable_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.dgvCheckListTable.Rows.Count == 0)
                    return;
                ((DataGridView)sender).CurrentRow.Selected = true;
                iIndexCheckListTable = ((DataGridView)sender).CurrentRow.Index;
                this.dgvCheckListTable.ContextMenuStrip = contextMenuStrip2;

                if (iIndexCheckListTable == -1)
                    this.dgvCheckListTable.ContextMenuStrip = null;

            }
        }
        private void dgvCheckListTable_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{
            //    if (this.dgvCheckListTable.Rows.Count == 0)
            //        return;

            //    if (e.RowIndex == -1)
            //    {
            //        this.dgvCheckListTable.ContextMenuStrip = null;
            //        return;
            //    }

            //    iIndexCheckListTable = ((DataGridView)sender).CurrentRow.Index;
            //    this.dgvCheckListTable.ContextMenuStrip = contextMenuStrip2;
            //    ((DataGridView)sender).CurrentRow.Selected = true;
            //}
        }

        private void SaveCheckList()
        {
            try
            {
                string path = @"CheckList.txt";
                string datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(datetime);
                sb.AppendLine(txbCDAMONumber.Text + ";" + initModel.currentSettings.processLayer);
                sb.AppendLine(Supervisor.ToString());
                sb.AppendLine(IPQC.ToString());
                sb.AppendLine(sequece);
                foreach (DataGridViewRow row in dgvCheckListTable.Rows)
                {
                    string sourceclass = row.Cells["tabsourceclass"].Value.ToString();//数据来源
                    string formno = row.Cells["tabformno"].Value.ToString();//对应单号
                    string itemno = row.Cells["tabitemno"].Value.ToString();//机种品号
                    string itemnname = row.Cells["tabitemname"].Value.ToString();//机种品名
                    string sbno = row.Cells["tabsbno"].Value.ToString();//设备编号
                    string sbname = row.Cells["tabsnname"].Value.ToString();//设备名称
                    string gcno = row.Cells["tabgcno"].Value.ToString();//过程编号
                    string gcname = row.Cells["tabgcname"].Value.ToString();//过程名称
                    string lbclass = row.Cells["tablbclass"].Value.ToString();//类别
                    string djxmname = row.Cells["tabdjxmname"].Value.ToString();//点检项目
                    string specvalue = row.Cells["tabspecname"].Value.ToString();//规格值
                    string result1 = row.Cells["tabResult1"].Value.ToString();
                    string result2 = row.Cells["tabResult2"].Value == null ? "" : row.Cells["tabResult2"].Value.ToString();// row.Cells["tabResult2"].Value.ToString();
                    string status = row.Cells["tabstatus"].Value.ToString();//结果
                    string djkind = row.Cells["tabdjkind"].Value.ToString();//点检类型
                    string maxvalue = row.Cells["tabmaxvalue"].Value.ToString();//上限值
                    string minvalue = row.Cells["tabminvalue"].Value.ToString();//下限值
                    string djclase = row.Cells["tabdjclass"].Value.ToString();//点检类别
                    string djversion = row.Cells["tabdjversion"].Value.ToString();//版本
                    string dataclass = row.Cells["tabdataclass"].Value.ToString();//状态

                    //string cell13 = row.Cells[13].Value == null ? "" : row.Cells[13].Value.ToString();
                    string linedata = sourceclass + "￥" + formno + "￥" + itemno + "￥" + itemnname + "￥" + sbno + "￥" + sbname + "￥" + gcno + "￥" + gcname + "￥" + lbclass + "￥" + djxmname + "￥" + specvalue + "￥" + result1 + "￥" + result2 + "￥" + status + "￥" + djkind + "￥" + maxvalue + "￥" + minvalue + "￥" + djclase + "￥" + djversion + "￥" + dataclass;
                    //string linedata = row.Cells[1].Value.ToString() + ";" + row.Cells[2].Value.ToString() + ";" + row.Cells[3].Value.ToString() + ";" + row.Cells[4].Value.ToString() + ";" + row.Cells[5].Value.ToString() + ";" + row.Cells[6].Value.ToString() + ";" + row.Cells[7].Value.ToString() + ";" + row.Cells[8].Value.ToString() + ";" + row.Cells[9].Value.ToString() + ";" + row.Cells[10].Value.ToString() + ";" + row.Cells[11].Value.ToString() + ";" + row.Cells[12].Value.ToString() + ";" + cell13 + ";" + row.Cells[14].Value.ToString() + ";" + row.Cells[15].Value.ToString() + ";" + row.Cells[16].Value.ToString() + ";" + row.Cells[17].Value.ToString() + ";" + row.Cells[18].Value.ToString() + ";" + row.Cells[19].Value.ToString() + ";" + row.Cells[20].Value.ToString() + ";" + djkind;
                    sb.AppendLine(linedata);
                }

                FileStream fs = new FileStream(path, FileMode.Create);
                byte[] bt = Encoding.UTF8.GetBytes(sb.ToString());
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bt, 0, bt.Length);
                fs.Flush();
                fs.Close();
                LogHelper.Info("Save checklist file success.");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        private bool ReadCheckListFile()
        {
            try
            {
                string path = @"CheckList.txt";
                if (File.Exists(path))
                {
                    string[] linelist = File.ReadAllLines(path);
                    string datetimespan = linelist[0];
                    string workorder = linelist[1];
                    Supervisor = Convert.ToBoolean(linelist[2]);
                    IPQC = Convert.ToBoolean(linelist[3]);
                    sequece = linelist[4];
                    TimeSpan span = DateTime.Now - Convert.ToDateTime(datetimespan);

                    if (span.TotalMinutes > Convert.ToInt32(config.RESTORE_TIME))//判断是否大于10分钟，大于10分钟则不自动点检
                    {
                        return false;
                    }
                    else
                    {
                        string[] workorders = workorder.Split(';');
                        if (workorders.Length > 1)
                        {
                            if (workorders[0] == this.txbCDAMONumber.Text)//判断工单是否有变化，无变化则自动点检
                            {
                                //if (workorders[1] == initModel.currentSettings.processLayer.ToString())//判断面次是否有变化
                                //{
                                #region setup checklist
                                int seq = 1;
                                if (linelist.Count() <= 6)
                                    return false;
                                this.dgvCheckListTable.Rows.Clear();
                                for (int i = 5; i < linelist.Count(); i++)
                                {
                                    string line = linelist[i];
                                    if (string.IsNullOrEmpty(line.Trim()))
                                        continue;

                                    string[] datas = line.Split('￥');
                                    object[] objValues = new object[] { seq, datas[17], datas[9], datas[7], datas[10], datas[11], "", datas[13], datas[14], datas[6], datas[15], datas[16], datas[8], datas[0], datas[1], datas[2], datas[3], datas[4], datas[5], datas[18], datas[19], "" };
                                    this.dgvCheckListTable.Rows.Add(objValues);
                                    seq++;
                                    SetCheckListInputStatusTable();
                                    if (datas[14] == "判断值")
                                    {
                                        string[] strInputValues = new string[] { "Y", "N" };
                                        DataTable dtInput = new DataTable();
                                        dtInput.Columns.Add("name");
                                        dtInput.Columns.Add("value");
                                        DataRow rowEmpty = dtInput.NewRow();
                                        rowEmpty["name"] = "";
                                        rowEmpty["value"] = "";
                                        dtInput.Rows.Add(rowEmpty);
                                        foreach (var strValues in strInputValues)
                                        {
                                            DataRow row = dtInput.NewRow();
                                            row["name"] = strValues;
                                            row["value"] = strValues;
                                            dtInput.Rows.Add(row);
                                        }

                                        DataGridViewComboBoxCell ComboBoxCell = new DataGridViewComboBoxCell();
                                        ComboBoxCell.DataSource = dtInput;
                                        ComboBoxCell.DisplayMember = "Name";
                                        ComboBoxCell.ValueMember = "Value";
                                        dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabResult2"] = ComboBoxCell;
                                    }
                                    dgvCheckListTable.Rows[this.dgvCheckListTable.Rows.Count - 1].Cells["tabResult2"].Value = datas[12];
                                    this.dgvCheckListTable.ClearSelection();

                                }
                                foreach (DataGridViewRow row in dgvCheckListTable.Rows)
                                {
                                    if (row.Cells["tabStatus"].Value.ToString() == "OK")
                                    {
                                        row.Cells["tabStatus"].Style.BackColor = Color.FromArgb(0, 192, 0);
                                    }
                                    else if ((row.Cells["tabStatus"].Value.ToString() == "NG"))
                                    {
                                        row.Cells["tabStatus"].Style.BackColor = Color.Red;
                                    }

                                }
                                return true;
                                #endregion
                                //}
                                //else
                                //{
                                //    return false;
                                //}
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
                return false;
            }
        }
        string strShift = "";
        private void WriteIntoShift2()
        {
            string datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            strShift = datetime;
            string path = @"CheckListShiftTemp.txt";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(datetime + ";" + this.txbCDAMONumber.Text);
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            byte[] bt = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Seek(0, SeekOrigin.Begin);
            fs.Write(bt, 0, bt.Length);
            fs.Flush();
            fs.Close();
        }

        //检查有没有到换班时间，如果到换班时间
        string strShiftChecklist = "";
        private bool CheckShiftChange2()
        {
            try
            {
                bool isValid = false;
                if (strShiftChecklist == "")
                    return false;

                string[] shifchangetimes = config.SHIFT_CHANGE_TIME.Split(';');
                List<string> shiftList = new List<string>();
                string nowDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                for (int i = 0; i < shifchangetimes.Length; i++)
                {

                    shiftList.Add(DateTime.Now.ToString("yyyy/MM/dd ") + shifchangetimes[i].Substring(0, 2) + ":" + shifchangetimes[i].Substring(2, 2));

                }

                shiftList.Sort();

                for (int j = shiftList.Count - 1; j < shiftList.Count; j--)
                {
                    if (j == -1)
                        break;
                    LogHelper.Debug("shift time: " + shiftList[j]);
                    string shitftime = shiftList[j];

                    if (Convert.ToDateTime(nowDate) > Convert.ToDateTime(shiftList[j])) //当前时间与设定的时间做比较，如果到换班时间则比较上次点检的时间
                    {
                        if (Convert.ToDateTime(strShiftChecklist) > Convert.ToDateTime(shitftime))
                        {
                            isValid = true;
                        }
                        break;
                    }
                    else
                    {
                        if (Convert.ToDateTime(strShiftChecklist).ToString("yyyy/MM/dd") != Convert.ToDateTime(nowDate).ToString("yyyy/MM/dd"))//add by qy
                        {
                            string covert_datetime = nowDate;
                            if (j == shiftList.Count - 1)
                            {
                                covert_datetime = shiftList[j - 1];
                            }
                            else if (j == 0)
                            {
                                covert_datetime = shiftList[j];
                            }
                            if (Convert.ToDateTime(strShiftChecklist) < Convert.ToDateTime(nowDate) && Convert.ToDateTime(nowDate) < Convert.ToDateTime(covert_datetime))
                            {
                                isValid = true;
                            }
                            break;
                        }

                        //if (Convert.ToDateTime(strShiftChecklist).ToString("yyyy/MM/dd") != Convert.ToDateTime(nowDate).ToString("yyyy/MM/dd"))//add by qy
                        //{
                        //    shitftime = Convert.ToDateTime(shitftime).AddDays(-1).ToString("yyyy/MM/dd HH:mm:ss");

                        //    if (Convert.ToDateTime(strShiftChecklist) > Convert.ToDateTime(shitftime))
                        //    {
                        //        isValid = true;
                        //    }
                        //    break;
                        //}
                    }
                }

                return isValid;
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex.Message, ex);
                return false;
            }

        }

        private void InitShift2(string wo)
        {
            string path = @"CheckListShiftTemp.txt";
            if (File.Exists(path))
            {
                string[] content = File.ReadAllLines(path);

                foreach (var item in content)
                {
                    if (item != "")
                    {
                        string[] items = item.Split(';');
                        //if (items[1] == wo)
                        //{
                        strShiftChecklist = items[0];
                        break;
                        //}
                    }
                }
            }
        }
        DateTime next_checklist_time = DateTime.Now;
        string checklist_freq_time = "";
        private void InitWorkOrderType()
        {

            Dictionary<string, string> dicfreq = new Dictionary<string, string>();
            string CHECKLIST_FREQ = config.CHECKLIST_FREQ;
            string[] freqs = CHECKLIST_FREQ.Split(';');
            foreach (var item in freqs)
            {
                string[] items = item.Split(',');
                string key = items[0];
                if (key == "")
                    key = "OTHERS";
                dicfreq[key] = items[1];
            }

            GetAttributeValue getAttriHandler = new GetAttributeValue(sessionContext, initModel, this);
            string[] valuesAttri = getAttriHandler.GetAttributeValueForAll(1, this.txbCDAMONumber.Text, "-1", "WORKORDER_TYPE");
            if (valuesAttri != null && valuesAttri.Length > 0)
            {
                string value = valuesAttri[1];

                if (CHECKLIST_FREQ.Contains(value))
                {
                    checklist_freq_time = dicfreq[value];
                }
                else
                {
                    checklist_freq_time = dicfreq["OTHERS"];
                }
            }
            else
            {
                checklist_freq_time = dicfreq["OTHERS"];
            }
            if (strShiftChecklist != "")
            {
                next_checklist_time = Convert.ToDateTime(strShiftChecklist).AddMinutes(double.Parse(checklist_freq_time) * 60);
            }
            else
            {
                next_checklist_time = DateTime.Now.AddMinutes(double.Parse(checklist_freq_time) * 60);
            }

        }

        private void InitProductionChecklist()
        {
            if (DateTime.Now > next_checklist_time)
            {
                if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
                {
                    InitTaskData_SOCKET("过程点检");
                    isStartLineCheck = false;
                    next_checklist_time = DateTime.Now.AddMinutes(double.Parse(checklist_freq_time) * 60);
                }
            }
        }

        public void GetRestoreTimerStart()
        {

            if (RestoreMaterialTimer != null && RestoreMaterialTimer.Enabled)
                return;
            RestoreMaterialTimer = new System.Timers.Timer();
            // 循环间隔时间(1分钟)
            RestoreMaterialTimer.Interval = Convert.ToInt32(config.RESTORE_TREAD_TIMER) * 1000;
            // 允许Timer执行
            RestoreMaterialTimer.Enabled = true;
            // 定义回调
            RestoreMaterialTimer.Elapsed += new ElapsedEventHandler(RestoreMaterialTimer_Elapsed);
            // 定义多次循环
            RestoreMaterialTimer.AutoReset = true;
        }

        private void RestoreMaterialTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveCheckList();
            InitProductionChecklist();
            InitShiftCheckList();
        }

        bool IsGetShiftCheckList = false;
        private void InitShiftCheckList()
        {
            if (config.CHECKLIST_SOURCE.ToUpper() == "TABLE")
            {
                //InitShift2(txbCDAMONumber.Text);
                if (!CheckShiftChange2())
                {
                    if (this.dgvCheckListTable.Rows.Count <= 0 || (this.dgvCheckListTable.Rows.Count > 0 && !isStartLineCheck))//!IsShiftCheck()
                    {
                        InitTaskData_SOCKET("开线点检;设备点检");
                        isStartLineCheck = true;
                    }
                }
            }
        }
        private bool IsShiftCheck()//true 表示已经带出开线点检的内容了
        {
            bool isValid = false;
            foreach (DataGridViewRow row in this.dgvCheckListTable.Rows)
            {
                if (row.Cells["tabdjclass"].Value.ToString() == "开线点检")
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }

        public void OpenScanPort()
        {
            initModel.scannerHandler = new ScannerHeandler(initModel, this);
            initModel.scannerHandler.heandler().DataReceived += new SerialDataReceivedEventHandler(DataRecivedHeandler);
            initModel.scannerHandler.heandler().Open();
        }
        #endregion


        #region 手动打叉板
        private void txtRow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                if (!VerifyModel())
                {
                    return;
                }

                if (!VerifyActivatedWO() || !VerifyCheckList() || !VerifyEquipment() || !VerifyMaterial())
                {
                    return;
                }
                decimal column = Convert.ToDecimal(this.lblpanelQty.Text) / Convert.ToDecimal(this.txtRow.Text);
                string a = column.ToString();
                string regular = @"^\d{1,10}$";
                Match match = Regex.Match(a, regular);
                if (match.Success)
                {
                    this.lblColumnQty.Text = a;
                }
                else
                {
                    errorHandler(2, Message("msg_row qty error"), "");
                    return;
                }

                InitFixtureGUI();
            }
        }
        private void InitFixtureGUI()
        {
            this.pnlScrap.Controls.Clear();
            string count = this.lblpanelQty.Text; //连板数
            int labelcount = 1;
            int rowcount = Convert.ToInt32(this.txtRow.Text);
            int columncount = Convert.ToInt32(this.lblColumnQty.Text);

            for (int i = 0; i < Convert.ToInt32(count); i++)
            {
                Label label = new Label();
                label.Text = Convert.ToString(i + 1);
                label.Name = "lable";
                label.Size = new Size(70, 50);
                label.Font = new Font("Segoe UI", 15);
                label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                label.BackColor = Color.Green;
                label.Click += new System.EventHandler(lable_Click);

                if (labelcount <= columncount)
                {
                    int x = 10 + 71 * (labelcount - 1);
                    int y = 14 * rowcount * 4;
                    label.Location = new Point(x, y);
                }

                label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                this.pnlScrap.Controls.Add(label);
                if (labelcount == columncount)
                {
                    labelcount = 1;
                    rowcount--;
                }
                else
                {
                    labelcount++;
                }

            }
        }
        List<string> manualScraplist = new List<string>();
        private void lable_Click(Object sender, EventArgs e)
        {
            Label lbl = (Label)sender;
            if (lbl.BackColor == Color.Green)
            {
                lbl.BackColor = System.Drawing.Color.Red;//控件的背景颜色
                manualScraplist.Add(lbl.Text);
            }
            else
            {
                lbl.BackColor = System.Drawing.Color.Green;//控件的背景颜色
                manualScraplist.Remove(lbl.Text);
            }

        }

        private void ResetScrapGUI()
        {
            for (int i = 0; i < this.pnlScrap.Controls.Count; i++)
            {
                this.pnlScrap.Controls[i].BackColor = Color.Green;
            }
            manualScraplist.Clear();
            isConfirmScrap = false;
            pnlScrap.Enabled = true;
            errorHandler(0, Message("msg_scrap board has been cancled"), "");
        }
        private void btnScrapCancel_Click(object sender, EventArgs e)
        {
            ResetScrapGUI();
        }
        private void InitScrapGUI()
        {
            if (initModel.numberOfSingleBoards.ToString() != this.lblpanelQty.Text)
            {
                //add by qy 20170401 将连板数显示在打叉板页面的栏位上
                this.lblpanelQty.Text = initModel.numberOfSingleBoards.ToString();
                this.txtRow.Text = "";
                this.lblColumnQty.Text = "0";
                this.pnlScrap.Controls.Clear();
            }
        }

        private void ckbIsNeedScrap_CheckedChanged(object sender, EventArgs e)
        {

        }
        bool isConfirmScrap = false;
        private void btnOKScrap_Click(object sender, EventArgs e)
        {
            try
            {
                if (config.BAD_BOARD_AUTO_RESET.ToUpper() == "ENABLE")
                {
                    if (BoardCome)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            isOK = ProcessSerialNumberData();
                        }));
                        if (isOK)
                        {
                            SendMsessageToPFC(PFCMessage.GO, "");
                            BoardCome = false;
                        }
                    }
                }
                else
                {
                    isConfirmScrap = true;
                    pnlScrap.Enabled = false;
                    errorHandler(0, Message("msg_scrap board confirm OK"), "");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }

        }
        #endregion
    }
}
