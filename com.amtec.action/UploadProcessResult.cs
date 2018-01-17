using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;

namespace com.amtec.action
{
    public class UploadProcessResult
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private int error;
        private MainView view;

        public UploadProcessResult(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int UploadProcessResultCall(String[] serialNumberArray, int processLayer)
        {
            String[] serialNumberUploadKey = new String[] { "ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE" };
            String[] serialNumberUploadValues = new String[] { };
            String[] serialNumberResultValues = new String[] { };
            serialNumberUploadValues = serialNumberArray;
            error = imsapi.trUploadState(sessionContext, init.configHandler.StationNumber, processLayer, "-1", "-1", 0, 0, -1, 0, serialNumberUploadKey, serialNumberUploadValues, out serialNumberResultValues);
            LogHelper.Info("Api trUploadState: result code =" + error);
            if ((error != 0) && (error != 210))
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trUploadState " + error, "");
                return error;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trUploadState " + error, "");
            return error;
        }

        public int UploadProcessResultCall(string serialNumber)
        {
            String[] serialNumberUploadKey = new String[] { "ERROR_CODE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE" };
            String[] serialNumberUploadValues = new String[] { };
            String[] serialNumberResultValues = new String[] { };
            LogHelper.Info("Api trUploadState(serial number = " + serialNumber);
            error = imsapi.trUploadState(sessionContext, init.configHandler.StationNumber, init.currentSettings.processLayer, serialNumber, "1", 0, 1, -1, 0, serialNumberUploadKey, serialNumberUploadValues, out serialNumberResultValues);

            if ((error != 0) && (error != 210))
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trUploadState " + error, "");
                return error;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trUploadState " + error, "");
            return error;
        }
    }
}
