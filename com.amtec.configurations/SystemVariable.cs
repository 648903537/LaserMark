using com.amtec.action;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.amtec.configurations
{
    public class SystemVariable
    {
        private static string strCurrentLangaugeCode = LanguageType.SimplifiedChinese;
        /// <summary>
        /// 用户登录当前选择语言代码：US、CHS、CHT
        /// </summary>
        public static string CurrentLangaugeCode
        {
            get { return strCurrentLangaugeCode; }
            set { strCurrentLangaugeCode = value; }
        }
    }
}
