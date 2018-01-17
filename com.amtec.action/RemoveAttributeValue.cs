using com.amtec.action;
using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class RemoveAttributeValue
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public RemoveAttributeValue(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int RemoveAttriValue(string attributeCode, string workorder)
        {
            int error = 0;
            string errorMsg = "";
            LogHelper.Info("begin api attribRemoveAttributeValue (station number =" + init.configHandler.StationNumber + ",attribute code=" + attributeCode + ")");
            error = imsapi.attribRemoveAttributeValue(sessionContext, init.configHandler.StationNumber, 1, workorder, "-1", attributeCode, "-1");
            LogHelper.Info("end api attribRemoveAttributeValue (result code = " + error + ")");
            //imsapi.imsapiGetErrorText(sessionContext, error, out errorMsg);
            errorMsg = UtilityFunction.GetZHSErrorString(error, init, sessionContext);
            if (error == 0)
            {
                //view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " attribRemoveAttributeValue " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " attribRemoveAttributeValue " + error + "(" + errorMsg + ")", "");
            }
            return error;
        }

        public int RemoveAttributeForAll(int objectType, string objectNumber, string objectDetail, string attributeCode)
        {
            int error = imsapi.attribRemoveAttributeValue(sessionContext, init.configHandler.StationNumber, objectType, objectNumber, objectDetail, attributeCode, "-1");
            LogHelper.Info("api attribGetAttributeValues (object type =" + objectType + ",object number =" + objectNumber + ",attribute code =" + attributeCode + ", error code =" + error + ")");
            return error;
        }
    }
}
