using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System.Collections.Generic;

namespace com.amtec.action
{
    public class GetAttributeValue
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public GetAttributeValue(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public string GetAttributeValuesForStation()
        {
            string returnValue = "";
            string errorMsg = "";
            string[] attributeCodeArray = new string[] { "attribActivatedStationLotNo" };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            LogHelper.Info("begin api attribGetAttributeValues (station number =" + init.configHandler.StationNumber + ")");
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 7, init.configHandler.StationNumber, "-1", attributeCodeArray, 0, attributeResultKeys, out attributeResultValues);
            LogHelper.Info("end api attribGetAttributeValues (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                returnValue = attributeResultValues[1];
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribGetAttributeValues " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " attribGetAttributeValues " + error + "(" + errorMsg + ")", "");
            }
            return returnValue;
        }

        public string GetAttributeValues(string serialNumber)
        {
            string errorMsg = "";
            string returnValue = "";
            Dictionary<string, string> dicValues = new Dictionary<string, string>();
            string[] attributeCodeArray = new string[] { "PartNumber_CompName" };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            LogHelper.Info("begin api attribGetAttributeValues (serial number =" + serialNumber + ")");
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 0, serialNumber, "-1", attributeCodeArray, 1, attributeResultKeys, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            LogHelper.Info("end api attribGetAttributeValues (result code = " + error + ")");
            if (error == 0)
            {
                int loop = attributeResultKeys.Length;
                int count = attributeResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    dicValues[attributeResultValues[i]] = attributeResultValues[i + 1];
                }
                returnValue = attributeResultValues[1];
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribGetAttributeValues " + error, "");
            }
            else
            {
                view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " attribGetAttributeValues " + error + "(" + errorMsg + ")", "");
            }
            return returnValue;
        }

        public Dictionary<string, string> GetAllAttributeValues(string serialNumber)
        {
            string errorMsg = "";
            string returnValue = "";
            Dictionary<string, string> dicValues = new Dictionary<string, string>();
            string[] attributeCodeArray = new string[] { };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            LogHelper.Info("begin api attribGetAttributeValues (serial number =" + serialNumber + ")");
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 0, serialNumber, "-1", attributeCodeArray, 1, attributeResultKeys, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            LogHelper.Info("end api attribGetAttributeValues (result code = " + error + ")");
            if (error == 0)
            {
                int loop = attributeResultKeys.Length;
                int count = attributeResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    dicValues[attributeResultValues[i]] = attributeResultValues[i + 1];
                }
                returnValue = attributeResultValues[1];
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribGetAttributeValues " + error, "");
            }
            else
            {
                view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " attribGetAttributeValues " + error + "(" + errorMsg + ")", "");
            }
            return dicValues;
        }

        public string GetObjectValeForSN(string stationNumber, string attriCode, string attriValue)//attriCode =*
        {
            string serialNumber = "";
            string errorMsg = "";
            KeyValue[] attributeFilters = new KeyValue[] { };
            string[] objectResultKeys = new string[] { "SERIAL_NUMBER" };
            string[] objectResultValues = new string[] { };
            LogHelper.Info("begin api attribGetObjectsForAttributeValues (station number =" + stationNumber + ",attribute code = " + attriCode + ",attribute value = " + attriValue + ")");
            int error = imsapi.attribGetObjectsForAttributeValues(sessionContext, stationNumber, 0, attriCode, null, 0, attributeFilters, objectResultKeys, out objectResultValues);
            LogHelper.Info("end api attribGetObjectsForAttributeValues (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribGetObjectsForAttributeValues " + error, "");
                serialNumber = objectResultValues[0];
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " attribGetObjectsForAttributeValues " + error + "(" + errorMsg + ")", "");
            }
            return serialNumber;
        }

        public Dictionary<string, string> GetAllAttributeValuesForWO(string workorder)
        {
            string errorMsg = "";
            Dictionary<string, string> dicValues = new Dictionary<string, string>();
            string[] attributeCodeArray = new string[] { };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            LogHelper.Info("begin api attribGetAttributeValues (work order number =" + workorder + ")");
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 1, workorder, "-1", attributeCodeArray, 1, attributeResultKeys, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            LogHelper.Info("end api attribGetAttributeValues (result code = " + error + ", error message =" + errorMsg + ")");
            if (error == 0)
            {
                int loop = attributeResultKeys.Length;
                int count = attributeResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    dicValues[attributeResultValues[i]] = attributeResultValues[i + 1];
                }
            }
            return dicValues;
        }

        public string[] GetAttributeValueForContainer(string attributeCode, string containerID)
        {
            string[] attributeCodeArray = new string[] { attributeCode };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 2, containerID, "-1", attributeCodeArray, 0, attributeResultKeys, out attributeResultValues);
            LogHelper.Info("api attribGetAttributeValues (material bin number =" + containerID + ", error code =" + error + ")");
            return attributeResultValues;
        }

        public string[] GetAttributeValueForEquipment(string attributeCode, string equipmentNo, string equipmentIndex)
        {
            string[] attributeCodeArray = new string[] { attributeCode };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 15, equipmentNo, equipmentIndex, attributeCodeArray, 0, attributeResultKeys, out attributeResultValues);
            LogHelper.Info("api attribGetAttributeValues (equipment number =" + equipmentNo + ", error code =" + error + ")");
            return attributeResultValues;
        }

        public Dictionary<string, string> GetAttributeValueForPart(string[] attributeCodes, string partNumber)
        {
            Dictionary<string, string> dicValues = new Dictionary<string, string>();
            string[] attributeCodeArray = attributeCodes;
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, 10, partNumber, "-1", attributeCodeArray, 0, attributeResultKeys, out attributeResultValues);
            LogHelper.Info("api attribGetAttributeValues (part number =" + partNumber + ", error code =" + error + ")");
            if (error == 0)
            {
                int loop = attributeResultKeys.Length;
                int count = attributeResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    dicValues[attributeResultValues[i]] = attributeResultValues[i + 1];
                }

            }
            return dicValues;
        }

        public string[] GetAttributeValueForAll(int objectType, string objectNumber, string objectDetail, string attributeCode)
        {
            string[] attributeCodeArray = new string[] { attributeCode };
            string[] attributeResultKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeResultValues = new string[] { };
            int error = imsapi.attribGetAttributeValues(sessionContext, init.configHandler.StationNumber, objectType, objectNumber, objectDetail, attributeCodeArray, 0, attributeResultKeys, out attributeResultValues);
            LogHelper.Info("api attribGetAttributeValues (object type =" + objectType + ",object number =" + objectNumber + ",attribute code =" + attributeCode + ", error code =" + error + ")");
            return attributeResultValues;
        }
    }
}
