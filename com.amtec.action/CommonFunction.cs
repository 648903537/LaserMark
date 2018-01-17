using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace com.amtec.action
{
    public class CommonFunction
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public CommonFunction(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int GetProcessLayerByWO(string workorder, string stationNumber)
        {
            int iProcessLayer = -1;
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("FUNC_MODE", "0"), new KeyValue("WORKSTEP_FLAG", "1"), new KeyValue("WORKORDER_NUMBER", workorder) };
            string[] workplanDataResultKeys = new string[] { "PROCESS_LAYER" };
            string[] workplanDataResultValues = new string[] { };
            int errorWP = imsapi.mdataGetWorkplanData(sessionContext, stationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("Api mdataGetWorkplanData: work order number =" + workorder + ", station number =" + stationNumber + ", result code =" + errorWP);
            if (errorWP == 0)
            {
                iProcessLayer = Convert.ToInt32(workplanDataResultValues[0]);
            }
            LogHelper.Info("Get Process Layer =" + iProcessLayer);
            return iProcessLayer;
        }

        public int UploadFailureAndResultData(string stationNumber, string serialNumber, string serialNumberPos, int processLayer, int serialNumberState, int duplicateSerialNumber
         , string[] measureValues, string[] failureValues)
        {
            string[] measureKeys = new string[] { "ERROR_CODE", "MEASURE_FAIL_CODE", "MEASURE_NAME", "MEASURE_VALUE" };
            string[] failureKeys = new string[] { "ERROR_CODE", "FAILURE_TYPE_CODE" };
            //string[] failureValues = new string[] { };
            string[] failureSlipKeys = new string[] { "ERROR_CODE", "TEST_STEP_NAME" };
            string[] failureSlipValues = new string[] { };
            string[] measureResultValues = new string[] { };
            string[] failureResultValues = new string[] { };
            string[] failureSlipResultValues = new string[] { };

            int error = imsapi.trUploadFailureAndResultData(sessionContext, stationNumber, processLayer, serialNumber, serialNumberPos,
                serialNumberState, duplicateSerialNumber, 0, -1, measureKeys, measureValues, out measureResultValues, failureKeys, failureValues, out failureResultValues,
                failureSlipKeys, failureSlipValues, out failureSlipResultValues);
            LogHelper.Info("Measurement data:");
            foreach (var item in measureValues)
            {
                LogHelper.Info(item);
            }
            LogHelper.Info("Failure data:");
            foreach (var item in failureValues)
            {
                LogHelper.Info(item);
            }
            LogHelper.Info("Api trUploadFailureAndResultData (station number:" + stationNumber + ", serial number:" + serialNumber + ",pos:" + serialNumberPos + ",process layer:" + processLayer + ",state:" + serialNumberState + ",error code:" + error);

            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trUploadFailureAndResultData " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trUploadFailureAndResultData " + error, "");
            }
            return error;
        }

        public int UploadProcessResultCall(string stationNumber, string serialNumber, int processLayer, int serialNumberState)
        {
            String[] serialNumberUploadKey = new String[] { "ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE" };
            String[] serialNumberUploadValues = new String[] { };
            String[] serialNumberResultValues = new String[] { };
            int error = imsapi.trUploadState(sessionContext, stationNumber, processLayer, serialNumber, "-1", serialNumberState, 0, -1, 0, serialNumberUploadKey, serialNumberUploadValues, out serialNumberResultValues);
            LogHelper.Info("Api trUploadState: station number=" + stationNumber + " serial number =" + serialNumber + ",process layer=" + processLayer + ", result code =" + error);
            if ((error != 0) && (error != 210))
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trUploadState " + error, "");
                return error;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trUploadState " + error, "");
            return error;
        }

        public string[] GetSNInfo(string serialNumber, string stationNumber)
        {
            int error = 0;
            string errorMsg = "";
            string[] serialNumberResultKeys = new string[] { "PART_DESC", "PART_NUMBER", "WORKORDER_NUMBER" };
            string[] serialNumberResultValues = new string[] { };
            LogHelper.Info("begin api trGetSerialNumberInfo (serial number =" + serialNumber + ",station number =" + stationNumber + ")");
            error = imsapi.trGetSerialNumberInfo(sessionContext, stationNumber, serialNumber, "-1", serialNumberResultKeys, out serialNumberResultValues);
            LogHelper.Info("end api trGetSerialNumberInfo (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trGetSerialNumberInfo " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trGetSerialNumberInfo " + error + "," + errorMsg, "");
            }
            return serialNumberResultValues;
        }

        public DataTable GetBomMaterialData(string workorder, string stationNumber)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ErpGroup", typeof(string)));
            dt.Columns.Add(new DataColumn("PartNumber", typeof(string)));
            dt.Columns.Add(new DataColumn("PartDesc", typeof(string)));
            dt.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dt.Columns.Add(new DataColumn("CompName", typeof(string)));
            dt.Columns.Add(new DataColumn("ProcessLayer", typeof(string)));
            dt.Columns.Add(new DataColumn("SetupFlag", typeof(string)));
            dt.Columns.Add(new DataColumn("ProductFlag", typeof(string)));

            KeyValue[] bomDataFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("BOM_ALTERNATIVE", "0"), new KeyValue("BOM_TYPE", "1") };
            string[] bomDataResultKeys = new string[] { "MACHINE_GROUP_NUMBER", "PART_NUMBER", "PART_DESC", "SETUP_FLAG", "QUANTITY", "COMP_NAME", "PROCESS_LAYER", "PRODUCT_FLAG" };
            string[] bomDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetBomData (Work Order:" + workorder + ",Station number:" + stationNumber + ")");
            int error = imsapi.mdataGetBomData(sessionContext, stationNumber, bomDataFilter, bomDataResultKeys, out bomDataResultValues);
            LogHelper.Info("end api mdataGetBomData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
                return null;
            }
            else
            {
                string machineGroupTemp = GetMachineGroup(stationNumber);
                int loop = bomDataResultKeys.Length;
                int count = bomDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (bomDataResultValues[i + 3] == "1" || bomDataResultValues[i + 7] == "1")
                    {
                        string strMachineGroup = bomDataResultValues[i].ToString();
                        if (machineGroupTemp == strMachineGroup)
                        {
                            DataRow row = dt.NewRow();
                            if (machineGroupTemp == strMachineGroup && bomDataResultValues[i + 3] == "1")
                            {
                                row["SetupFlag"] = "Y";
                            }
                            else
                            {
                                row["SetupFlag"] = "";
                            }
                            row["ErpGroup"] = bomDataResultValues[i].ToString();
                            row["PartNumber"] = bomDataResultValues[i + 1].ToString();
                            row["PartDesc"] = bomDataResultValues[i + 2].ToString();
                            row["Quantity"] = bomDataResultValues[i + 4].ToString();
                            row["CompName"] = bomDataResultValues[i + 5].ToString();
                            row["ProcessLayer"] = bomDataResultValues[i + 6].ToString();
                            row["ProductFlag"] = bomDataResultValues[i + 7].ToString();
                            dt.Rows.Add(row);
                        }
                    }
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
            }
            return dt;
        }

        public List<string> GetBomMaterialDataBySNSec(string serialNumber, string stationNumber)
        {
            List<string> partList = new List<string>();
            KeyValue[] bomDataFilter = new KeyValue[] { new KeyValue("SERIAL_NUMBER", serialNumber), new KeyValue("BOM_ALTERNATIVE", "0"), new KeyValue("BOM_TYPE", "1") };
            string[] bomDataResultKeys = new string[] { "MACHINE_GROUP_NUMBER", "PART_NUMBER", "SETUP_FLAG", "PRODUCT_FLAG" };
            string[] bomDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetBomData (Serial Number:" + serialNumber + ", Station number:" + stationNumber + ")");
            int error = imsapi.mdataGetBomData(sessionContext, stationNumber, bomDataFilter, bomDataResultKeys, out bomDataResultValues);
            LogHelper.Info("end api mdataGetBomData (result code = " + error + ")");
            if (error == 0)
            {
                string machineGroupTemp = GetMachineGroup(stationNumber);
                int loop = bomDataResultKeys.Length;
                int count = bomDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (bomDataResultValues[i + 2] == "1" && machineGroupTemp == bomDataResultValues[i] && bomDataResultValues[i + 3] != "1")
                    {
                        partList.Add(bomDataResultValues[i + 1]);
                        LogHelper.Info("Setup part from bom:" + bomDataResultValues[i + 1]);
                    }
                }
            }
            return partList;
        }

        public DataTable GetBomMaterialDataBySN(string serialNumber, string stationNumber)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ErpGroup", typeof(string)));
            dt.Columns.Add(new DataColumn("PartNumber", typeof(string)));
            dt.Columns.Add(new DataColumn("PartDesc", typeof(string)));
            dt.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dt.Columns.Add(new DataColumn("CompName", typeof(string)));
            dt.Columns.Add(new DataColumn("ProcessLayer", typeof(string)));
            dt.Columns.Add(new DataColumn("SetupFlag", typeof(string)));
            dt.Columns.Add(new DataColumn("ProductFlag", typeof(string)));

            KeyValue[] bomDataFilter = new KeyValue[] { new KeyValue("SERIAL_NUMBER", serialNumber), new KeyValue("BOM_ALTERNATIVE", "0"), new KeyValue("BOM_TYPE", "1") };
            string[] bomDataResultKeys = new string[] { "MACHINE_GROUP_NUMBER", "PART_NUMBER", "PART_DESC", "SETUP_FLAG", "QUANTITY", "COMP_NAME", "PROCESS_LAYER", "PRODUCT_FLAG" };
            string[] bomDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetBomData (Serial Number:" + serialNumber + ")");
            int error = imsapi.mdataGetBomData(sessionContext, stationNumber, bomDataFilter, bomDataResultKeys, out bomDataResultValues);
            LogHelper.Info("end api mdataGetBomData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
                return null;
            }
            else
            {
                string machineGroupTemp = GetMachineGroup(stationNumber);
                int loop = bomDataResultKeys.Length;
                int count = bomDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (bomDataResultValues[i + 3] == "1" || bomDataResultValues[i + 7] == "1")
                    {
                        string strMachineGroup = bomDataResultValues[i].ToString();
                        if (machineGroupTemp == strMachineGroup)
                        {
                            DataRow row = dt.NewRow();
                            if (machineGroupTemp == strMachineGroup && bomDataResultValues[i + 3] == "1")
                            {
                                row["SetupFlag"] = "Y";
                            }
                            else
                            {
                                row["SetupFlag"] = "";
                            }
                            row["ErpGroup"] = bomDataResultValues[i].ToString();
                            row["PartNumber"] = bomDataResultValues[i + 1].ToString();
                            row["PartDesc"] = bomDataResultValues[i + 2].ToString();
                            row["Quantity"] = bomDataResultValues[i + 4].ToString();
                            row["CompName"] = bomDataResultValues[i + 5].ToString();
                            row["ProcessLayer"] = bomDataResultValues[i + 6].ToString();
                            row["ProductFlag"] = bomDataResultValues[i + 7].ToString();
                            dt.Rows.Add(row);
                        }
                    }
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
            }
            return dt;
        }

        public Dictionary<string, string> GetPPNAndCompNameFromBOM(string serialNumber, string stationNumber)
        {
            Dictionary<string, string> dicValues = new Dictionary<string, string>();
            KeyValue[] bomDataFilter = new KeyValue[] { new KeyValue("SERIAL_NUMBER", serialNumber), new KeyValue("BOM_ALTERNATIVE", "0"), new KeyValue("BOM_TYPE", "1") };
            string[] bomDataResultKeys = new string[] { "MACHINE_GROUP_NUMBER", "PART_NUMBER", "COMP_NAME" };
            string[] bomDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetBomData (Serial Number:" + serialNumber + ")");
            int error = imsapi.mdataGetBomData(sessionContext, stationNumber, bomDataFilter, bomDataResultKeys, out bomDataResultValues);
            LogHelper.Info("end api mdataGetBomData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
                return null;
            }
            else
            {
                string machineGroupTemp = GetMachineGroup(stationNumber);
                int loop = bomDataResultKeys.Length;
                int count = bomDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    string strMachineGroup = bomDataResultValues[i].ToString();
                    if (machineGroupTemp == strMachineGroup)
                    {
                        dicValues[bomDataResultValues[i + 1].ToString()] = bomDataResultValues[i + 2].ToString();
                    }
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
            }
            return dicValues;
        }

        public DataTable GetBomMaterialDataBySNExt(string serialNumber, string stationNumber)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ErpGroup", typeof(string)));
            dt.Columns.Add(new DataColumn("PartNumber", typeof(string)));
            dt.Columns.Add(new DataColumn("PartDesc", typeof(string)));
            dt.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dt.Columns.Add(new DataColumn("CompName", typeof(string)));
            dt.Columns.Add(new DataColumn("ProcessLayer", typeof(string)));
            dt.Columns.Add(new DataColumn("SetupFlag", typeof(string)));
            dt.Columns.Add(new DataColumn("ProductFlag", typeof(string)));

            KeyValue[] bomDataFilter = new KeyValue[] { new KeyValue("SERIAL_NUMBER", serialNumber), new KeyValue("BOM_ALTERNATIVE", "0"), new KeyValue("BOM_TYPE", "1") };
            string[] bomDataResultKeys = new string[] { "MACHINE_GROUP_NUMBER", "PART_NUMBER", "PART_DESC", "SETUP_FLAG", "QUANTITY", "COMP_NAME", "PROCESS_LAYER", "PRODUCT_FLAG" };
            string[] bomDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetBomData (Serial Number:" + serialNumber + ")");
            int error = imsapi.mdataGetBomData(sessionContext, stationNumber, bomDataFilter, bomDataResultKeys, out bomDataResultValues);
            LogHelper.Info("end api mdataGetBomData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
                return dt;
            }
            else
            {
                string machineGroupTemp = GetMachineGroup(stationNumber);
                int loop = bomDataResultKeys.Length;
                int count = bomDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (bomDataResultValues[i + 3] == "1" || bomDataResultValues[i + 7] == "1")
                    {
                        string strMachineGroup = bomDataResultValues[i].ToString();
                        DataRow row = dt.NewRow();
                        row["ErpGroup"] = bomDataResultValues[i].ToString();
                        row["PartNumber"] = bomDataResultValues[i + 1].ToString();
                        row["PartDesc"] = bomDataResultValues[i + 2].ToString();
                        row["Quantity"] = bomDataResultValues[i + 4].ToString();
                        row["CompName"] = bomDataResultValues[i + 5].ToString();
                        row["ProcessLayer"] = bomDataResultValues[i + 6].ToString();
                        row["ProductFlag"] = bomDataResultValues[i + 7].ToString();
                        dt.Rows.Add(row);
                    }
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
            }
            return dt;
        }

        private string GetMachineGroup(string stationNumber)
        {
            string machineGroup = "";
            KeyValue[] machineAssetStructureFilter = new KeyValue[] { new KeyValue("DISSOLVING_LEVEL", "1"), new KeyValue("FUNC_MODE", "0"), new KeyValue("STATION_NUMBER", stationNumber) };
            string[] machineAssetStructureResultKeys = new string[] { "MACHINE_GROUP_NUMBER" };
            string[] machineAssetStructureValues = new string[] { };
            int error = imsapi.mdataGetMachineAssetStructure(sessionContext, stationNumber, machineAssetStructureFilter, machineAssetStructureResultKeys, out machineAssetStructureValues);
            LogHelper.Info("api mdataGetMachineAssetStructure (station number = " + stationNumber + "), error code =" + error);
            if (error == 0)
                machineGroup = machineAssetStructureValues[0];
            LogHelper.Info("Machine Group Number = " + machineGroup);
            return machineGroup;
        }

        public WorkplanEntity GetWorkStepDataByStation(string workorder, string stationNumber)
        {
            WorkplanEntity entity = null;
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("WORKSTEP_FLAG", "1") };
            string[] workplanDataResultKeys = new string[] { "PROCESS_LAYER", "WORKSTEP_NUMBER", "WORKPLAN_ID" };
            string[] workplanDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetWorkplanData (Work Order:" + workorder + ")");
            int error = imsapi.mdataGetWorkplanData(sessionContext, stationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("end api mdataGetWorkplanData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetWorkplanData " + error, "");
                return null;
            }
            if (error == 0)
            {
                entity = new WorkplanEntity();
                entity.PROCESS_LAYER = workplanDataResultValues[0];
                entity.WORKSTEP_NUMBER = workplanDataResultValues[1];
                //entity.WORKPLAN_ID = workplanDataResultValues[2];
            }
            return entity;
        }

        public bool VerifySNHasUpload(string serialNumber, string stationNumber, int processLayer)
        {
            string[] uploadInfoResultKeys = new string[] { "SERIAL_NUMBER_STATE" };
            string[] uploadInfoResultValues = new string[] { };
            int error = imsapi.trGetSerialNumberUploadInfo(sessionContext, stationNumber, processLayer, serialNumber, "-1", 1, uploadInfoResultKeys, out uploadInfoResultValues);
            LogHelper.Info("Api trGetSerialNumberUploadInfo station number =" + stationNumber + ", serial number =" + serialNumber + ", process Layer =" + processLayer + ", error code =" + error);
            if (error == 0)
            {
                if (uploadInfoResultValues[0] == "-1")
                    return false;
                else
                    return true;
            }

            return false;
        }

        public Dictionary<string, string> MdataGetFailureDataforStation(string stationNumber)
        {
            var failuredataResultKeys = new string[] { "FAILURE_TYPE_CODE", "FAILURE_TYPE_DESC" };
            var failureDataResultValues = new string[] { };
            Dictionary<string, string> dicFailureMap = new Dictionary<string, string>();
            LogHelper.Info("begin api mdataGetFailureDataForStation (Station:" + stationNumber);
            int error = imsapi.mdataGetFailureDataForStation(sessionContext, stationNumber, 0, failuredataResultKeys, out failureDataResultValues);
            LogHelper.Info("end api mdataGetFailureDataForStation (ResultCode = " + error + ")");
            if (error == 0)
            {
                for (int i = 0; i < failureDataResultValues.Length / 2; i++)
                {
                    if (!dicFailureMap.ContainsKey(failureDataResultValues[2 * i + 1]))
                    {
                        dicFailureMap[failureDataResultValues[2 * i + 1].ToUpper()] = failureDataResultValues[2 * i + 0];
                    }
                }
                // view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetFailureDataForStation " + error, "");
            }
            else
            {
                // view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetFailureDataForStation " + error, "");
            }
            return dicFailureMap;
        }

        public string[] GetEquipmentDetailData(string equipmentNo)
        {
            int errorCode = 0;
            string errorMsg = "";
            KeyValue[] equipmentGetFilters = new KeyValue[] { new KeyValue("EQUIPMENT_NUMBER", equipmentNo) };
            string[] equipmentGetResultKeys = new string[] { "EQUIPMENT_STATE", "ERROR_CODE", "PART_NUMBER", "EQUIPMENT_INDEX" };
            string[] equipmentGetResultValues = new string[] { };
            errorCode = imsapi.equGetEquipment(sessionContext, init.configHandler.StationNumber, equipmentGetFilters, equipmentGetResultKeys, out equipmentGetResultValues);
            LogHelper.Info("Api equGetEquipment: station no =" + init.configHandler.StationNumber + "equipment number =" + equipmentNo + ",result code =" + errorCode);
            if (errorCode == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equGetEquipment " + errorCode, "");
            }
            else
            {
                //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " equGetEquipment " + errorCode + "," + errorMsg, "");
            }
            return equipmentGetResultValues;
        }

        public string GetCalendarData()
        {
            string[] calendarDataResultKeys = new string[] { "DISPLAY_DATE" };
            string[] calendarDataResultValues = new string[] { };
            string value = "";
            int resultCode = imsapi.mdataGetCalendarData(sessionContext, init.configHandler.StationNumber, calendarDataResultKeys, out calendarDataResultValues);
            if (resultCode == 0)
            {
                value = calendarDataResultValues[0];
            }
            LogHelper.Info("Api mdataGetCalendarData: result code =" + resultCode);
            return value;
        }
    }
}
