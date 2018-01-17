using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.model
{
    public class EquipmentEntity
    {
        public string EQUIPMENT_NUMBER { get; set; }

        public string EQUIPMENT_DESCRIPTION { get; set; }

        public string PART_NUMBER { get; set; }

        public string EQUIPMENT_CHECKSTATE { get; set; }
    }

    public class EquipmentEntityExt
    {
        public string PART_NUMBER { get; set; }

        public string EQUIPMENT_INDEX { get; set; }

        public string EQUIPMENT_NUMBER { get; set; }

        public string EQUIPMENT_STATE { get; set; }

        public string SECONDS_BEFORE_EXPIRATION { get; set; }

        public string USAGES_BEFORE_EXPIRATION { get; set; }
    }
}
