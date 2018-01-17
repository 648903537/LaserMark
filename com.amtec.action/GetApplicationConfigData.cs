using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Linq;

namespace com.amtec.action
{
    public class GetApplicationConfigData
    {
        DataTable dt = null;
        public GetApplicationConfigData()
        {
          
        }

        public DataTable GetConfigData()
        {
            dt = new DataTable();
            dt.Columns.Add("ParameterName", typeof(string));
            dt.Columns.Add("ParameterValue", typeof(string));
            XDocument config = XDocument.Load("ApplicationConfig.xml");
            foreach (var item in config.Descendants())
            {
                if (!item.HasElements)
                {
                    DataRow row = dt.NewRow();
                    row["ParameterName"] = item.Name;
                    row["ParameterValue"] = item.Value;
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }
    }
}
