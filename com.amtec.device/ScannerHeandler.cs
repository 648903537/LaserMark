using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using com.amtec.model;
using com.amtec.forms;

namespace com.amtec.device
{
    public class ScannerHeandler
    {

        private SerialPort serialPort;
        private InitModel init;
        private MainView view;

        public void DoWork()
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(this.DataRecivedHeandler);
            serialPort.Open();
        }

        public ScannerHeandler(InitModel init, MainView view)
        {
            this.init = init;
            this.view = view;
            serialPort = new SerialPort();
            serialPort.PortName = init.configHandler.SerialPort;
            serialPort.BaudRate = int.Parse(init.configHandler.BaudRate);
            serialPort.Parity = (Parity)int.Parse(init.configHandler.Parity);
            serialPort.StopBits = (StopBits)1;
            serialPort.Handshake = Handshake.None;
            serialPort.DataBits = int.Parse(init.configHandler.DataBits);
            serialPort.NewLine = "\r";

        }

        public SerialPort heandler()
        {
            return serialPort;
        }

        public void endCommand()
        {
            /* char[] charArray;
             String text = init.configHandler.EndCommand;
             String tmpString = text.Trim();
             tmpString = tmpString.Replace("ESC", "*");
             charArray = tmpString.ToCharArray();

             for (int i = 0; i < charArray.Length; i++)
             {
                 if (charArray[i].Equals((char)42))
                 {
                     charArray[i] = (char)27;
                 }
             }

             try
             {
                 serialPort.Write(charArray, 0, charArray.Length);
                 Console.WriteLine("SEND END COMMAND " + charArray);
             }
             catch
             {

             }*/
        }

        public void sendHigh()
        {

            /*char[] charArray;
            String text = init.configHandler.High;
            String tmpString = text.Trim();
            tmpString = tmpString.Replace("ESC", "*");
            charArray = tmpString.ToCharArray();

            for (int i = 0; i < charArray.Length; i++)
            {
                if (charArray[i].Equals((char)42))
                {
                    charArray[i] = (char)27;
                }
            }

            try
            {
                serialPort.Write(charArray, 0, charArray.Length);
                view.errorHandler(5, "SEND HIGH", "SEND HIGH");
            }
            catch
            {
                view.errorHandler(2, "SEND HIGH ERROR", "SEND HIGH ERROR");
            }*/
        }

        public void sendLow()
        {
            /*char[] charArray;
            String text = init.configHandler.Low;
            String tmpString = text.Trim();
            tmpString = tmpString.Replace("ESC", "*");
            charArray = tmpString.ToCharArray();

            for (int i = 0; i < charArray.Length; i++)
            {
                if (charArray[i].Equals((char)42))
                {
                    charArray[i] = (char)27;
                }
            }

            try
            {
                serialPort.Write(charArray, 0, charArray.Length);
                view.errorHandler(0, "SEND LOW", "SEND LOW");
            }
            catch
            {
                view.errorHandler(2, "SEND LOW ERROR", "SEND LOW ERROR");
            }*/
        }

        public void DataRecivedHeandler(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;
            String indata = sp.ReadLine();
            char[] initCheck = indata.ToCharArray();
            //initModel.scannerHandler.sendHigh();
            //initModel.scannerHandler.sendLow();

            if (!indata.StartsWith("T") || !!indata.StartsWith("\nT"))
            {
                serialPort.DiscardInBuffer();
                //return;
            }
            else
            {

                if (indata.Length == 2)
                {
                    this.sendLow();
                    Thread.Sleep(500);
                    this.endCommand();
                    //initModel.scannerHandler.heandler().DiscardInBuffer();
                    return;
                }
                else
                {
                    this.sendLow();
                    Thread.Sleep(500);
                    this.endCommand();
                }

                Console.WriteLine("");
                //view.SetContainerIDText(indata);
                System.Console.Write(indata);

                //Match match = Regex.Match(fieldSerialNumber.Text, initModel.configHandler.MSSExtractPattern);

                //ScanProcess scanProcess = new ScanProcess(sessionContext.getSessionContext(), initModel, this);
                //scanProcess.ScanProcessResultCall(match.ToString());
                //initModel.scannerHandler.heandler().DiscardInBuffer();
            }
        }

    }
}
