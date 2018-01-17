using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using com.amtec.configurations;

namespace com.amtec.action
{

    public class LanguageType
    {
        public static string English = "US";
        public static string SimplifiedChinese = "ZHS";
        public static string TraditionalChinese = "ZHT";
    }

    /// <summary>
    /// language 的摘要说明。
    /// </summary>
    [Serializable]
    public class LanguageWord
    {
        public LanguageWord()
        {
            ControlID = "";
            ControlENText = "";
            ControlCHSText = "";
            ControlCHTText = "";
        }

        public string ControlID;

        public string ControlENText;

        public string ControlCHSText;

        public string ControlCHTText;
    }

    /// <summary>
    /// 多语言
    /// </summary>
    public class LanguagePack
    {
        private static Dictionary<string, LanguageWord> dicLangaugeList = null;

        public static string GetLanguageWord(string controlID)
        {
            if (dicLangaugeList == null)
            {
                LanguagePack pack = new LanguagePack();
                pack.Init();
            }
            string strText = "";
            
            if (dicLangaugeList.ContainsKey(controlID.ToUpper()) == true)
            {
                LanguageWord word = dicLangaugeList[controlID.ToUpper()];
                if (SystemVariable.CurrentLangaugeCode == LanguageType.SimplifiedChinese)
                    strText = word.ControlCHSText;
                else if (SystemVariable.CurrentLangaugeCode == LanguageType.English)
                    strText = word.ControlENText;
                else if (SystemVariable.CurrentLangaugeCode == LanguageType.TraditionalChinese)
                    strText = word.ControlCHTText;
                else
                    strText = word.ControlCHSText;
            }
            return strText;
        }

        private void Init()
        {
            dicLangaugeList = new Dictionary<string, LanguageWord>();
            try
            {
                string strFileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "language.mdb");
                string strConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Password=;Data Source=" + strFileName + ";Persist Security Info=True";
                OleDbConnection conn = new OleDbConnection(strConnectionString);
                conn.Open();
                OleDbDataAdapter da = new OleDbDataAdapter("select * from Control", conn);
                DataTable tb = new DataTable();
                da.Fill(tb);
                conn.Close();

                for (int i = 0; i < tb.Rows.Count; i++)
                {
                    LanguageWord word = new LanguageWord();
                    word.ControlID = tb.Rows[i]["ControlID"].ToString();
                    word.ControlENText = tb.Rows[i]["ControlENText"].ToString();
                    word.ControlCHSText = tb.Rows[i]["ControlCHSText"].ToString();
                    word.ControlCHTText = tb.Rows[i]["ControlCHTText"].ToString();
                    dicLangaugeList.Add(word.ControlID.ToUpper(), word);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
