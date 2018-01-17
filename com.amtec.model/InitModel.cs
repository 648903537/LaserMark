using com.amtec.configurations;
using com.amtec.device;
using System.Collections.Generic;

namespace com.amtec.model
{
    public class InitModel
    {

        public ScannerHeandler scannerHandler { get; set; }

        //public DatabaseConnection dbHandler { get; set; }

        public ApplicationConfiguration configHandler { get; set; }

        public GetStationSettingModel currentSettings { get; set; }

        public LanguageResources lang { get; set; }

        public int numberOfSingleBoards { get; set; }

        public Dictionary<int, string> ErrorCodeZHS { get; set; }
    }
}
