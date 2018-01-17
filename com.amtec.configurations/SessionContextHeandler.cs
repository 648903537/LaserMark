using com.amtec.action;
using com.amtec.forms;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.configurations
{
    public class SessionContextHeandler
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionValidationStruct sessionValidationStruct;
        private IMSApiSessionContextStruct sessionContext = null;
        private int initResult;
        private LoginForm mainView;


        public SessionContextHeandler(ApplicationConfiguration config, LoginForm mainView)
        {
            this.mainView = mainView;
            //sessionValidationStruct = new IMSApiSessionValidationStruct();
            //sessionValidationStruct.stationNumber = config.StationNumber;
            //sessionValidationStruct.stationPassword = "";
            //sessionValidationStruct.user = "";
            //sessionValidationStruct.password = "";
            //sessionValidationStruct.client = config.Client;
            //sessionValidationStruct.registrationType = config.RegistrationType;
            //sessionValidationStruct.systemIdentifier = config.StationNumber;

            initResult = imsapi.imsapiInit();

            if (initResult != 0)
            {
                mainView.SetStatusLabelText("Conncection to DMS failed", 1);
                mainView.isCanLogin = false;
                LogHelper.Info("Conncection to DMS failed");
            }
            else
            {
                mainView.SetStatusLabelText("Conncection to DMS established", 0);
                mainView.isCanLogin = true;
                LogHelper.Info("Conncection to DMS established");
            }
        }

        public IMSApiSessionContextStruct getSessionContext()
        {
            if (initResult != IMSApiDotNetConstants.RES_OK)
            {
                return null;
            }
            else
            {
                int result = imsapi.regLogin(sessionValidationStruct, out sessionContext);
                if (result != IMSApiDotNetConstants.RES_OK)
                {
                    return null;
                }
                else
                {
                    return sessionContext;
                }
            }
        }
    }
}
