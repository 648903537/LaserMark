using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.action
{
    public class GetUserData
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private int error;

        public GetUserData(IMSApiSessionContextStruct sessionContext)
        {
            this.sessionContext = sessionContext;
        }

        public string[] mdataGetUserData(string username,string password,string station)
        {
            KeyValue[] mdataGetUserDataFilter = new KeyValue[] { new KeyValue("USER_NAME", username), new KeyValue("USER_PASSWORD", password) };
            string[] mdataGetUserDataKeys = new string[] { "TEAM_CODE","TEAM_DESC","TEAM_NUMBER" };
            string[] mdataGetUserDataValues = new string[] { };
            bool hasmore=false;
            int error = imsapi.mdataGetUserData(sessionContext, station, mdataGetUserDataFilter, mdataGetUserDataKeys, out mdataGetUserDataValues, out hasmore);
            LogHelper.Info("mdataGetUserData username =" + username + ",result code = " + error);
            if (error != 0)
            {
                LogHelper.Error("mdataGetUserData " + error);
            }
            else
            {
                LogHelper.Info("mdataGetUserData " + error);
            }
            return mdataGetUserDataValues;
        }
    }
}
