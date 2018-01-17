using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.model
{
    public class GetStationSettingModel
    {
        public String bomVersion { get; set; }

        public String attribute1 { get; set; }

        public String workorderNumber { get; set; }

        public String partNumber { get; set; }

        public String workorderState { get; set; }

        public int processVersion { get; set; }

        public int getError { get; set; }

        public int processLayer { get; set; }

        public int QuantityMO { get; set; }

        public String partdesc { get; set; }

    }
}
