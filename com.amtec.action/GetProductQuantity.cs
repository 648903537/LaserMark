using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class GetProductQuantity
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public GetProductQuantity(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }


        public ProductEntity GetProductQty(int processLayer, string workorder)
        {
            ProductEntity entity = null;
            KeyValue[] workorderFilters = new KeyValue[] { };
            KeyValue[] filterDataArray = new KeyValue[] { new KeyValue("0", workorder) };
            string[] productQuantityResultKeys = new string[] { "QUANTITY_FAIL", "QUANTITY_PASS", "QUANTITY_SCRAP", "QUANTITY_WORKORDER_FINISHED"
                , "QUANTITY_WORKORDER_STARTED", "QUANTITY_WORKORDER_TOTAL", "STATION_NUMBER", "WORKSTEP_NUMBER" };
            string[] productQuantityResultValues = new string[] { };
            LogHelper.Info("begin api trGetProductQuantity (Station number:" + init.configHandler.StationNumber + ",Process layer:" + processLayer + ",Workorder number:" + workorder + ")");
            int error = imsapi.trGetProductQuantity(sessionContext, init.configHandler.StationNumber, processLayer, 1, workorderFilters, filterDataArray, productQuantityResultKeys, out productQuantityResultValues);
            LogHelper.Info("end api trGetProductQuantity (result code = " + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " trGetProductQuantity " + error, "");
                return null;
            }
            else
            {
                view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " trGetProductQuantity " + error, "");
            }
            if (productQuantityResultValues.Length > 0)
            {
                entity = new ProductEntity();
                int loop = productQuantityResultKeys.Length;
                int count = productQuantityResultValues.Length;
                for (int i = 0; i < count; i += loop)
                {
                    entity.QUANTITY_FAIL = productQuantityResultValues[i + 0];
                    entity.QUANTITY_PASS = productQuantityResultValues[i + 1];
                    entity.QUANTITY_SCRAP = productQuantityResultValues[i + 2];
                    entity.QUANTITY_WORKORDER_FINISHED = productQuantityResultValues[i + 3];
                    entity.QUANTITY_WORKORDER_STARTED = productQuantityResultValues[i + 4];
                    entity.QUANTITY_WORKORDER_TOTAL = productQuantityResultValues[i + 5];
                    entity.STATION_NUMBER = productQuantityResultValues[i + 6];
                    entity.WORKSTEP_NUMBER = productQuantityResultValues[i + 7];
                }
            }
            return entity;
        }
    }
}
