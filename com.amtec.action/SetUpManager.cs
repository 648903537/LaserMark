using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class SetUpManager
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public SetUpManager(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int UpdateMaterialSetUpByBin(int processLayer, string workorderNumber, string materialBinNumber, string materialQty, string partNumber, string setupName, string setupPos)
        {
            int error = 0;
            string[] materialSetupUploadKeys = new string[] { "ERROR_CODE", "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_QTY_TOTAL", "PART_NUMBER", "SETUP_POSITION", "SETUP_STATE" };
            string[] materialSetupUploadValues = new string[] { "0", materialBinNumber, materialQty, partNumber, setupPos, "0" };
            string[] compPositionsUploadKeys = new string[] { "COMP_REFERENCE" };
            string[] compPositionsUploadValues = new string[] { };
            string[] materialSetupResultValues = new string[] { };
            string[] compPositionsResultValues = new string[] { };
            LogHelper.Info("begin api setupUpdateMaterialSetup (material bin number:" + materialBinNumber + ")");
            error = imsapi.setupUpdateMaterialSetup(sessionContext, init.configHandler.StationNumber, processLayer, workorderNumber, "-1", setupName, materialSetupUploadKeys
                , materialSetupUploadValues, compPositionsUploadKeys, compPositionsUploadValues, out materialSetupResultValues, out compPositionsResultValues);
            LogHelper.Info("end api setupUpdateMaterialSetup (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " setupUpdateMaterialSetup " + error, "");
            }
            else
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " setupUpdateMaterialSetup " + error, "");
            }
            return error;
        }

        public int SetupStateChange(string workorder, int activateFlag, int processLayer)
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

        public int SetupCheck(string workorder)
        {
            int error = 0;
            string workorderNumber = "";
            string productNumber = "";
            string placementName = "";
            int setupActive = 0;
            CheckSetupData[] checkSetupDataArray = new CheckSetupData[] { };
            error = imsapi.setupCheck(sessionContext, init.configHandler.StationNumber, "-1", "-1", workorder, init.currentSettings.processLayer, 0, 0
                , out workorderNumber, out productNumber, out placementName, out setupActive, out checkSetupDataArray);
            LogHelper.Info("Api setupCheck station number:" + init.configHandler.StationNumber + ",work order:" + workorder + ",process layer:" + init.currentSettings.processLayer + ", result code=" + error);
            if (error == 604)
                error = 0;
            return error;
        }

        public string[] GetSetupMaterialByStation(string stationNumber, int processLayer)
        {
            string[] setupResultKeys = new string[] { "MATERIAL_BIN_NUMBER" };
            string[] setupResultValues = new string[] { };
            string[] componentResultKeys = new string[] { };
            string[] componentResultValues = new string[] { };
            int error = imsapi.setupGetMaterialSetup(sessionContext, stationNumber, processLayer, 0, -1, -1, setupResultKeys, out setupResultValues, componentResultKeys, out componentResultValues);
            LogHelper.Info("Api setupGetMaterialSetup station number =" + stationNumber + ", process layer =" + processLayer + ", result code =" + error);
            return setupResultValues;
        }

        public int UpdateMaterialBinBooking(string stationNumber, string materialBin, double strQty)
        {
            string[] materialBinBookingUploadKeys = new string[] { "ERROR_CODE", "MATERIAL_BIN_NUMBER", "QUANTITY", "TRANSACTION_CODE" };
            string[] materialBinBookingUploadValues = new string[] { "0", materialBin, strQty.ToString(), "0" };
            string[] materialBinBookingResultValues = new string[] { };
            LogHelper.Info("begin api mlUploadMaterialBinBooking (material bin number =" + materialBin + ",quantity=" + strQty + ")");
            int error = imsapi.mlUploadMaterialBinBooking(sessionContext, stationNumber, materialBinBookingUploadKeys, materialBinBookingUploadValues, out materialBinBookingResultValues);
            LogHelper.Info("end api mlUploadMaterialBinBooking (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mlUploadMaterialBinBooking " + error, "");
                return error;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mlUploadMaterialBinBooking " + error, "");
            return error;
        }
    }
}
