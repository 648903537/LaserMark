using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class CheckUserSkill
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public CheckUserSkill(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int CheckUserSkillForWS(string userName)
        {
            int errorCode = 0;
            KeyValue[] checkUserSkillFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", init.currentSettings.workorderNumber) };
            LogHelper.Info("begin api trCheckUserSkill (Station number:" + init.configHandler.StationNumber + ",process layer:" + init.currentSettings.processLayer + ",user name:" + userName + ")");
            errorCode = imsapi.trCheckUserSkill(sessionContext, init.configHandler.StationNumber, init.currentSettings.processLayer, userName, checkUserSkillFilter);
            LogHelper.Info("end api trCheckUserSkill (result code = " + errorCode + ")");
            if (errorCode != 0)
            {
                view.errorHandler(3, init.lang.ERROR_API_CALL_ERROR + " trCheckUserSkill " + errorCode, "");
            }
            else
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trCheckUserSkill " + errorCode, "");
            }
            return errorCode;
        }
    }
}
