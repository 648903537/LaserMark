using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.model
{
    public class ProductEntity
    {
        public string QUANTITY_FAIL { get; set; }

        public string QUANTITY_PASS { get; set; }

        public string QUANTITY_SCRAP { get; set; }

        public string QUANTITY_WORKORDER_FINISHED { get; set; }

        public string QUANTITY_WORKORDER_STARTED { get; set; }

        public string QUANTITY_WORKORDER_TOTAL { get; set; }

        public string STATION_NUMBER { get; set; }

        public string WORKSTEP_NUMBER { get; set; }
    }
}
