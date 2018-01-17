using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.model
{
    public class ShippingLotEntity
    {
        public string HU_NUMBER { get; set; }

        public string MATERIAL_BIN_NUMBER { get; set; }

        public string MATERIAL_BIN_QTY_ACTUAL { get; set; }

        public string MATERIAL_BIN_QTY_TOTAL { get; set; }

        public string MATERIAL_BIN_STATE { get; set; }

        public string PART_DESC { get; set; }

        public string PART_NUMBER { get; set; }

        public string QUANTITY_UNIT { get; set; }

        public string SHIPPING_COMPLETE_DATE { get; set; }

        public string SHIPPING_LOT_NUMBER { get; set; }

        public string SHIPPING_LOT_NUMBER2 { get; set; }

        public string SHIPPING_LOT_PART_NUMBER { get; set; }


        public string SHIPPING_LOT_SNO_PART_DESCRIPTION { get; set; }

        public string SHIPPING_LOT_SNO_PART_NUMBER { get; set; }

        public string SHIPPING_LOT_SNO_QTY_ACTUAL { get; set; }

        public string SHIPPING_LOT_SNO_QTY_TOTAL { get; set; }

        public string SHIPPING_SEND_DATE { get; set; }

        public string WORKORDER_NUMBER { get; set; }
    }
}
