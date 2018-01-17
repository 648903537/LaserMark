using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System.Data;

namespace com.amtec.action
{
    public class GetMaterialBinData
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public GetMaterialBinData(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public string[] GetMaterialBinDataDetails(string materialBinNo)
        {
            KeyValue[] materialBinFilters = new KeyValue[] { new KeyValue("MATERIAL_BIN_NUMBER", materialBinNo) };
            AttributeInfo[] attributes = new AttributeInfo[] { };
            string[] materialBinResultKeys = new string[] { "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_PART_NUMBER", "MATERIAL_BIN_QTY_ACTUAL", "MATERIAL_BIN_QTY_TOTAL", "PART_DESC", "SUPPLIER_CHARGE_NUMBER", "EXPIRATION_DATE", "LOCK_STATE" };
            string[] materialBinResultValues = new string[] { };
            LogHelper.Info("begin api mlGetMaterialBinData (Material bin number:" + materialBinNo + ")");
            int error = imsapi.mlGetMaterialBinData(sessionContext, init.configHandler.StationNumber, materialBinFilters, attributes, materialBinResultKeys, out materialBinResultValues);
            LogHelper.Info("end api mlGetMaterialBinData (result code = " + error + ")");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mlGetMaterialBinData " + error + ", material bin number = " + materialBinNo, "");
            }
            else
            {
                string errorMsg = "";
                //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mlGetMaterialBinData " + error + ",error message =" + errorMsg, "");
            }
            return materialBinResultValues;
        }

        public string GetNextMaterialBinNumber(string partNumber)
        {
            string materialBinNo = "";
            LogHelper.Info("begin api mlGetNextMaterialBinNumber (part number:" + partNumber + ")");
            int error = imsapi.mlGetNextMaterialBinNumber(sessionContext, init.configHandler.StationNumber, partNumber, out materialBinNo);
            LogHelper.Info("end api mlGetNextMaterialBinNumber (result code = " + error + ")");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mlGetNextMaterialBinNumber " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mlGetNextMaterialBinNumber " + error, "");
            }
            return materialBinNo;
        }

        public int CreateMaterialBinNumber(string materialBinNo, string partNumber)
        {
            int error = 0;
            string[] materialBinUploadKeys = new string[] { "ERROR_CODE", "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_PART_NUMBER", "MATERIAL_BIN_QTY_ACTUAL" };
            string[] materialBinUploadValues = new string[] { "0", materialBinNo, partNumber, "0" };
            string[] materialBinResultValues = new string[] { };
            LogHelper.Info("begin api mlCreateNewMaterialBin (part number:" + partNumber + ",material bin number:" + materialBinNo + ")");
            error = imsapi.mlCreateNewMaterialBin(sessionContext, init.configHandler.StationNumber, materialBinUploadKeys, materialBinUploadValues, out materialBinResultValues);
            LogHelper.Info("end api mlCreateNewMaterialBin (result code = " + error + ")");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mlCreateNewMaterialBin " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mlCreateNewMaterialBin " + error, "");
            }
            return error;
        }

        public DataTable GetBomMaterialData(string workorder)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ErpGroup", typeof(string)));
            dt.Columns.Add(new DataColumn("PartNumber", typeof(string)));
            dt.Columns.Add(new DataColumn("PartDesc", typeof(string)));
            dt.Columns.Add(new DataColumn("Quantity", typeof(string)));
            dt.Columns.Add(new DataColumn("CompName", typeof(string)));
            dt.Columns.Add(new DataColumn("ProcessLayer", typeof(string)));

            KeyValue[] bomDataFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("BOM_ALTERNATIVE", "0") };
            string[] bomDataResultKeys = new string[] { "MACHINE_GROUP_NUMBER", "PART_NUMBER", "PART_DESC", "SETUP_FLAG", "QUANTITY", "COMP_NAME", "PROCESS_LAYER" };
            string[] bomDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetBomData (Work Order:" + workorder + ")");
            int error = imsapi.mdataGetBomData(sessionContext, init.configHandler.StationNumber, bomDataFilter, bomDataResultKeys, out bomDataResultValues);

            LogHelper.Info("end api mdataGetBomData (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetBomData " + error, "");
                return null;
            }
            else
            {
                string machineGroupTemp = GetMachineGroup(init.configHandler.StationNumber);
                int loop = bomDataResultKeys.Length;
                int count = bomDataResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (bomDataResultValues[i + 3] == "1")
                    {
                        string strMachineGroup = bomDataResultValues[i].ToString();
                        int iProcessLayer = int.Parse(bomDataResultValues[i + 6].ToString());
                        if (strMachineGroup == machineGroupTemp && init.currentSettings.processLayer == iProcessLayer)
                        {
                            DataRow row = dt.NewRow();
                            row["ErpGroup"] = bomDataResultValues[i].ToString();
                            row["PartNumber"] = bomDataResultValues[i + 1].ToString();
                            row["PartDesc"] = bomDataResultValues[i + 2].ToString();
                            row["Quantity"] = bomDataResultValues[i + 4].ToString();
                            row["CompName"] = bomDataResultValues[i + 5].ToString();
                            row["ProcessLayer"] = bomDataResultValues[i + 6].ToString();
                            dt.Rows.Add(row);
                        }
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
    }
}
