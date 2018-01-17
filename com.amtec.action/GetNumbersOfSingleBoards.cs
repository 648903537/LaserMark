using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;

namespace com.amtec.action
{
    public class GetNumbersOfSingleBoards
    {

        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private MainView view;
        private InitModel init;

        public GetNumbersOfSingleBoards(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public List<MdataGetPartData> GetNumbersOfSingleBoardsResultCall(String partNumber)
        {
            List<MdataGetPartData> dataModel = new List<MdataGetPartData>();
            KeyValue[] partFilter = new KeyValue[] { new KeyValue("PART_NUMBER", partNumber) };
            String[] partDataResultKey = new String[] { "QUANTITY_MULTIPLE_BOARD" };
            String[] partDataResultValues = new String[] { };
            LogHelper.Info("begin api mdataGetPartData (part no:" + partNumber + ")");
            int result = imsapi.mdataGetPartData(sessionContext, init.configHandler.StationNumber, partFilter, partDataResultKey, out partDataResultValues);
            LogHelper.Info("end api mdataGetPartData (result code = " + result + ")");
            if (result != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mdataGetPartData " + result, "");
                return null;
            }
            MdataGetPartData data = new MdataGetPartData();
            data.quantityMultipleBoard = int.Parse(partDataResultValues[0]);
            dataModel.Add(data);
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mdataGetPartData " + result, "");
            return dataModel;
        }
    }
}
