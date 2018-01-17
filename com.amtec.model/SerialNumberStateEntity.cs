using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.model
{
    public class SerialNumberStateEntity
    {
        public string lockState { get; set; }
        public string serialNumber { get; set; }
        public string serialNumberPos { get; set; }
        public string serialNumberState { get; set; }

        public SerialNumberStateEntity(string strLockState, string strSN, string strSNPos, string strSNState)
        {
            this.serialNumber = strSN;
            this.serialNumberPos = strSNPos;
            this.serialNumberState = strSNState;
            this.lockState = strLockState;
        }
    }
}
