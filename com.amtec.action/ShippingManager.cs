using com.amtec.action;
using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;

namespace ShippingClient.com.amtec.action
{
    public class ShippingManager
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public ShippingManager(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int ActivateShippingLotAtStation(string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipActivateShippingLotAtKap (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ")");
            error = imsapi.shipActivateShippingLotAtKap(sessionContext, init.configHandler.StationNumber, lotNumber);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            LogHelper.Info("end api shipActivateShippingLotAtKap (result code = " + error + ")");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipActivateShippingLotAtKap " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipActivateShippingLotAtKap " + error + "," + errorMsg, "");
            }
            return error;
        }

        public int DeactivateShippingLotAtStation(string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipDeactivateShippingLotAtKap (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ")");
            error = imsapi.shipDeactivateShippingLotAtKap(sessionContext, init.configHandler.StationNumber, lotNumber);
            LogHelper.Info("end api shipDeactivateShippingLotAtKap (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipDeactivateShippingLotAtKap " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipDeactivateShippingLotAtKap " + error + "," + errorMsg, "");
            }
            return error;
        }

        public int AddSNToShippingLot(string lotNumber, string serialNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipAddSerialNumberToShippingLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ",serial number =" + serialNumber + ")");
            error = imsapi.shipAddSerialNumberToShippingLot(sessionContext, init.configHandler.StationNumber, lotNumber, serialNumber, -1, -1);
            LogHelper.Info("end api shipAddSerialNumberToShippingLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0 || error == 405)//405 'Last serial number'
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipAddSerialNumberToShippingLot " + error, "");
            }
            else//-437 'Package quantity already reached'
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipAddSerialNumberToShippingLot " + error + "," + errorMsg, "");
            }
            return error;
        }


        public int CheckSNAddToShippingLot(string lotNumber, string serialNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipCheckSerialNumberAddToShippingLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ",serial number =" + serialNumber + ")");
            error = imsapi.shipCheckSerialNumberAddToShippingLot(sessionContext, init.configHandler.StationNumber, lotNumber, serialNumber, "-1");
            LogHelper.Info("end api shipCheckSerialNumberAddToShippingLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipCheckSerialNumberAddToShippingLot " + error, "");
            }
            else//=-424 'Serial number is already assigned to the lot';=-437 'Package quantity already reached'//=-425 'Part no. is different from the serial numbers already included'
            {
                view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " shipCheckSerialNumberAddToShippingLot " + error + "," + errorMsg, "");
            }
            return error;
        }

        public int CheckSNFromShippingLot(string lotNumber, string serialNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipCheckSerialNumberFromShippingLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ",serial number =" + serialNumber + ")");
            error = imsapi.shipCheckSerialNumberFromShippingLot(sessionContext, init.configHandler.StationNumber, lotNumber, serialNumber, "-1");
            LogHelper.Info("end api shipCheckSerialNumberFromShippingLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipCheckSerialNumberFromShippingLot " + error, "");
            }
            else//-427 'Serial number was not assigned to a lot'
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipCheckSerialNumberFromShippingLot " + error + "," + errorMsg, "");
            }
            return error;
        }

        public int CompleteShippingLot(string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipCompleteLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ")");
            error = imsapi.shipCompleteLot(sessionContext, init.configHandler.StationNumber, lotNumber, 1, -1);
            LogHelper.Info("end api shipCompleteLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipCompleteLot " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipCompleteLot " + error + "," + errorMsg, "");
            }
            return error;
        }

        public ShippingLotEntity GetLotFromSN(string serialNumber)
        {
            int error = 0;
            string errorMsg = "";
            ShippingLotEntity entity = new ShippingLotEntity();
            string[] lotResultKeys = new string[] { "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_QTY_ACTUAL", "MATERIAL_BIN_QTY_TOTAL", "MATERIAL_BIN_STATE", "PART_NUMBER","PART_DESC"
            ,"QUANTITY_UNIT","SHIPPING_LOT_NUMBER","SHIPPING_LOT_NUMBER2","SHIPPING_LOT_PART_NUMBER","SHIPPING_LOT_SNO_PART_DESCRIPTION","SHIPPING_LOT_SNO_PART_NUMBER"
            ,"SHIPPING_LOT_SNO_QTY_ACTUAL","SHIPPING_LOT_SNO_QTY_TOTAL"};
            string[] lotResultValues = new string[] { };
            LogHelper.Info("begin api shipGetLotFromSerialNumber (Station number:" + init.configHandler.StationNumber + ",serial number =" + serialNumber + ")");
            error = imsapi.shipGetLotFromSerialNumber(sessionContext, init.configHandler.StationNumber, serialNumber, "-1", lotResultKeys, out lotResultValues);
            LogHelper.Info("end api shipGetLotFromSerialNumber (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                int loop = lotResultKeys.Length;
                int count = lotResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    entity.MATERIAL_BIN_NUMBER = lotResultValues[i + 0];
                    entity.MATERIAL_BIN_QTY_ACTUAL = lotResultValues[i + 1];
                    entity.MATERIAL_BIN_QTY_TOTAL = lotResultValues[i + 2];
                    entity.MATERIAL_BIN_STATE = lotResultValues[i + 3];
                    entity.PART_NUMBER = lotResultValues[i + 4];
                    entity.PART_DESC = lotResultValues[i + 5];
                    entity.QUANTITY_UNIT = lotResultValues[i + 6];
                    entity.SHIPPING_LOT_NUMBER = lotResultValues[i + 7];
                    entity.SHIPPING_LOT_NUMBER2 = lotResultValues[i + 8];
                    entity.SHIPPING_LOT_PART_NUMBER = lotResultValues[i + 9];
                    entity.SHIPPING_LOT_SNO_PART_DESCRIPTION = lotResultValues[i + 10];
                    entity.SHIPPING_LOT_SNO_PART_NUMBER = lotResultValues[i + 11];
                    entity.SHIPPING_LOT_SNO_QTY_ACTUAL = lotResultValues[i + 12];
                    entity.SHIPPING_LOT_SNO_QTY_TOTAL = lotResultValues[i + 13];

                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipGetLotFromSerialNumber " + error, "");
            }
            else//-418 'Serial number was not found in the lot'
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipGetLotFromSerialNumber " + error + "," + errorMsg, "");
            }
            return entity;
        }

        public List<SNFromShippingLotEntity> GetSNDataFromShippingLot(string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            List<SNFromShippingLotEntity> listEntity = new List<SNFromShippingLotEntity>();
            string[] serialNumberResultKeys = new string[] { "PART_DESC", "PART_NUMBER", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SHIPPING_DATE"
                , "SHIPPING_STATION_DESC", "SHIPPING_STATION_NUMBER","WORKORDER_NUMBER" };
            string[] serialNumberResultValues = new string[] { };
            LogHelper.Info("begin api shipGetSerialNumberDataForShippingLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ")");
            error = imsapi.shipGetSerialNumberDataForShippingLot(sessionContext, init.configHandler.StationNumber, lotNumber, serialNumberResultKeys, out serialNumberResultValues);
            LogHelper.Info("end api shipGetSerialNumberDataForShippingLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                int loop = serialNumberResultKeys.Length;
                int count = serialNumberResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    SNFromShippingLotEntity entity = new SNFromShippingLotEntity();
                    entity.PART_DESC = serialNumberResultValues[i + 0];
                    entity.PART_NUMBER = serialNumberResultValues[i + 1];
                    entity.SERIAL_NUMBER = serialNumberResultValues[i + 2];
                    entity.SERIAL_NUMBER_POS = serialNumberResultValues[i + 3];
                    entity.SHIPPING_DATE = ConvertToDateTime(serialNumberResultValues[i + 4]);
                    entity.SHIPPING_STATION_DESC = serialNumberResultValues[i + 5];
                    entity.SHIPPING_STATION_NUMBER = serialNumberResultValues[i + 6];
                    entity.WORKORDER_NUMBER = serialNumberResultValues[i + 7];
                    listEntity.Add(entity);
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipGetSerialNumberDataForShippingLot " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipGetSerialNumberDataForShippingLot " + error + "," + errorMsg, "");
            }
            return listEntity;
        }

        private string ConvertToDateTime(string strValue)
        {
            long numer = long.Parse(strValue);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(numer).ToLocalTime();
            return date.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public ShippingLotEntity GetShippingLotInfo(string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            ShippingLotEntity entity = new ShippingLotEntity();
            KeyValue[] shippingLotFilters = new KeyValue[] { new KeyValue("SHIPPING_LOT_NUMBER", lotNumber) };
            string[] shippingLotResultKeys = new string[] { "HU_NUMBER", "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_QTY_ACTUAL", "MATERIAL_BIN_QTY_TOTAL", "MATERIAL_BIN_STATE", "PART_DESC", "PART_NUMBER"
                , "QUANTITY_UNIT", "SHIPPING_COMPLETE_DATE","SHIPPING_LOT_NUMBER","SHIPPING_LOT_NUMBER2","SHIPPING_LOT_PART_NUMBER","SHIPPING_LOT_SNO_PART_DESCRIPTION","SHIPPING_LOT_SNO_PART_NUMBER"
                ,"SHIPPING_LOT_SNO_QTY_ACTUAL","SHIPPING_LOT_SNO_QTY_TOTAL","SHIPPING_SEND_DATE","WORKORDER_NUMBER" };
            string[] shippingLotResultValues = new string[] { };
            LogHelper.Info("begin api shipGetShippingLotInfo (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ")");
            error = imsapi.shipGetShippingLotInfo(sessionContext, init.configHandler.StationNumber, shippingLotFilters, shippingLotResultKeys, out shippingLotResultValues);
            LogHelper.Info("end api shipGetShippingLotInfo (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                int loop = shippingLotResultKeys.Length;
                int count = shippingLotResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    entity.HU_NUMBER = shippingLotResultValues[i + 0];
                    entity.MATERIAL_BIN_NUMBER = shippingLotResultValues[i + 1];
                    entity.MATERIAL_BIN_QTY_ACTUAL = shippingLotResultValues[i + 2];
                    entity.MATERIAL_BIN_QTY_TOTAL = shippingLotResultValues[i + 3];
                    entity.MATERIAL_BIN_STATE = shippingLotResultValues[i + 4];
                    entity.PART_DESC = shippingLotResultValues[i + 5];
                    entity.PART_NUMBER = shippingLotResultValues[i + 6];
                    entity.QUANTITY_UNIT = shippingLotResultValues[i + 7];
                    entity.SHIPPING_COMPLETE_DATE = shippingLotResultValues[i + 8];
                    entity.SHIPPING_LOT_NUMBER = shippingLotResultValues[i + 9];
                    entity.SHIPPING_LOT_NUMBER2 = shippingLotResultValues[i + 10];
                    entity.SHIPPING_LOT_PART_NUMBER = shippingLotResultValues[i + 11];
                    entity.SHIPPING_LOT_SNO_PART_DESCRIPTION = shippingLotResultValues[i + 12];
                    entity.SHIPPING_LOT_SNO_PART_NUMBER = shippingLotResultValues[i + 13];
                    entity.SHIPPING_LOT_SNO_QTY_ACTUAL = shippingLotResultValues[i + 14];
                    entity.SHIPPING_LOT_SNO_QTY_TOTAL = shippingLotResultValues[i + 15];
                    entity.SHIPPING_SEND_DATE = shippingLotResultValues[i + 16];
                    entity.WORKORDER_NUMBER = shippingLotResultValues[i + 17];
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipGetShippingLotInfo " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipGetShippingLotInfo " + error + "," + errorMsg, "");
            }
            return entity;
        }

        public int RemoveSNFormShippingLot(string lotNumber, string serialNumber)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api shipRemoveSerialNumberFromShippingLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ",serial number =" + serialNumber + ")");
            error = imsapi.shipRemoveSerialNumberFromShippingLot(sessionContext, init.configHandler.StationNumber, lotNumber, serialNumber, "-1", -1);
            LogHelper.Info("end api shipRemoveSerialNumberFromShippingLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipRemoveSerialNumberFromShippingLot " + error, "");
            }
            else//-418 'Serial number was not found in the lot'
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipRemoveSerialNumberFromShippingLot " + error + "," + errorMsg, "");
            }
            return error;
        }

        public ShippingLotEntity ReuseCompleteShippingLot(string lotNumber, int functionMode)
        {
            int error = 0;
            string errorMsg = "";
            ShippingLotEntity entity = new ShippingLotEntity();
            string[] shippingLotResultKeys = new string[] { "HU_NUMBER", "MATERIAL_BIN_NUMBER", "MATERIAL_BIN_QTY_ACTUAL", "MATERIAL_BIN_QTY_TOTAL", "MATERIAL_BIN_STATE", "PART_DESC", "PART_NUMBER"
                , "QUANTITY_UNIT","SHIPPING_LOT_NUMBER","SHIPPING_LOT_NUMBER2","SHIPPING_LOT_PART_NUMBER","SHIPPING_LOT_SNO_PART_DESCRIPTION","SHIPPING_LOT_SNO_PART_NUMBER"
                ,"SHIPPING_LOT_SNO_QTY_ACTUAL","SHIPPING_LOT_SNO_QTY_TOTAL","WORKORDER_NUMBER" };
            string[] shippingLotResultValues = new string[] { };
            LogHelper.Info("begin api shipReuseCompletedShippingLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ",function mode =" + functionMode + ")");
            error = imsapi.shipReuseCompletedShippingLot(sessionContext, init.configHandler.StationNumber, functionMode, lotNumber, shippingLotResultKeys, out shippingLotResultValues);
            LogHelper.Info("end api shipReuseCompletedShippingLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                int loop = shippingLotResultKeys.Length;
                int count = shippingLotResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    entity.HU_NUMBER = shippingLotResultValues[i + 0];
                    entity.MATERIAL_BIN_NUMBER = shippingLotResultValues[i + 1];
                    entity.MATERIAL_BIN_QTY_ACTUAL = shippingLotResultValues[i + 2];
                    entity.MATERIAL_BIN_QTY_TOTAL = shippingLotResultValues[i + 3];
                    entity.MATERIAL_BIN_STATE = shippingLotResultValues[i + 4];
                    entity.PART_DESC = shippingLotResultValues[i + 5];
                    entity.PART_NUMBER = shippingLotResultValues[i + 6];
                    entity.QUANTITY_UNIT = shippingLotResultValues[i + 7];
                    //entity.SHIPPING_COMPLETE_DATE = shippingLotResultValues[i + 8];
                    entity.SHIPPING_LOT_NUMBER = shippingLotResultValues[i + 8];
                    entity.SHIPPING_LOT_NUMBER2 = shippingLotResultValues[i + 9];
                    entity.SHIPPING_LOT_PART_NUMBER = shippingLotResultValues[i + 10];
                    entity.SHIPPING_LOT_SNO_PART_DESCRIPTION = shippingLotResultValues[i + 11];
                    entity.SHIPPING_LOT_SNO_PART_NUMBER = shippingLotResultValues[i + 12];
                    entity.SHIPPING_LOT_SNO_QTY_ACTUAL = shippingLotResultValues[i + 13];
                    entity.SHIPPING_LOT_SNO_QTY_TOTAL = shippingLotResultValues[i + 14];
                    entity.WORKORDER_NUMBER = shippingLotResultValues[i + 15];
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipReuseCompletedShippingLot " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipReuseCompletedShippingLot " + error + "," + errorMsg, "");
            }
            return entity;
        }

        public int SendShippingLot(string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            KeyValue[] shippingLotSendValues = new KeyValue[] { };
            LogHelper.Info("begin api shipSendLot (Station number:" + init.configHandler.StationNumber + ",lot number =" + lotNumber + ")");
            error = imsapi.shipSendLot(sessionContext, init.configHandler.StationNumber, lotNumber, -1, shippingLotSendValues);
            LogHelper.Info("end api shipSendLot (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " shipSendLot " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " shipSendLot " + error + "," + errorMsg, "");
            }
            return error;
        }
    }
}
