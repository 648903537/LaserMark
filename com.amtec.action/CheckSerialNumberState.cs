using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;

namespace com.amtec.action
{
    public class CheckSerialNumberState
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public CheckSerialNumberState(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public List<SerialNumberStateEntity> GetSerialNumberData(string serialNumber)
        {
            List<SerialNumberStateEntity> snList = new List<SerialNumberStateEntity>();
            String[] serialNumberStateResultKeys = new String[] { "LOCK_STATE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE" };
            String[] serialNumberStateResultValues = new String[] { };
            LogHelper.Info("begin api trCheckSerialNumberState (Serial number:" + serialNumber + ")");
            int error = imsapi.trCheckSerialNumberState(sessionContext, init.configHandler.StationNumber, init.currentSettings.processLayer, 1, serialNumber, "-1", serialNumberStateResultKeys, out serialNumberStateResultValues);
            LogHelper.Info("end api trCheckSerialNumberState (result code = " + error + ")");
            if ((error != 0) && (error != 5) && (error != 6) && (error != 204) && (error != 207) && (error != 212))
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trCheckSerialNumberState " + error, "");
            }
            else
            {
                int counter = serialNumberStateResultValues.Length;
                int loop = serialNumberStateResultKeys.Length;
                for (int i = 0; i < counter; i += loop)
                {
                    snList.Add(new SerialNumberStateEntity(serialNumberStateResultValues[i], serialNumberStateResultValues[i + 1], serialNumberStateResultValues[i + 2], serialNumberStateResultValues[i + 3]));
                }
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trCheckSerialNumberState " + error, "");
            }
            return snList;
        }

        public bool CheckSNState(string serialNumber)
        {
            String[] serialNumberStateResultKeys = new String[] { "LOCK_STATE", "SERIAL_NUMBER", "SERIAL_NUMBER_POS", "SERIAL_NUMBER_STATE" };
            String[] serialNumberStateResultValues = new String[] { };
            LogHelper.Info("begin api trCheckSerialNumberState (Serial number:" + serialNumber + ")");
            int error = imsapi.trCheckSerialNumberState(sessionContext, init.configHandler.StationNumber, init.currentSettings.processLayer, 1, serialNumber, "-1", serialNumberStateResultKeys, out serialNumberStateResultValues);
            LogHelper.Info("end api trCheckSerialNumberState (result code = " + error + ")");
            if ((error != 0) && (error != 5) && (error != 6) && (error != 204) && (error != 207) && (error != 212))
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trCheckSerialNumberState " + error, "");
                return false;
            }
            else
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trCheckSerialNumberState " + error, "");
            }
            return true;
        }
    }
}
