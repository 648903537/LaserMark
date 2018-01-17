using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.model
{
    public class CommonModel
    {
        public string Station { get; set; }
        public string Client { get; set; }
        public string RegisterType { get; set; }
        public string APPTYPE { get; set; }
        public string APPID { get; set; }
        public string Cluster { get; set; }
        public string UpdateConfig { get; set; }
    }

    public enum MessageType
    {
        OK,
        Error,
        Warning,
        Instruction
    }

    public enum PFCMessage
    {
        PING,
        GO,
        COMPLETE,
        CONFIRM
    }
}
