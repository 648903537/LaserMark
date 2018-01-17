using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace com.amtec.action
{
    /// <summary>
    /// MutiLanguage 的摘要说明。
    /// </summary>
    public class MutiLanguages
    {
        public MutiLanguages()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public static string ParserMessage(string originMsg)
        {
            if (originMsg == string.Empty)
            {
                return string.Empty;
            }
            string errMsg = originMsg;
            Regex regex = new Regex(@"\$([A-Za-z0-9_]+)");
            string word = string.Empty;
            foreach (Match match in regex.Matches(errMsg))
            {
                word = LanguagePack.GetLanguageWord(match.Value);

                if (word != null && word != string.Empty)
                {
                    errMsg = errMsg.Replace(match.Value, word);
                }
            }
            return errMsg;
        }

        public static string ParserString(string origin)
        {
            if (origin == string.Empty)
            {
                return string.Empty;
            }
            return LanguagePack.GetLanguageWord(origin);
        }

        public void InitLangauge(Control ctl)
        {
            string strMsgId = "$" + ctl.Name;
            // 设置单个控件的显示值      
            if (ctl is Label)
            {
                ((Label)ctl).Text = GetString(strMsgId, ctl);
            }
            else if (ctl is Button)
            {
                ((Button)ctl).Text = GetString(strMsgId, ctl);
            }
            else if (ctl is CheckBox)
            {
                ((CheckBox)ctl).Text = GetString(strMsgId, ctl);
            }
            else if (ctl is RadioButton)
            {
                ((RadioButton)ctl).Text = GetString(strMsgId, ctl);
            }
            else if (ctl is DataGridView)
            {
                DataGridView grid = (DataGridView)ctl;
                foreach (DataGridViewColumn item in grid.Columns)
                {
                    string strID = "$grid_" + item.Name;
                    item.HeaderText = GetString(strID, null);
                }
            }
            else if (ctl is GroupBox)
            {
                ((GroupBox)ctl).Text = GetString(strMsgId, ctl);
                // 处理下级控件
                for (int i = 0; i < ctl.Controls.Count; i++)
                {
                    InitLangauge(ctl.Controls[i]);
                }
            }
            else if(ctl is ContextMenuStrip)
            {
                ContextMenuStrip strip = (ContextMenuStrip)ctl;
                foreach(ToolStripItem item in strip.Items)
                {
                    item.Text = GetString("$" + item.Name, null);
                }
            }          
            else
            {
                // 处理下级控件
                for (int i = 0; i < ctl.Controls.Count; i++)
                {
                    InitLangauge(ctl.Controls[i]);
                }
            }
        }

        public void InitLangaugeForTabControl(TabControl ctl)
        {
            foreach (TabPage item in ctl.TabPages)
            {
                item.Text = GetString("$" + item.Name, null);
            }
        }

        private string GetString(string messageId, Control ctl)
        {
            string strMsg = MutiLanguages.ParserString(messageId);
            if (strMsg == "")
            {
                if (ctl == null)
                    return strMsg;

                string strText = "";
                System.Reflection.PropertyInfo property = ctl.GetType().GetProperty("Caption");
                if (property != null)
                    strText = property.GetValue(ctl, null).ToString();
                else
                    strText = ctl.Text;

                strMsg = strText;
            }
            return strMsg;
        }
    }
}
