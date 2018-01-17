using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.client.dotnet;
using com.itac.mes.imsapi.domain.container;

namespace com.amtec.action
{
    public class ProcessMaterialBinData
    {
        private static IMSApiDotNet imsapi = IMSApiDotNet.loadLibrary();
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        private MainView view;

        public ProcessMaterialBinData(IMSApiSessionContextStruct sessionContext, InitModel init, MainView view)
        {
            this.sessionContext = sessionContext;
            this.init = init;
            this.view = view;
        }

        public int UpdateMaterialBinBooking(string materialBin, string workorder, double strQty)
        {
            string[] materialBinBookingUploadKeys = new string[] { "ERROR_CODE", "MATERIAL_BIN_NUMBER", "QUANTITY", "STATION_NUMBER", "TRANSACTION_CODE", "WORKORDER_NUMBER" };
            string[] materialBinBookingUploadValues = new string[] { "0", materialBin, strQty.ToString(), init.configHandler.StationNumber, "0", workorder };
            string[] materialBinBookingResultValues = new string[] { };
            int error = imsapi.mlUploadMaterialBinBooking(sessionContext, init.configHandler.StationNumber, materialBinBookingUploadKeys, materialBinBookingUploadValues, out materialBinBookingResultValues);
            LogHelper.Info("Api mlUploadMaterialBinBooking (material bin number =" + materialBin + ",quantity=" + strQty + ",result code =" + error + ")");
            if (error != 0)
            {
                view.errorHandler(2, init.lang.ERROR_API_CALL_ERROR + " mlUploadMaterialBinBooking " + error, "");
                return error;
            }
            view.errorHandler(0, init.lang.ERROR_API_CALL_ERROR + " mlUploadMaterialBinBooking " + error, "");
            return error;
        }
    }
}
