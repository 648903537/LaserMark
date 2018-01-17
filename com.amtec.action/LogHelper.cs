using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using log4net;
using System.Xml.Linq;
using com.amtec.model;
using com.amtec.configurations;

namespace com.amtec.action
{
    public sealed class LogHelper
    {
        public static readonly log4net.ILog localLog = null;
        static LogHelper()
        {
            XDocument config = XDocument.Load("ApplicationConfig.xml");
            CommonModel commonModel = ReadIhasFileData.getInstance();
            string stationNumber = commonModel.Station;
            string filePath = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string _appDir = Path.GetDirectoryName(filePath);
            log4net.GlobalContext.Properties["LogPath"] = @"C:\DMS_Log";//_appDir + @"\log"; 
            log4net.GlobalContext.Properties["LogName"] = stationNumber + "_1.log";
            log4net.Config.XmlConfigurator.Configure();
            localLog = log4net.LogManager.GetLogger("loggerLog4Net");
            Info("Application start...");
            Info("Version :" + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Info(config);
        }

        #region 创建logger
        static ILog GetLog()
        {
            return localLog;
        }
        #endregion

        #region Fatal

        public static void Fatal(object msg)
        {
            try
            { GetLog().Fatal(msg); }
            catch (Exception)
            { }
        }
        public static void Fatal(object msg, Exception exception)
        {
            try
            { GetLog().Fatal(msg, exception); }
            catch (Exception)
            { }
        }
        #endregion

        #region Error

        public static void Error(object msg)
        {
            try
            { GetLog().Error(msg); }
            catch (Exception)
            { }
        }
        public static void Error(object msg, Exception exception)
        {
            try
            { GetLog().Error(msg, exception); }
            catch (Exception)
            { }
        }
        #endregion

        #region Warn

        public static void Warn(object msg)
        {
            try
            { GetLog().Warn(msg); }
            catch (Exception)
            { }
        }
        public static void Warn(object msg, Exception exception)
        {
            try
            { GetLog().Warn(msg, exception); }
            catch (Exception)
            { }
        }
        #endregion

        #region Info

        public static void Info(object msg)
        {
            try
            { GetLog().Info(msg); }
            catch
            { }
        }
        public static void Info(object msg, Exception exception)
        {
            try
            { GetLog().Info(msg, exception); }
            catch
            { }
        }
        #endregion

        #region Debug

        public static void Debug(object msg)
        {
            try
            { GetLog().Debug(msg); }
            catch (Exception)
            { }
        }
        public static void Debug(object msg, Exception exception)
        {
            try
            { GetLog().Debug(msg, exception); }
            catch (Exception)
            { }
        }

        #endregion
    }
}
