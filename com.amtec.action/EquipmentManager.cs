using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;

namespace com.amtec.action
{
    public class EquipmentManager
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public EquipmentManager(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int CheckEquipmentData(string workOder)
        {
            int errorCode = 0;
            string errorMsg = "";
            string[] equipmentResultKeys = new string[] { "EQUIPMENT_CHECKSTATE", "EQUIPMENT_NUMBER", "PART_NUMBER" };
            string[] equipmentResultValues = new string[] { };
            errorCode = imsapi.equCheckEquipmentData(sessionContext, init.configHandler.StationNumber, workOder, "-1", "-1", init.currentSettings.processLayer, 0, equipmentResultKeys, out equipmentResultValues);
            LogHelper.Info("Api equCheckEquipmentData: work order  =" + workOder + ",error code =" + errorCode);
            if (equipmentResultValues.Length > 0)
            {
                foreach (var item in equipmentResultValues)
                {
                    LogHelper.Info(item);
                }
            }
            if (errorCode == 0)
            {
                List<EquipmentEntity> entityList = new List<EquipmentEntity>();
                int loop = equipmentResultKeys.Length;
                int count = equipmentResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    EquipmentEntity entity = new EquipmentEntity();
                    entity.EQUIPMENT_CHECKSTATE = equipmentResultValues[i + 0];
                    entity.EQUIPMENT_NUMBER = equipmentResultValues[i + 1];
                    entity.PART_NUMBER = equipmentResultValues[i + 2];
                    entityList.Add(entity);
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equCheckEquipmentData " + errorCode, "");
            }
            else
            {
                //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " equCheckEquipmentData " + errorCode + "," + errorMsg, "");
            }
            return errorCode;
        }

        public List<EquipmentEntity> GetRequiredEquipmentData(string workorder)
        {
            int errorCode = 0;
            string errorMsg = "";
            List<string> equipList = new List<string>();
            List<EquipmentEntity> entityList = new List<EquipmentEntity>();
            string[] equipmentResultKeys = new string[] { "EQUIPMENT_NUMBER", "PART_NUMBER", "EQUIPMENT_DESCRIPTION" };
            string[] equipmentResultValues = new string[] { };
            errorCode = imsapi.equGetRequiredEquipmentData(sessionContext, init.configHandler.StationNumber, workorder, "-1", "-1", init.currentSettings.processLayer, "-1",
                equipmentResultKeys, out equipmentResultValues);
            LogHelper.Info("Api equGetRequiredEquipmentData: workorder number =" + workorder + ",error code =" + errorCode);
            if (errorCode == 0)
            {
                int loop = equipmentResultKeys.Length;
                int count = equipmentResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (equipList.Contains(equipmentResultValues[i + 1]))
                        continue;
                    EquipmentEntity entity = new EquipmentEntity();
                    entity.EQUIPMENT_NUMBER = equipmentResultValues[i];
                    entity.EQUIPMENT_DESCRIPTION = equipmentResultValues[i + 2];
                    entity.PART_NUMBER = equipmentResultValues[i + 1];
                    entityList.Add(entity);
                    equipList.Add(equipmentResultValues[i + 1]);
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equGetRequiredEquipmentData " + errorCode, "");
            }
            else
            {
                //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " equGetRequiredEquipmentData " + errorCode + "," + errorMsg, "");
            }
            return entityList;
        }

        public int UpdateEquipmentData(string equipmentIndex, string equipmentNo, int setupFlag)
        {
            int errorCode = 0;
            string errorMsg = "";
            string[] equipmentUploadKeys = new string[] { "EQUIPMENT_INDEX", "EQUIPMENT_NUMBER", "ERROR_CODE" };
            string[] equipmentUploadValues = new string[] { equipmentIndex, equipmentNo, "0" };
            string[] equipmentResultValues = new string[] { };
            errorCode = imsapi.equUpdateEquipmentData(sessionContext, init.configHandler.StationNumber, setupFlag, "-1", init.currentSettings.workorderNumber, init.currentSettings.processLayer,
                equipmentUploadKeys, equipmentUploadValues, out equipmentResultValues);
            LogHelper.Info("Api equUpdateEquipmentData: equipment no =" + equipmentNo + ",setup flag = " + setupFlag + ", error code =" + errorCode);
            if (errorCode == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equUpdateEquipmentData " + errorCode, "");
            }
            else
            {
                if (errorCode == 5)
                {
                    if (equipmentResultValues[1] == "1301")
                    {
                        errorCode = 0;
                    }
                    else if (equipmentResultValues[1] == "1300")
                    {
                        //remove
                        errorCode = Convert.ToInt32(equipmentResultValues[2]);
                        UpdateEquipmentData(equipmentIndex, equipmentNo, 1);
                    }
                    else
                    {
                        errorCode = Convert.ToInt32(equipmentResultValues[2]);
                    }

                }
                if (errorCode != 0)
                {
                    //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                    errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                    view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " equUpdateEquipmentData " + errorCode + "," + errorMsg, "");
                }
            }
            return errorCode;
        }

        public EquipmentEntityExt GetSetupEquipmentData(string equipmentNo)
        {
            int errorCode = 0;
            string errorMsg = "";
            EquipmentEntityExt entity = null;
            string[] equipmentSetupResultValues = new string[] { };
            string[] equipmentSetupResultKeys = new string[] { "EQUIPMENT_NUMBER", "EQUIPMENT_STATE", "SECONDS_BEFORE_EXPIRATION", "USAGES_BEFORE_EXPIRATION" };
            errorCode = imsapi.equGetSetupEquipmentData(sessionContext, init.configHandler.StationNumber, new KeyValue[] { }, equipmentSetupResultKeys, out equipmentSetupResultValues);
            LogHelper.Info("Api equGetSetupEquipmentData: equipment no =" + equipmentNo + ",error code =" + errorCode);
            if (errorCode == 0)
            {
                int loop = equipmentSetupResultKeys.Length;
                int count = equipmentSetupResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    if (equipmentSetupResultValues[i] == equipmentNo)
                    {
                        entity = new EquipmentEntityExt();
                        entity.EQUIPMENT_NUMBER = equipmentSetupResultValues[i];
                        entity.EQUIPMENT_STATE = equipmentSetupResultValues[i + 1];
                        entity.SECONDS_BEFORE_EXPIRATION = equipmentSetupResultValues[i + 2];
                        entity.USAGES_BEFORE_EXPIRATION = equipmentSetupResultValues[i + 3];
                    }
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equGetSetupEquipmentData " + errorCode, "");
            }
            else
            {
                //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " equGetSetupEquipmentData " + errorCode + "," + errorMsg, "");
            }
            return entity;
        }

        public List<EquipmentEntityExt> GetSetupEquipmentDataByStation(string stationNumber)
        {
            int errorCode = 0;
            string errorMsg = "";
            List<EquipmentEntityExt> entityList = new List<EquipmentEntityExt>();
            EquipmentEntityExt entity = null;
            string[] equipmentSetupResultValues = new string[] { };
            string[] equipmentSetupResultKeys = new string[] { "EQUIPMENT_NUMBER", "EQUIPMENT_STATE", "SECONDS_BEFORE_EXPIRATION", "USAGES_BEFORE_EXPIRATION", "EQUIPMENT_INDEX" };
            errorCode = imsapi.equGetSetupEquipmentData(sessionContext, stationNumber, new KeyValue[] { }, equipmentSetupResultKeys, out equipmentSetupResultValues);
            LogHelper.Info("Api equGetSetupEquipmentData: station number =" + stationNumber + ",error code =" + errorCode);
            if (errorCode == 0)
            {
                int loop = equipmentSetupResultKeys.Length;
                int count = equipmentSetupResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    entity = new EquipmentEntityExt();
                    entity.EQUIPMENT_NUMBER = equipmentSetupResultValues[i];
                    entity.EQUIPMENT_STATE = equipmentSetupResultValues[i + 1];
                    entity.SECONDS_BEFORE_EXPIRATION = equipmentSetupResultValues[i + 2];
                    entity.USAGES_BEFORE_EXPIRATION = equipmentSetupResultValues[i + 3];
                    entity.EQUIPMENT_INDEX = equipmentSetupResultValues[i + 4];
                    entityList.Add(entity);
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equGetSetupEquipmentData " + errorCode, "");
            }
            else
            {
                //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " equGetSetupEquipmentData " + errorCode + "," + errorMsg, "");
            }
            return entityList;
        }

        public void RemoveEquipment()
        {
            int errorCode = 0;
            string errorMsg = "";
            errorCode = imsapi.equRemoveEquipment(sessionContext, init.configHandler.StationNumber, init.currentSettings.workorderNumber, "-1", init.currentSettings.processLayer);
            LogHelper.Info("Api equRemoveEquipment: station no =" + init.configHandler.StationNumber + ",error code =" + errorCode);
            if (errorCode == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " equRemoveEquipment " + errorCode, "");
            }
            else
            {
                //imsapi.imsapiGetErrorText(sessionContext, errorCode, out errorMsg);
                errorMsg = UtilityFunction.GetZHSErrorString(errorCode, init, sessionContext);
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " equRemoveEquipment " + errorCode + "," + errorMsg, "");
            }
        }

        public string[] GetEquipmentDetailData(string equipmentNo)
        {
            int errorCode = 0;
            string errorMsg = "";
            KeyValue[] equipmentGetFilters = new KeyValue[] { new KeyValue("EQUIPMENT_NUMBER", equipmentNo) };
            string[] equipmentGetResultKeys = new string[] { "EQUIPMENT_STATE", "ERROR_CODE", "PART_NUMBER", "EQUIPMENT_INDEX" };
            string[] equipmentGetResultValues = new string[] { };
            errorCode = imsapi.equGetEquipment(sessionContext, init.configHandler.StationNumber, equipmentGetFilters, equipmentGetResultKeys, out equipmentGetResultValues);
            LogHelper.Info("Api equGetEquipment: station no =" + init.configHandler.StationNumber + "equipment number =" + equipmentNo + ",error code =" + errorCode);
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
    }
}
