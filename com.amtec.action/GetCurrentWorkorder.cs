using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;


namespace com.amtec.action
{
    public class GetCurrentWorkorder
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;
        private int error;

        public GetCurrentWorkorder(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public GetStationSettingModel GetCurrentWorkorderResultCall()
        {

            GetStationSettingModel stationSetting = new GetStationSettingModel();
            String[] stationSettingResultKey = new String[] { "BOM_VERSION", "WORKORDER_NUMBER", "PART_NUMBER", "WORKORDER_STATE", "PROCESS_VERSION", "PROCESS_LAYER", "ATTRIBUTE_1", "QUANTITY" };
            String[] stationSettingResultValues;
            LogHelper.Info("begin api trGetStationSetting (Station number:" + init.configHandler.StationNumber + ")");
            error = imsapi.trGetStationSetting(sessionContext, init.configHandler.StationNumber, stationSettingResultKey, out stationSettingResultValues);
            LogHelper.Info("end api trGetStationSetting (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trGetStationSetting " + error, "");
                return null;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trGetStationSetting " + error, "");
            stationSetting.bomVersion = stationSettingResultValues[0];
            stationSetting.workorderNumber = stationSettingResultValues[1];
            stationSetting.partNumber = stationSettingResultValues[2];
            stationSetting.workorderState = stationSettingResultValues[3];
            stationSetting.processVersion = int.Parse(stationSettingResultValues[4]);
            stationSetting.processLayer = int.Parse(stationSettingResultValues[5]);
            stationSetting.attribute1 = stationSettingResultValues[6];
            stationSetting.QuantityMO = int.Parse(stationSettingResultValues[7]);
            return stationSetting;
        }

        public string[] GetSerialnumberForWorkOrder(string workorder,string maxrow)
        {
            string[] serialNumberResultKeys = new string[] { "SERIAL_NUMBER", "SERIAL_NUMBER_STATE" };
            string[] serialNumberResultValues = new string[] { };
            int error = imsapi.trGetSerialNumberForWorkOrderAndWorkstep(sessionContext, init.configHandler.StationNumber, init.currentSettings.processLayer, workorder, 0, 1, 1, 0, int.Parse(maxrow), 0, 0, serialNumberResultKeys, out serialNumberResultValues);
            LogHelper.Info("end api trGetSerialNumberForWorkOrderAndWorkstep (workorder=" + workorder + ",result code = " + error + ")");
            return serialNumberResultValues;
        }
    }
}
