using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class AppendAttribute
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public AppendAttribute(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int AppendAttributeValues(string strCode, string strValue)
        {
            int error = 0;
            string errorMsg = "";
            string[] attributeUploadKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[] { strCode, strValue, "0" };
            string[] attributeResultValues = new string[] { };
            LogHelper.Info("begin api attribAppendAttributeValues (attribute code =" + strCode + ",attribute value=" + strValue + ")");
            error = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, 7, init.configHandler.StationNumber, "-1", -1, 1
                , attributeUploadKeys, attributeUploadValues, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            LogHelper.Info("end api attribAppendAttributeValues (result code = " + error + ")");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error + "(" + errorMsg + ")", "");
            }
            return error;
        }

        public int AppendAttributeValuesForContainer(string strCode, string strValue, string lotNumber)
        {
            int error = 0;
            string errorMsg = "";
            string[] attributeUploadKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[] { strCode, strValue, "0" };
            string[] attributeResultValues = new string[] { };
            LogHelper.Info("begin api attribAppendAttributeValues (attribute code =" + strCode + ",attribute value=" + strValue + ")");
            error = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, 2, lotNumber, "-1", -1, 1
                , attributeUploadKeys, attributeUploadValues, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            LogHelper.Info("end api attribAppendAttributeValues (result code = " + error + ")");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error + "(" + errorMsg + ")", "");
            }
            return error;
        }

        public int AppendAttributeValuesForWO(string strCode, string strValue, string strDesc, string strWorkOrder)
        {
            int error = 0;
            string errorMsg = "";
            string[] attributeUploadKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[] { strCode, strValue, "0" };
            string[] attributeResultValues = new string[] { };
            //LogHelper.Info("begin attribAppendAttributeValues (material bin number =" + strMaterialBin + ",ATTRIBUTE_CODE =" + strCode + ",ATTRIBUTE_VALUE =" + strValue + ")");
            error = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, 1, strWorkOrder, "-1", -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            //LogHelper.Info("end attribAppendAttributeValues error=" + error + "");
            if (error == 0)
            {
                // view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error, "");
            }
            else
            {
                if (attributeResultValues[2] == "-901")//attribute code not exist, create
                {
                    imsapi.attribCreateAttribute(sessionContext, init.configHandler.StationNumber, 1, strCode, strDesc, "N");
                    int error2 = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, 1, strWorkOrder, "-1", -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
                    //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
                    errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
                    if (error2 == 0)
                    {
                        // view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error2, "");
                    }
                    else
                    {
                        view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error2 + "(" + errorMsg + ")", "");
                    }
                    return error2;
                }
            }
            return error;
        }

        public int AppendAttributeValuesForSN(string strCode, string strValue, string strDesc, string serialNumber)
        {
            int error = 0;
            string errorMsg = "";
            string[] attributeUploadKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[] { strCode, strValue, "0" };
            string[] attributeResultValues = new string[] { };
            //LogHelper.Info("begin attribAppendAttributeValues (material bin number =" + strMaterialBin + ",ATTRIBUTE_CODE =" + strCode + ",ATTRIBUTE_VALUE =" + strValue + ")");
            error = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, 0, serialNumber, "-1", -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            //LogHelper.Info("end attribAppendAttributeValues error=" + error + "");
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error, "");
            }
            else
            {
                if (attributeResultValues[2] == "-901")//attribute code not exist, create
                {
                    imsapi.attribCreateAttribute(sessionContext, init.configHandler.StationNumber, 0, strCode, strDesc, "N");
                    int error2 = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, 0, serialNumber, "-1", -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
                    //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
                    errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
                    if (error2 == 0)
                    {
                        view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error2, "");
                    }
                    else
                    {
                        view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error2 + "(" + errorMsg + ")", "");
                    }
                    return error2;
                }
            }
            return error;
        }
        public int AppendAttributeForAll(int objectType, string objectNumber, string objectDetail, string attributeCode, string sttributeValue)
        {
            int error = 0;
            string errorMsg = "";
            string[] attributeUploadKeys = new string[] { "ATTRIBUTE_CODE", "ATTRIBUTE_VALUE", "ERROR_CODE" };
            string[] attributeUploadValues = new string[] { attributeCode, sttributeValue, "0" };
            string[] attributeResultValues = new string[] { };
            error = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, objectType, objectNumber, objectDetail, -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
            LogHelper.Info("Api attribAppendAttributeValues error=" + error + ",object type=" + objectType + ",object number=" + objectNumber + ",object detail=" + objectDetail);
            if (error == 0)
            {
                //view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error, "");
            }
            else
            {
                if (attributeResultValues[2] == "-901")//attribute code not exist, create
                {
                    imsapi.attribCreateAttribute(sessionContext, init.configHandler.StationNumber, objectType, attributeCode, attributeCode, "N");
                    error = imsapi.attribAppendAttributeValues(sessionContext, init.configHandler.StationNumber, objectType, objectNumber, objectDetail, -1, 1, attributeUploadKeys, attributeUploadValues, out attributeResultValues);
                    if (error == 0)
                    {
                        //view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error, "");
                    }
                    else
                    {
                        //view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " attribAppendAttributeValues " + error + "(" + errorMsg + ")", "");
                    }
                }
            }
            return error;
        }
    }
}
