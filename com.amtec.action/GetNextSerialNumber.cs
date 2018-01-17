using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class GetNextSerialNumber
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public GetNextSerialNumber(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public SerialNumberData[] GetSerialNumber(string workOrderNumber, int numberRecords)
        {
            SerialNumberData[] serialNumberArray = new SerialNumberData[] { };
            int error = imsapi.trGetNextSerialNumber(sessionContext, init.configHandler.StationNumber, workOrderNumber, "-1", numberRecords, out serialNumberArray);
            if (error == 0)
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trGetNextSerialNumber " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trGetNextSerialNumber " + error, "");
            }
            return serialNumberArray;
        }
    }
}
