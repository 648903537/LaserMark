using com.amtec.configurations;
using com.amtec.device;
using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace com.amtec.action
{
    public class InitializeMainGUI
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private ApplicationConfiguration config;
        private InitModel initModel;
        private MainView view;
        private LanguageResources lang;
        private bool isInitializeSucces = true;
        FileSystemWatcher watcher = new FileSystemWatcher();

        public InitializeMainGUI(IMSApiSessionContextStruct sessionContext, ApplicationConfiguration config, MainView view, LanguageResources lang)
        {
            this.sessionContext = sessionContext;
            this.config = config;
            this.view = view;
            this.lang = lang;
        }

        public InitModel Initialize()
        {
            initModel = new InitModel();

            try
            {
                initModel.configHandler = config;
                initModel.lang = lang;
            }
            catch
            {
                view.errorHandler(2, lang.ERROR_CONFIG_ERROR, lang.ERROR_CONFIG_ERROR);
                isInitializeSucces = false;
            }

            try
            {
                if (config.SerialPort != "")
                {
                    initModel.scannerHandler = new ScannerHeandler(initModel, view);
                    initModel.scannerHandler.heandler().DataReceived += new SerialDataReceivedEventHandler(view.DataRecivedHeandler);
                    initModel.scannerHandler.heandler().Open();
                    view.errorHandler(0, lang.ERROR_SCANNER_PORT_OPEN, "");
                }
            }
            catch (Exception e)
            {
                view.errorHandler(2, lang.ERROR_SCANNER_PORT_CLOSE, "");
                isInitializeSucces = false;
                LogHelper.Error(e.Message);
            }

            try
            {
                GetCurrentWorkorder currentWorkorder = new GetCurrentWorkorder(sessionContext, initModel, view);
                initModel.currentSettings = currentWorkorder.GetCurrentWorkorderResultCall();
            }
            catch (Exception ex)
            {
                view.errorHandler(2, "current setting error", "current setting error");
                isInitializeSucces = false;
                LogHelper.Error(ex.Message);
            }
            //read error code ZHS from excel
            try
            {
                string[] LineList = File.ReadAllLines(@"ErrorCodeZH.csv", Encoding.Default);
                Dictionary<int, string> dicErrorCodeMapping = new Dictionary<int, string>();
                if (LineList == null || LineList.Length == 0)
                { }
                else
                {
                    for (int i = 1; i < LineList.Length; i++)
                    {
                        string linecontent = LineList[i].Trim();
                        string[] linegroups = linecontent.Split(new char[] { ',' });
                        int iErrorCode = Convert.ToInt32(linegroups[0].Trim());
                        string strECDesc = linegroups[2].ToString();
                        if (!dicErrorCodeMapping.ContainsKey(iErrorCode))
                            dicErrorCodeMapping[iErrorCode] = strECDesc;
                    }
                }
                initModel.ErrorCodeZHS = dicErrorCodeMapping;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            if (initModel.currentSettings != null)
            {
                try
                {
                    //GetNumbersOfSingleBoards getNumBoard = new GetNumbersOfSingleBoards(sessionContext, initModel, view);
                    //List<MdataGetPartData> listData = getNumBoard.GetNumbersOfSingleBoardsResultCall(initModel.currentSettings.partNumber);
                    //if (listData != null && listData.Count > 0)
                    //{
                    //    MdataGetPartData mData = listData[0];
                    //    initModel.numberOfSingleBoards = mData.quantityMultipleBoard;
                    //}
                }
                catch (Exception ex)
                {
                    view.errorHandler(2, "current setting error", "current setting error");
                    isInitializeSucces = false;
                    LogHelper.Error(ex.Message);
                    throw;
                }

                try
                {
                    switch (initModel.currentSettings.getError)
                    {
                        case 0:
                            view.Invoke(new MethodInvoker(delegate
                            {
                                view.getFieldPartNumber().Text = initModel.currentSettings.partNumber;
                                view.getFieldWorkorder().Text = initModel.currentSettings.workorderNumber;
                            }));
                            break;

                        default:
                            isInitializeSucces = false;
                            return initModel;
                    }
                }
                catch
                {
                    view.errorHandler(2, lang.ERROR_STATION_SETTINGS_ERROR, lang.ERROR_STATION_SETTINGS_ERROR);
                    isInitializeSucces = false;
                    return initModel;
                }
            }

            try
            {
                ListenerFolder(config.LogFileFolder);
            }
            catch (Exception ex1)
            {
                isInitializeSucces = false;
                LogHelper.Error(ex1);
            }

            if (isInitializeSucces)
            {
                view.errorHandler(0, initModel.lang.ERROR_INITIALIZE_SUCCESS, initModel.lang.ERROR_INITIALIZE_SUCCESS);
                view.SetStatusLabelText(initModel.lang.ERROR_INITIALIZE_SUCCESS);
            }
            else
            {
                view.errorHandler(3, initModel.lang.ERROR_INITIALIZE_ERROR, initModel.lang.ERROR_INITIALIZE_ERROR);
                view.SetStatusLabelText(initModel.lang.ERROR_INITIALIZE_ERROR);
            }
            return initModel;
        }

        private void ListenerFolder(string path)
        {
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.FileName; //| NotifyFilters.| NotifyFilters.FileName
            watcher.Filter = "*.trace"; //设定监听的文件类型           
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.EnableRaisingEvents = true;
            view.errorHandler(0, "ListenerFolder Started. " + path, "");
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            try
            {
                string filename = e.Name;
                string filepath = e.FullPath;
                view.errorHandler(0, "Listener filename:" + filename + " start!", "");
                Thread.Sleep(Convert.ToInt32(config.WaitTime));//等待10s
                try
                {
                    view.ListenFile(filepath);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.Message, ex);
                    view.errorHandler(3, "Read error." + ex.Message, "");
                }

            }
            catch (Exception ex)
            {
                view.errorHandler(3, "Read error." + ex.Message, "");
            }
        }
    }
}
