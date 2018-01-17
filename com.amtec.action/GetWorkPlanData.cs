using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System.Collections.Generic;
using System.Data;

namespace com.amtec.action
{
    public class GetWorkPlanData
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public GetWorkPlanData(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public List<WorkplanEntity> GetWorkStepDataByStation(string workorder)
        {
            List<WorkplanEntity> entityList = new List<WorkplanEntity>();
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("WORKSTEP_FLAG", "1") };
            string[] workplanDataResultKeys = new string[] { "CYCLE_TIME_MACHINE", "CYCLE_TIME_USER", "EQUIPMENT_AVAILABLE", "ERP_CHANGE_NUMBER", "ERP_GROUP_DESC", "ERP_GROUP_NUMBER", "MAX_TEST_COUNT", "MDA_DOCUMENT_AVAILABLE" ,
            "MSL_OFFSET","MSL_RELEVANT","MULTIPLE_COUNT","OBLIGATORY_CONFIRM_FLAG","PROCESS_LAYER","SEPARATION_FLAG","SETUP_FLAG","SETUP_TIME_MACHINE","SETUP_TIME_USER","SKILL_DESC","SKILL_LEVEL","STATION_DESC","STATION_NUMBER",
            "VOUCHER_NUMBER","WORKSTEP_AG","WORKSTEP_AVO","WORKSTEP_DESC","WORKSTEP_INFO","WORKSTEP_NUMBER","WORKSTEP_NUMBER_ALT"};
            string[] workplanDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetWorkplanData (Work Order:" + init.currentSettings.workorderNumber + ")");
            int error = imsapi.mdataGetWorkplanData(sessionContext, init.configHandler.StationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("end api mdataGetWorkplanData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
                return null;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
            if (error == 0)
            {
                int loop = workplanDataResultKeys.Length;
                int count = workplanDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    WorkplanEntity entity = new WorkplanEntity();
                    entity.CYCLE_TIME_MACHINE = workplanDataResultValues[i];
                    entity.CYCLE_TIME_USER = workplanDataResultValues[i + 1];
                    entity.EQUIPMENT_AVAILABLE = workplanDataResultValues[i + 2];
                    entity.ERP_CHANGE_NUMBER = workplanDataResultValues[i + 3];
                    entity.ERP_GROUP_DESC = workplanDataResultValues[i + 4];
                    entity.ERP_GROUP_NUMBER = workplanDataResultValues[i + 5];
                    entity.MAX_TEST_COUNT = workplanDataResultValues[i + 6];
                    entity.MDA_DOCUMENT_AVAILABLE = workplanDataResultValues[i + 7];
                    entity.MSL_OFFSET = workplanDataResultValues[i + 8];
                    entity.MSL_RELEVANT = workplanDataResultValues[i + 9];
                    entity.MULTIPLE_COUNT = workplanDataResultValues[i + 10];
                    entity.OBLIGATORY_CONFIRM_FLAG = workplanDataResultValues[i + 11];
                    entity.PROCESS_LAYER = workplanDataResultValues[i + 12];
                    entity.SEPARATION_FLAG = workplanDataResultValues[i + 13];
                    entity.SETUP_FLAG = workplanDataResultValues[i + 14];
                    entity.SETUP_TIME_MACHINE = workplanDataResultValues[i + 15];
                    entity.SETUP_TIME_USER = workplanDataResultValues[i + 16];
                    entity.SKILL_DESC = workplanDataResultValues[i + 17];
                    entity.SKILL_LEVEL = workplanDataResultValues[i + 18];
                    entity.STATION_DESC = workplanDataResultValues[i + 19];
                    entity.STATION_NUMBER = workplanDataResultValues[i + 20];
                    entity.VOUCHER_NUMBER = workplanDataResultValues[i + 21];
                    entity.WORKSTEP_AG = workplanDataResultValues[i + 22];
                    entity.WORKSTEP_AVO = workplanDataResultValues[i + 23];

                    entity.WORKSTEP_DESC = workplanDataResultValues[i + 24];
                    entity.WORKSTEP_INFO = workplanDataResultValues[i + 25];
                    entity.WORKSTEP_NUMBER = workplanDataResultValues[i + 26];
                    entity.WORKSTEP_NUMBER_ALT = workplanDataResultValues[i + 27];
                    entityList.Add(entity);
                }
            }
            return entityList;
        }

        public DataTable GetWorkPlanDetail(string workorder)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AttributeName", typeof(string));
            dt.Columns.Add("AttributeValue", typeof(string));
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("WORKSTEP_FLAG", "1") };
            string[] workplanDataResultKeys = new string[] { "CYCLE_TIME_MACHINE", "CYCLE_TIME_USER", "EQUIPMENT_AVAILABLE", "ERP_CHANGE_NUMBER", "ERP_GROUP_DESC", "ERP_GROUP_NUMBER", "MAX_TEST_COUNT", "MDA_DOCUMENT_AVAILABLE" ,
            "MSL_OFFSET","MSL_RELEVANT","MULTIPLE_COUNT","OBLIGATORY_CONFIRM_FLAG","PROCESS_LAYER","SEPARATION_FLAG","SETUP_FLAG","SETUP_TIME_MACHINE","SETUP_TIME_USER","SKILL_DESC","SKILL_LEVEL","STATION_DESC","STATION_NUMBER",
            "VOUCHER_NUMBER","WORKSTEP_AG","WORKSTEP_AVO","WORKSTEP_DESC","WORKSTEP_INFO","WORKSTEP_NUMBER","WORKSTEP_NUMBER_ALT"};
            string[] workplanDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetWorkplanData (Work Order:" + init.currentSettings.workorderNumber + ")");
            int error = imsapi.mdataGetWorkplanData(sessionContext, init.configHandler.StationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("end api mdataGetWorkplanData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
                return null;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
            if (error == 0)
            {
                for (int j = 0; j < workplanDataResultValues.Length; j++)
                {
                    DataRow row = dt.NewRow();
                    row["AttributeName"] = workplanDataResultKeys[j];
                    row["AttributeValue"] = workplanDataResultValues[j];
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

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
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
            if (error == 0)
            {
                string processLayer = workplanDataResultValues[1];
                return processLayer;
            }
            return null;
        }

        public string GetWorkStepInfobyWorkPlan(string workorder, int processlayer)
        {
            string workStep_text = "";
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("FUNC_MODE", "0"), new KeyValue("PROCESS_LAYER", processlayer.ToString()), new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("WORKSTEP_FLAG", "1") };
            string[] workplanDataResultKeys = new string[] { "WORKSTEP_INFO" };
            string[] workplanDataResultValues = new string[] { };
            int error = imsapi.mdataGetWorkplanData(sessionContext, init.configHandler.StationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("end api mdataGetWorkplanData (workorder:" + workorder + ",processlayer:" + processlayer + ",result code = " + error + ")");
            if (error != 0)
            {
            }
            else
            {
                if (workplanDataResultValues.Length > 0)
                    workStep_text = workplanDataResultValues[0];
            }
            return workStep_text;
        }
    }
}
