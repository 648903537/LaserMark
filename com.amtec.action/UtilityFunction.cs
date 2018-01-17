using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Globalization;
using System.Text;

namespace com.amtec.action
{
    public class UtilityFunction
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private static IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public UtilityFunction(IMSApiSessionContextStruct _sessionContext, InitModel init, MainView view)
        {
            sessionContext = _sessionContext;
            this.init = init;
            this.view = view;
        }

        public static string GetZHSErrorString(int iErrorCode, InitModel _init, IMSApiSessionContextStruct _sessionContext)
        {
            string errorString = "";
            if (_init.configHandler.Language == "US")
            {
                imsapi.imsapiGetErrorText(sessionContext, iErrorCode, out errorString);
            }
            else
            {
                if (_init.ErrorCodeZHS.ContainsKey(iErrorCode))
                    errorString = _init.ErrorCodeZHS[iErrorCode];
                else
                {
                    imsapi.imsapiGetErrorText(sessionContext, iErrorCode, out errorString);
                }
            }

            return errorString;
        }

        public DateTime GetServerDateTime()
        {
            var calendarDataResultKeys = new string[] { "CURRENT_TIME_MILLIS" };
            var calendarDataResultValues = new string[] { };
            int error = imsapi.mdataGetCalendarData(sessionContext, init.configHandler.StationNumber, calendarDataResultKeys, out calendarDataResultValues);
            if (error != 0)
            {
                LogHelper.Info("API mdataGetCalendarData error code = " + error);
                return DateTime.Now;
            }
            long numer = long.Parse(calendarDataResultValues[0]);
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(numer).ToLocalTime();
            return date;
        }

        /// <summary>
        /// 字符串编码转换
        /// </summary>
        /// <param name="srcEncoding">原编码</param>
        /// <param name="dstEncoding">目标编码</param>
        /// <param name="srcBytes">原字符串</param>
        /// <returns>字符串</returns>
        public static string TransferEncoding(Encoding srcEncoding, Encoding dstEncoding, string srcStr)
        {
            byte[] srcBytes = srcEncoding.GetBytes(srcStr);
            byte[] bytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);
            return dstEncoding.GetString(bytes);
        }

        /// <summary>
        /// 字节数组转为字符串
        /// 将指定的字节数组的每个元素的数值转换为它的等效十六进制字符串表示形式。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string BitToString(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            //将指定的字节数组的每个元素的数值转换为它的等效十六进制字符串表示形式。
            return BitConverter.ToString(bytes);
        }

        /// <summary>
        /// 将十六进制字符串转为字节数组
        /// </summary>
        /// <param name="bitStr"></param>
        /// <returns></returns>
        public static byte[] FromBitString(string bitStr)
        {
            if (bitStr == null)
            {
                return null;
            }

            string[] sInput = bitStr.Split("-".ToCharArray());
            byte[] data = new byte[sInput.Length];
            for (int i = 0; i < sInput.Length; i++)
            {
                data[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
            }

            return data;
        }

          private string GetErpGroupNumber(string stationNumber)
        {
            string erpGroupNo = "";
            KeyValue[] machineAssetStructureFilter = new KeyValue[] { new KeyValue("DISSOLVING_LEVEL", "1"), new KeyValue("FUNC_MODE", "2"), new KeyValue("STATION_NUMBER", stationNumber) };
            string[] machineAssetStructureResultKeys = new string[] { "ERP_GROUP_NUMBER" };
            string[] machineAssetStructureValues = new string[] { };
            int error = imsapi.mdataGetMachineAssetStructure(sessionContext, stationNumber, machineAssetStructureFilter, machineAssetStructureResultKeys, out machineAssetStructureValues);
            LogHelper.Info("api mdataGetMachineAssetStructure (station number = " + stationNumber + "), error code =" + error);
            if (error == 0)
                erpGroupNo = machineAssetStructureValues[0];
            LogHelper.Info("Erp Group number = " + erpGroupNo);
            return erpGroupNo;
        }

        private string GetMachineGroup(string stationNumber)
        {
            string machineGroup = "";
            KeyValue[] machineAssetStructureFilter = new KeyValue[] { new KeyValue("DISSOLVING_LEVEL", "1"), new KeyValue("FUNC_MODE", "0"), new KeyValue("STATION_NUMBER", stationNumber) };
            string[] machineAssetStructureResultKeys = new string[] { "MACHINE_GROUP_NUMBER" };
            string[] machineAssetStructureValues = new string[] { };
            int error = imsapi.mdataGetMachineAssetStructure(sessionContext, stationNumber, machineAssetStructureFilter, machineAssetStructureResultKeys, out machineAssetStructureValues);
            LogHelper.Info("api mdataGetMachineAssetStructure (station number = " + stationNumber + "), error code =" + error);
            if (error == 0)
                machineGroup = machineAssetStructureValues[0];
            LogHelper.Info("Machine Group Number = " + machineGroup);
            return machineGroup;
        }

        public int GetProcessLayer(string stationNumber, string workorder)
        {
            int processLayer = 2;
            KeyValue[] workplanFilter = new KeyValue[] { new KeyValue("WORKORDER_NUMBER", workorder), new KeyValue("WORKSTEP_FLAG", "1") };
            string[] workplanDataResultKeys = new string[] { "PROCESS_LAYER" };
            string[] workplanDataResultValues = new string[] { };
            LogHelper.Info("begin api mdataGetWorkplanData (Work Order:" + init.currentSettings.workorderNumber + ")");
            int error = imsapi.mdataGetWorkplanData(sessionContext, stationNumber, workplanFilter, workplanDataResultKeys, out workplanDataResultValues);
            LogHelper.Info("end api mdataGetWorkplanData (result code = " + error + ")");
            if (error == 0)
            {
                processLayer = int.Parse(workplanDataResultValues[0]);
            }
            return processLayer;
        }
    }
}
