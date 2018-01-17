using com.amtec.action;
using com.amtec.model;
using System;
using System.IO;
using System.Text;

namespace com.amtec.configurations
{
    public class ReadIhasFileData
    {
        private static CommonModel instance;
        public static CommonModel getInstance()
        {
            if (instance == null)
            {
                instance = ReadIhasFile();
            }
            return instance;
        }

        public static CommonModel ReadIhasFile()
        {
            string ihasfilename = "ihas.properties";
            StreamReader sr = null;
            CommonModel commonModel = new CommonModel();
            string content = "";
            StringBuilder sb = new StringBuilder();
            try
            {
                sr = new StreamReader(ihasfilename);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    sb.AppendLine(line);
                    string[] lines = line.Split('=');

                    if (lines[0] == "Station")
                    {
                        commonModel.Station = lines[1];
                    }
                    else if (lines[0] == "Client")
                    {
                        commonModel.Client = lines[1];
                    }
                    else if (lines[0] == "RegisterType")
                    {
                        commonModel.RegisterType = lines[1];
                    }
                    else if (lines[0] == "APPTYPE")
                    {
                        commonModel.APPTYPE = lines[1];
                    }
                    else if (lines[0] == "APPID")
                    {
                        commonModel.APPID = lines[1];
                    }
                    else if (lines[0] == "Cluster")
                    {
                        commonModel.Cluster = lines[1];
                    }
                    else if (lines[0] == "UpdateConfig")
                    {
                        commonModel.UpdateConfig = lines[1];
                    }
                }
                sr.Close();
                //change the "UpdateConfig" value
                try
                {
                    if (commonModel.UpdateConfig == "Y")
                    {
                        using (StreamWriter sw = new StreamWriter(ihasfilename, false))
                        {
                            content = sb.ToString();
                            content = content.Replace("UpdateConfig=" + commonModel.UpdateConfig, "UpdateConfig=N");
                            sw.Write(content);
                        }
                    }     
                }
                catch (Exception e)
                {
                    LogHelper.Error(e.Message, e);
                }

                return commonModel;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Read ihas.properties.", ex);
                return null;
            }
        }
    }
}
