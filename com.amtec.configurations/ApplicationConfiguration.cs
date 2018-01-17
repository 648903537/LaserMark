using com.amtec.action;
using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace com.amtec.configurations
{
    public class ApplicationConfiguration
    {
        public String StationNumber { get; set; }

        public String Client { get; set; }

        public String RegistrationType { get; set; }

        public String SerialPort { get; set; }

        public String BaudRate { get; set; }

        public String Parity { get; set; }

        public String StopBits { get; set; }

        public String DataBits { get; set; }

        public String NewLineSymbol { get; set; }

        public String DLExtractPattern { get; set; }

        public String MBNExtractPattern { get; set; }

        public String MDAPath { get; set; }

        public String EquipmentExtractPattern { get; set; }

        public String OpacityValue { get; set; }

        public String LocationXY { get; set; }

        public String MaterialWarningQty { get; set; }

        public String IPAdress { get; set; }

        public String Port { get; set; }

        public String LogInType { get; set; }

        public String LoadExtractPattern { get; set; }

        public String Language { get; set; }

        public String CheckListFolder { get; set; }

        public String BJIPath { get; set; }

        public String BCIPath { get; set; }

        public String LogFileFolder { get; set; }

        public String LogTransOK { get; set; }

        public String LogTransError { get; set; }

        public String ChangeFileName { get; set; }

        public String WaitTime { get; set; }
        public String UserTeam { get; set; }
        public String FileNamePattern { get; set; }

        public String FilterByFileName { get; set; }

        public String BackupsOKFile { get; set; }

        public String PRINTER_MODE { get; set; }
        public String LABEL_TEMPLATE_PATH { get; set; }
        public String LABEL_TEMPLATE_FILE { get; set; }
        public String DEFAULT_LABLE { get; set; }
        public String PrintSerialPort { get; set; }
        public String PrintBaudRate { get; set; }
        public String PrintParity { get; set; }
        public String PrintStopBits { get; set; }
        public String PrintDataBits { get; set; }
        public String PrintNewLineSymbol { get; set; }
        public String LABEL_QTY { get; set; }
        public String AutoNextMaterial { get; set; }
        public String LAYER_DISPLAY { get; set; }

        public String CHECKLIST_IPAddress { get; set; }
        public String CHECKLIST_Port { get; set; }
        public String CHECKLIST_SOURCE { get; set; }
        public String AUTH_CHECKLIST_APP_TEAM { get; set; }
        public String CHECKLIST_FREQ { get; set; }
        public String SHIFT_CHANGE_TIME { get; set; }
        public String RESTORE_TIME { get; set; }
        public String RESTORE_TREAD_TIMER { get; set; }
        public String BAD_BOARD_AUTO_RESET { get; set; }

        public String UPLOAD_NG_MODE { get; set; }

        Dictionary<string, string> dicConfig = null;

        public ApplicationConfiguration()
        {
            try
            {
                CommonModel commonModel = ReadIhasFileData.getInstance();
                XDocument config = XDocument.Load("ApplicationConfig.xml");
                StationNumber = commonModel.Station;
                Client = commonModel.Client;
                RegistrationType = commonModel.RegisterType;
                SerialPort = GetDescendants("SerialPort", config);//config.Descendants("SerialPort").First().Value;
                BaudRate = GetDescendants("BaudRate", config);//config.Descendants("BaudRate").First().Value;
                Parity = GetDescendants("Parity", config);//config.Descendants("Parity").First().Value;
                StopBits = GetDescendants("StopBits", config);//config.Descendants("StopBits").First().Value;
                DataBits = GetDescendants("DataBits", config);//config.Descendants("DataBits").First().Value;
                NewLineSymbol = GetDescendants("NewLineSymbol", config);// config.Descendants("NewLineSymbol").First().Value;
                MBNExtractPattern = GetDescendants("MBNExtractPattern", config);//config.Descendants("MBNExtractPattern").First().Value;
                EquipmentExtractPattern = GetDescendants("EquipmentExtractPattern", config);//config.Descendants("EquipmentExtractPattern").First().Value;
                OpacityValue = GetDescendants("OpacityValue", config);// config.Descendants("OpacityValue").First().Value;
                LocationXY = GetDescendants("LocationXY", config);// config.Descendants("LocationXY").First().Value;
                MaterialWarningQty = GetDescendants("MaterialWarningQty", config);// config.Descendants("MaterialWarningQty").First().Value;
                IPAdress = GetDescendants("IPAdress", config);// config.Descendants("IPAdress").First().Value;
                Port = GetDescendants("Port", config);//config.Descendants("Port").First().Value;
                LoadExtractPattern = GetDescendants("LoadExtractPattern", config);//config.Descendants("LoadExtractPattern").First().Value;
                LogInType = GetDescendants("LogInType", config);//config.Descendants("LogInType").First().Value;
                Language = GetDescendants("Language", config);//config.Descendants("Language").First().Value;
                CheckListFolder = GetDescendants("CheckListFolder", config);//config.Descendants("CheckListFolder").First().Value;
                MDAPath = GetDescendants("MDAPath", config);//config.Descendants("MDAPath").First().Value;
                BJIPath = GetDescendants("BJIPath", config);//config.Descendants("BJIPath").First().Value;
                BCIPath = GetDescendants("BCIPath", config);//config.Descendants("BCIPath").First().Value;
                LogFileFolder = GetDescendants("LogFileFolder", config);//config.Descendants("LogFileFolder").First().Value;
                LogTransOK = GetDescendants("LogTransOK", config);//config.Descendants("LogTransOK").First().Value;
                LogTransError = GetDescendants("LogTransError", config);// config.Descendants("LogTransError").First().Value;
                ChangeFileName = GetDescendants("ChangeFileName", config);// config.Descendants("ChangeFileName").First().Value;
                WaitTime = GetDescendants("WaitTime", config);//config.Descendants("WaitTime").First().Value;
                UserTeam = GetDescendants("AUTH_TEAM", config);//config.Descendants("UserTeam").First().Value;
                FilterByFileName = GetDescendants("FilterByFileName", config);//config.Descendants("FilterByFileName").First().Value;
                FileNamePattern = GetDescendants("FileNamePattern", config);//config.Descendants("FileNamePattern").First().Value;
                BackupsOKFile = GetDescendants("BackupsOKFile", config);//config.Descendants("FileNamePattern").First().Value;

                PRINTER_MODE = GetDescendants("OUTPUT_MODE", config);
                LABEL_TEMPLATE_PATH = GetDescendants("LABEL_TEMPLATE_PATH", config);
                LABEL_TEMPLATE_FILE = GetDescendants("LABEL_TEMPLATE_FILE", config);
                DEFAULT_LABLE = GetDescendants("DEFAULT_LABLE", config);
                PrintSerialPort = GetDescendants("PrintSerialPort", config);
                PrintBaudRate = GetDescendants("PrintBaudRate", config);
                PrintParity = GetDescendants("PrintParity", config);
                PrintStopBits = GetDescendants("PrintStopBits", config);
                PrintDataBits = GetDescendants("PrintDataBits", config);
                PrintNewLineSymbol = GetDescendants("PrintNewLineSymbol", config);
                LABEL_QTY = GetDescendants("LABEL_QTY", config);
                AutoNextMaterial = GetDescendants("MATERIAL_SPLICING", config);
                LAYER_DISPLAY = GetDescendants("LAYER_DISPLAY", config);

                CHECKLIST_IPAddress = GetDescendants("CHECKLIST_IPAddress", config);
                CHECKLIST_Port = GetDescendants("CHECKLIST_Port", config);
                CHECKLIST_SOURCE = GetDescendants("CHECKLIST_SOURCE", config);
                AUTH_CHECKLIST_APP_TEAM = GetDescendants("AUTH_CHECKLIST_APP_TEAM", config);
                CHECKLIST_FREQ = GetDescendants("CHECKLIST_FREQ", config);
                SHIFT_CHANGE_TIME = GetDescendants("SHIFT_CHANGE_TIME", config);
                RESTORE_TIME = GetDescendants("RESTORE_TIME", config);
                RESTORE_TREAD_TIMER = GetDescendants("RESTORE_TREAD_TIMER", config);
                BAD_BOARD_AUTO_RESET = GetDescendants("BAD_BOARD_AUTO_RESET", config);
                UPLOAD_NG_MODE = GetDescendants("UPLOAD_NG_MODE", config);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public ApplicationConfiguration(IMSApiSessionContextStruct sessionContext, MainView view)
        {
            try
            {
                dicConfig = new Dictionary<string, string>();
                ConfigManage configHandler = new ConfigManage(sessionContext, view);
                CommonModel commonModel = ReadIhasFileData.getInstance();
                if (commonModel.UpdateConfig == "L")
                {
                    XDocument config = XDocument.Load("ApplicationConfig.xml");
                    StationNumber = commonModel.Station;
                    Client = commonModel.Client;
                    RegistrationType = commonModel.RegisterType;
                    SerialPort = GetDescendants("SerialPort", config);//config.Descendants("SerialPort").First().Value;
                    BaudRate = GetDescendants("BaudRate", config);//config.Descendants("BaudRate").First().Value;
                    Parity = GetDescendants("Parity", config);//config.Descendants("Parity").First().Value;
                    StopBits = GetDescendants("StopBits", config);//config.Descendants("StopBits").First().Value;
                    DataBits = GetDescendants("DataBits", config);//config.Descendants("DataBits").First().Value;
                    NewLineSymbol = GetDescendants("NewLineSymbol", config);//config.Descendants("NewLineSymbol").First().Value;
                    MBNExtractPattern = GetDescendants("MBNExtractPattern", config);//config.Descendants("MBNExtractPattern").First().Value;
                    OpacityValue = GetDescendants("OpacityValue", config);// config.Descendants("OpacityValue").First().Value;
                    LocationXY = GetDescendants("LocationXY", config);// config.Descendants("LocationXY").First().Value;
                    MaterialWarningQty = GetDescendants("MaterialWarningQty", config);//config.Descendants("MaterialWarningQty").First().Value;
                    EquipmentExtractPattern = GetDescendants("EquipmentExtractPattern", config);// config.Descendants("EquipmentExtractPattern").First().Value;
                    IPAdress = GetDescendants("IPAdress", config);//config.Descendants("IPAdress").First().Value;
                    Port = GetDescendants("Port", config);// config.Descendants("Port").First().Value;
                    LoadExtractPattern = GetDescendants("LoadExtractPattern", config);//config.Descendants("LoadExtractPattern").First().Value;
                    LogInType = GetDescendants("LogInType", config);//config.Descendants("LogInType").First().Value;
                    Language = GetDescendants("Language", config);//config.Descendants("Language").First().Value;
                    CheckListFolder = GetDescendants("CheckListFolder", config);//config.Descendants("CheckListFolder").First().Value;
                    MDAPath = GetDescendants("MDAPath", config);//config.Descendants("MDAPath").First().Value;
                    BJIPath = GetDescendants("BJIPath", config);// config.Descendants("BJIPath").First().Value;
                    BCIPath = GetDescendants("BCIPath", config);//config.Descendants("BCIPath").First().Value;
                    LogFileFolder = GetDescendants("LogFileFolder", config);//config.Descendants("LogFileFolder").First().Value;
                    LogTransOK = GetDescendants("LogTransOK", config);//config.Descendants("LogTransOK").First().Value;
                    LogTransError = GetDescendants("LogTransError", config);//config.Descendants("LogTransError").First().Value;
                    ChangeFileName = GetDescendants("ChangeFileName", config);//config.Descendants("ChangeFileName").First().Value;
                    WaitTime = GetDescendants("WaitTime", config);//config.Descendants("WaitTime").First().Value;
                    UserTeam = GetDescendants("AUTH_TEAM", config);//config.Descendants("UserTeam").First().Value;
                    FilterByFileName = GetDescendants("FilterByFileName", config);//config.Descendants("FilterByFileName").First().Value;
                    FileNamePattern = GetDescendants("FileNamePattern", config);//config.Descendants("FileNamePattern").First().Value;
                    BackupsOKFile = GetDescendants("BackupsOKFile", config);//config.Descendants("FileNamePattern").First().Value;

                    PRINTER_MODE = GetDescendants("OUTPUT_MODE", config);
                    LABEL_TEMPLATE_PATH = GetDescendants("LABEL_TEMPLATE_PATH", config);
                    LABEL_TEMPLATE_FILE = GetDescendants("LABEL_TEMPLATE_FILE", config);
                    DEFAULT_LABLE = GetDescendants("DEFAULT_LABLE", config);
                    PrintSerialPort = GetDescendants("PrintSerialPort", config);
                    PrintBaudRate = GetDescendants("PrintBaudRate", config);
                    PrintParity = GetDescendants("PrintParity", config);
                    PrintStopBits = GetDescendants("PrintStopBits", config);
                    PrintDataBits = GetDescendants("PrintDataBits", config);
                    PrintNewLineSymbol = GetDescendants("PrintNewLineSymbol", config);
                    LABEL_QTY = GetDescendants("LABEL_QTY", config);
                    AutoNextMaterial = GetDescendants("MATERIAL_SPLICING", config);
                    LAYER_DISPLAY = GetDescendants("LAYER_DISPLAY", config);

                    CHECKLIST_IPAddress = GetDescendants("CHECKLIST_IPAddress", config);
                    CHECKLIST_Port = GetDescendants("CHECKLIST_Port", config);
                    CHECKLIST_SOURCE = GetDescendants("CHECKLIST_SOURCE", config);
                    AUTH_CHECKLIST_APP_TEAM = GetDescendants("AUTH_CHECKLIST_APP_TEAM", config);
                    CHECKLIST_FREQ = GetDescendants("CHECKLIST_FREQ", config);
                    SHIFT_CHANGE_TIME = GetDescendants("SHIFT_CHANGE_TIME", config);
                    RESTORE_TIME = GetDescendants("RESTORE_TIME", config);
                    RESTORE_TREAD_TIMER = GetDescendants("RESTORE_TREAD_TIMER", config);
                    BAD_BOARD_AUTO_RESET = GetDescendants("BAD_BOARD_AUTO_RESET", config);
                    UPLOAD_NG_MODE = GetDescendants("UPLOAD_NG_MODE", config);
                }
                else
                {
                    if (commonModel.UpdateConfig == "Y")
                    {
                        //int error = configHandler.DeleteConfigParameters(commonModel.APPTYPE);
                        //if (error == 0 || error == -3303 || error == -3302)
                        //{
                        //    WriteParameterToiTac(configHandler);
                        //}
                        string[] parametersValue = configHandler.GetParametersForScope(commonModel.APPTYPE);
                        if (parametersValue != null && parametersValue.Length > 0)
                        {
                            foreach (var parameterID in parametersValue)
                            {
                                configHandler.DeleteConfigParametersExt(parameterID);
                            }
                        }
                        WriteParameterToiTac(configHandler);
                    }
                    List<ConfigEntity> getvalues = configHandler.GetConfigData(commonModel.APPID, commonModel.APPTYPE, commonModel.Cluster, commonModel.Station);
                    if (getvalues != null)
                    {
                        foreach (var item in getvalues)
                        {
                            if (item != null)
                            {
                                string[] strs = item.PARAMETER_NAME.Split(new char[] { '.' });
                                dicConfig.Add(strs[strs.Length - 1], item.CONFIG_VALUE);
                                LogHelper.Info(strs[strs.Length - 1] + ": " + item.CONFIG_VALUE);
                            }
                        }
                    }

                    StationNumber = commonModel.Station;
                    Client = commonModel.Client;
                    RegistrationType = commonModel.RegisterType;
                    SerialPort = GetParameterValue("SerialPort");
                    BaudRate = GetParameterValue("BaudRate");
                    Parity = GetParameterValue("Parity");
                    StopBits = GetParameterValue("StopBits");
                    DataBits = GetParameterValue("DataBits");
                    NewLineSymbol = GetParameterValue("NewLineSymbol");
                    MBNExtractPattern = GetParameterValue("MBNExtractPattern");
                    OpacityValue = GetParameterValue("OpacityValue");
                    LocationXY = GetParameterValue("LocationXY");
                    MaterialWarningQty = GetParameterValue("MaterialWarningQty");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        private string GetParameterValue(string parameterName)
        {
            if (dicConfig.ContainsKey(parameterName))
            {
                return dicConfig[parameterName];
            }
            else
            {
                return "";
            }
        }

        private void WriteParameterToiTac(ConfigManage configHandler)
        {
            GetApplicationDatas getData = new GetApplicationDatas();
            List<ParameterEntity> entityList = getData.GetApplicationEntity();
            string[] strs = GetParameterString(entityList);
            string[] strvalues = GetValueString(entityList);
            if (strs != null && strs.Length > 0)
            {
                int errorCode = configHandler.CreateConfigParameter(strs);
                if (errorCode == 0 || errorCode == 5)
                {
                    CommonModel commonModel = ReadIhasFileData.getInstance();
                    int re = configHandler.UpdateParameterValues(commonModel.APPID, commonModel.APPTYPE, commonModel.Cluster, commonModel.Station, strvalues);
                }
            }

            //if (entityList.Count > 0)
            //{
            //    List<ParameterEntity> entitySubList = null;
            //    CommonModel commonModel = ReadIhasFileData.getInstance();
            //    foreach (var entity in entityList)
            //    {
            //        entitySubList = new List<ParameterEntity>();
            //        entitySubList.Add(entity);
            //        string[] strs = GetParameterString(entitySubList);
            //        string[] strvalues = GetValueString(entitySubList);
            //        if (strs != null && strs.Length > 0)
            //        {
            //            int errorCode = configHandler.CreateConfigParameter(strs);
            //            if (errorCode == 0 || errorCode == 5)
            //            {
            //                int re = configHandler.UpdateParameterValues(commonModel.APPID, commonModel.APPTYPE, commonModel.Cluster, commonModel.Station, strvalues);
            //            }
            //            else if (errorCode == -3301)//Parameter already exists
            //            {
            //                int re = configHandler.UpdateParameterValues(commonModel.APPID, commonModel.APPTYPE, commonModel.Cluster, commonModel.Station, strvalues);
            //            }
            //        }
            //    }
            //}
        }

        private string[] GetParameterString(List<ParameterEntity> entityList)
        {
            List<string> strList = new List<string>();
            foreach (var entity in entityList)
            {
                strList.Add(entity.PARAMETER_DESCRIPTION);
                strList.Add(entity.PARAMETER_DIMPATH);
                strList.Add(entity.PARAMETER_DISPLAYNAME);
                strList.Add(entity.PARAMETER_NAME);
                strList.Add(entity.PARAMETER_PARENT_NAME);
                strList.Add(entity.PARAMETER_SCOPE);
                strList.Add(entity.PARAMETER_TYPE_NAME);
            }
            return strList.ToArray();
        }

        private string[] GetValueString(List<ParameterEntity> entityList)
        {
            List<string> strList = new List<string>();
            foreach (var entity in entityList)
            {
                if (entity.PARAMETER_VALUE == "")
                    continue;
                strList.Add(entity.PARAMETER_VALUE);
                strList.Add(entity.PARAMETER_NAME);

            }
            return strList.ToArray();
        }

        private string GetDescendants(string parameter, XDocument _config)
        {
            try
            {
                string value = _config.Descendants(parameter).First().Value;

                return value;
            }
            catch
            {
                LogHelper.Error("Parameter is not exist." + parameter);
                return "";
            }
        }
    }
}
