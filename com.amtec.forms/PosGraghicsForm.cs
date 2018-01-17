using com.amtec.action;
using com.amtec.forms;
using com.amtec.model;
using com.itac.mes.imsapi.domain.container;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScreenPrinter.com.amtec.forms
{
    public partial class PosGraghicsForm : Form
    {
        MainView mv = null;
        private IMSApiSessionContextStruct sessionContext;
        private InitModel init;
        string partnumber = "";
        string processlayer = "";

        public PosGraghicsForm(MainView mv1, IMSApiSessionContextStruct _sessionContext, InitModel _init, string _partnumber, string _processlayer)
        {
            InitializeComponent();
            mv = mv1;
            sessionContext = _sessionContext;
            init = _init;
            partnumber = _partnumber;
            processlayer = _processlayer;
        }

        private void PosGraghicsForm_Load(object sender, EventArgs e)
        {
            new MouseMoveClass2(this, pictureBox1);
            this.Opacity = Convert.ToDouble(mv.config.OpacityValue) / 100;
            int x = Convert.ToInt32(mv.config.LocationXY.Split('|')[0]);
            int y = Convert.ToInt32(mv.config.LocationXY.Split('|')[1]);
            this.Location = new Point(x, y);
            getImage();
        }

        private void getImage()
        {
            GetDocumentData getDocument = new GetDocumentData(sessionContext, init, mv);
            List<DocumentEntity> listDoc2 = getDocument.GetDocumentDataByPN(partnumber);
            if (listDoc2 != null && listDoc2.Count > 0)
            {
                foreach (DocumentEntity item in listDoc2)
                {
                    string filename = item.MDA_FILE_NAME;
                    if (filename.Split('_').Length > 2)
                    {
                        if (filename.Split('_')[0] == ConvertProcessLayerToString(processlayer))
                        {
                            SetDocumentControlForDoc(long.Parse(item.MDA_DOCUMENT_ID), item.MDA_FILE_NAME);
                            break;
                        }
                    }
                }
                if (pictureBox1.Image == null)
                {
                    string defaultpartnumber = "";
                    string layer = ConvertProcessLayerToString(processlayer);
                    string[] default_part = mv.config.LAYER_DISPLAY.Split(';');
                    foreach (var part in default_part)
                    {
                        if (part.Split('_')[0] == layer)
                            defaultpartnumber = part;
                    }
                    if (defaultpartnumber != "")
                    {
                        List<DocumentEntity> listDoc = getDocument.GetDocumentDataByPN(defaultpartnumber);
                        foreach (DocumentEntity item in listDoc)
                        {
                            string filename = item.MDA_FILE_NAME;
                            if (filename.Split('_')[0] == ConvertProcessLayerToString(processlayer))
                            {
                                SetDocumentControlForDoc(long.Parse(item.MDA_DOCUMENT_ID), item.MDA_FILE_NAME);
                                break;
                            }
                        }
                    }
                    if (pictureBox1.Image == null)
                    {
                        mv.errorHandler(2, mv.Message("msg_no graphical file set in MDA"), "");
                    }
                }
            }
            else
            {
                mv.errorHandler(2, mv.Message("msg_no graphical file set in MDA"), "");
            }
        }
        private string ConvertProcessLayerToString(string str)
        {
            string iValue = "";
            switch (str)
            {
                case "0":
                    iValue = "TOP";
                    break;
                case "1":
                    iValue = "BOT";
                    break;
                default:
                    iValue = "";
                    break;
            }
            return iValue;
        }
        private void SetDocumentControlForDoc(long documentID, string fileName)
        {
            try
            {
                string path = mv.config.MDAPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string filePath = path + @"/" + fileName;
                //if (!File.Exists(filePath))
                //{
                GetDocumentData documentHandler = new GetDocumentData(sessionContext, init, mv);
                byte[] content = documentHandler.GetDocumnetContentByID(documentID);
                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
                if (content != null)
                {
                    fs.Write(content, 0, content.Length);
                }
                //}
                //pictureBox1.Image = Image.FromFile(filePath, false);
                pictureBox1.Image = Image.FromStream(fs);
                fs.Flush();
                fs.Dispose();
                fs.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Debug(ex.Message, ex);
            }
        }
    }
}
