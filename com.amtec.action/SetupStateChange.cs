using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class SetupStateChange
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public SetupStateChange(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int ClearSetUpForStation(int processLayer)
        {

            int error = imsapi.setupStateChange(sessionContext, init.configHandler.StationNumber, processLayer, "-1", "-1", -1, 2);
            if ((error == 0) || (error == -133) || (error == -605))
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " setupStateChange " + error, "");
            }
            else
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " setupStateChange " + error, "");
            }
            return error;
        }
    }
}
