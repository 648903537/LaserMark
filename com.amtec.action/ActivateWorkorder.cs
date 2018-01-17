using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class ActivateWorkorder
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private int error;
        private MainView view;
        private InitModel init;

        public ActivateWorkorder(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int ActivateWorkorderResultcall(string workorder, int processLayer)
        {
            SetUpManager setupHandler = new SetUpManager(sessionContext, init, view);
            setupHandler.SetupStateChange(workorder, 2, processLayer);
            int activationResult = imsapi.trActivateWorkOrder(sessionContext, init.configHandler.StationNumber, workorder, "-1", "-1", processLayer, 2);//1 = Activate work order for the station only;2 = Activate work order for entire line
            LogHelper.Info("Api trActivateWorkOrder: error code =" + activationResult);
            if (activationResult == 0)
            {
                error = activationResult;
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trActivateWorkOrder " + activationResult, "");
            }
            else
            {
                error = activationResult;
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trActivateWorkOrder " + activationResult, "");
            }
            return activationResult;
        }

        public int ActivateWorkorderExt(string stationNumber, string workorder, int flag)
        {
            //get process layer from station number
            string processLayer = GetProcessLayerByWP(stationNumber, workorder);
            if (processLayer != null)
            {
                int iprocessLayer = int.Parse(processLayer);
                //Delete setup
                SetupStateChange(workorder, 2, iprocessLayer);
                //activate wo
                int activationResult = imsapi.trActivateWorkOrder(sessionContext, stationNumber, workorder, "-1", "-1", iprocessLayer, flag);//1 = Activate work order for the station only;2 = Activate work order for entire line
                LogHelper.Info("Api trActivateWorkOrder: error code =" + activationResult);
                if (activationResult == 0)
                {
                    view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trActivateWorkOrder " + activationResult, "");
                }
                else
                {
                    view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trActivateWorkOrder " + activationResult, "");
                }
                return activationResult;
            }
            view.errorHandler(2, "Get process layer from work plan error.", "");
            return -99;
        }

        /// <summary>
        /// 获取该工单在站点的操作位置(mdataGetWorkplanData中的PROCESS_LAYER栏位,PROCESS_LAYER工作面,即正面/反面/不分面)
        /// </summary>
        /// <param name="stationNumber"></param>
        /// <param name="workorder"></param>
        /// <returns></returns>
        public string GetProcessLayerByWP(string stationNumber, string workorder)
        {
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("WORKSTEP_FLAG", "1") };
            string[] workplanDataResultKeys = new string[] { "ERP_GROUP_NUMBER", "PROCESS_LAYER" };
            string[] workplanDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetWorkplanData (Work Order:" + workorder + ",station number =" + stationNumber + ")");
            int error = imsapi.mdataGetWorkplanData(sessionContext, stationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("end api mdataGetWorkplanData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
                return null;
            }
            //打印控制台
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");

            if (error == 0)
            {
                string processLayer = workplanDataResultValues[1];
                LogHelper.Info("Process Layer = " + processLayer);
                return processLayer;
            }
            return null;
        }

        private int SetupStateChange(string workorder, int activateFlag, int processLayer)
        {
            int error = 0;
            //0 = Activate setup
            //1 = Deactivate setup
            //2 = Delete setup
            error = imsapi.setupStateChange(sessionContext, init.configHandler.StationNumber, processLayer, workorder, "-1", -1, activateFlag);
            LogHelper.Info("Api setupStateChange: error code =" + error);
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " setupStateChange " + error, "");
            }
            else
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " setupStateChange " + error, "");
            }
            return error;
        }
    }
}
