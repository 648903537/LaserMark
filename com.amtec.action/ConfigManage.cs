using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System.Collections.Generic;

namespace com.amtec.action
{
    public class ConfigManage
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private MainView view;

        public ConfigManage(IMSApiSessionContextStruct sessionContext, MainView view)
        {
            this.sessionContext = sessionContext;
            this.view = view;
        }

        public List<ConfigEntity> GetConfigData(string configAppID, string configAppType, string configCluster, string configStation)
        {
            List<ConfigEntity> entityList = null;
            KeyValue[] options = new KeyValue[] { };
            KeyValue[] configContext = new KeyValue[] { new KeyValue("CONFIG_APPID", configAppID), new KeyValue("CONFIG_APPTYPE", configAppType), new KeyValue("CONFIG_CLUSTER", configCluster), new KeyValue("CONFIG_STATION", configStation) };
            KeyValue[] parameterFilter = new KeyValue[] { new KeyValue("PARAMETER_SCOPE", configAppType) };
            string[] parameterResultKeys = new string[] { };
            string[] parameterResultValues = new string[] { };
            string[] resultKeys = new string[] { "CONFIG_VALUE", "PARAMETER_ID", "PARAMETER_INDEX", "PARAMETER_NAME" };
            string[] resultValues = new string[] { };
            int errorCode = imsapi.configGetValues(sessionContext, options, configContext, parameterFilter, parameterResultKeys, out parameterResultValues, resultKeys, out resultValues);
            LogHelper.Info("Api configGetValues error code=" + errorCode);
            if (errorCode == 0)
            {
                int loop = resultKeys.Length;
                int count = resultValues.Length;
                entityList = new List<ConfigEntity>();
                for (int i = 0; i < count; i += loop)
                {
                    ConfigEntity entity = new ConfigEntity();
                    entity.CONFIG_VALUE = resultValues[i + 0];
                    entity.PARAMETER_ID = resultValues[i + 1];
                    entity.PARAMETER_INDEX = resultValues[i + 2];
                    entity.PARAMETER_NAME = resultValues[i + 3];
                    entityList.Add(entity);
                }
            }
            return entityList;
        }

        public int CreateConfigParameter(string[] paramaterValues)
        {
            int errorCode = 0;
            KeyValue[] options = new KeyValue[] { };
            string[] parameterUploadKeys = new string[]{"PARAMETER_DESCRIPTION","PARAMETER_DIMPATH","PARAMETER_DISPLAYNAME","PARAMETER_NAME"
                ,"PARAMETER_PARENT_NAME","PARAMETER_SCOPE","PARAMETER_TYPE_NAME"};
            string[] parameterUploadValues = paramaterValues;
            string[] parameterResultKeys = new string[] { "ERROR_CODE" };
            string[] parameterResultValues = new string[] { };
            errorCode = imsapi.configCreateParameters(sessionContext, options, parameterUploadKeys, parameterUploadValues, parameterResultKeys, out parameterResultValues);
            LogHelper.Info("Api configCreateParameters error" + errorCode);
            return errorCode;
        }

        public int UpdateParameterValues(string configAppID, string configAppType, string configCluster, string configStation, string[] uploadValues)
        {
            KeyValue[] options = new KeyValue[] { };
            KeyValue[] configContext = new KeyValue[] { new KeyValue("CONFIG_APPID", configAppID), new KeyValue("CONFIG_APPTYPE", configAppType), new KeyValue("CONFIG_CLUSTER", configCluster), new KeyValue("CONFIG_STATION", configStation) };
            string[] uploadKeys = new string[] { "CONFIG_VALUE", "PARAMETER_NAME" };
            string[] resultKeys = new string[] { "CONFIG_VALUE", "ERROR_CODE", "PARAMETER_NAME" };
            string[] resultValues = new string[] { };
            int errorCode = imsapi.configUpdateValues(sessionContext, options, configContext, uploadKeys, uploadValues, resultKeys, out resultValues);
            LogHelper.Info("Api configUpdateValues error" + errorCode);
            return errorCode;
        }

        public int DeleteConfigParameters(string parameterScope)
        {
            KeyValue[] options = new KeyValue[] { };
            KeyValue[] parameterFilter = new KeyValue[] { new KeyValue("PARAMETER_SCOPE", parameterScope) };
            string[] parameterResultKeys = new string[] { "ERROR_CODE" };
            string[] parameterResultValues = new string[] { };
            int errorCode = imsapi.configDeleteParameters(sessionContext, options, parameterFilter, parameterResultKeys, out parameterResultValues);
            LogHelper.Info("Api configDeleteParameters error" + errorCode);
            return errorCode;
        }

        public int DeleteConfigParametersExt(string parameterID)
        {
            KeyValue[] options = new KeyValue[] { };
            KeyValue[] parameterFilter = new KeyValue[] { new KeyValue("PARAMETER_ID", parameterID) };
            string[] parameterResultKeys = new string[] { "ERROR_CODE" };
            string[] parameterResultValues = new string[] { };
            int errorCode = imsapi.configDeleteParameters(sessionContext, options, parameterFilter, parameterResultKeys, out parameterResultValues);
            LogHelper.Info("Api configDeleteParameters error" + errorCode + ", parameter id =" + parameterID);
            return errorCode;
        }

        public string[] GetParametersForScope(string parameterScope)
        {
            KeyValue[] options = new KeyValue[] { };
            KeyValue[] parameterFilter = new KeyValue[] { new KeyValue("PARAMETER_SCOPE", parameterScope) };
            string[] parameterResultKeys = new string[] { "PARAMETER_ID" };
            string[] parameterResultValues = new string[] { };
            int errorCode = imsapi.configGetParameters(sessionContext, options, parameterFilter, parameterResultKeys, out parameterResultValues);
            LogHelper.Info("Api configGetParameters error" + errorCode + ", parameter scope =" + parameterScope);
            return parameterResultValues;
        }
    }
}
