using com.amtec.configurations;
using com.amtec.model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace com.amtec.action
{
    public class GetApplicationDatas
    {
        public string scope = "";
        List<ParameterEntity> entityList = new List<ParameterEntity>();
        public GetApplicationDatas()
        { }

        public List<ParameterEntity> GetApplicationEntity()
        {
            entityList = new List<ParameterEntity>();
            string filePath = Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string _appDir = Path.GetDirectoryName(filePath);
            CommonModel commonModel = ReadIhasFileData.getInstance();
            XDocument config = XDocument.Load(_appDir + @"\ApplicationConfig.xml");
            XElement xRoot = config.Root;
            string rootName = commonModel.APPTYPE; //xRoot.Name.ToString();
            scope = commonModel.APPTYPE;
            ParameterEntity entityRoot = new ParameterEntity();
            entityRoot.PARAMETER_DESCRIPTION = rootName;
            entityRoot.PARAMETER_DISPLAYNAME = rootName;
            entityRoot.PARAMETER_DIMPATH = "{CLUSTER,APPTYPE,APPID,STATION}";
            entityRoot.PARAMETER_NAME = rootName;
            entityRoot.PARAMETER_PARENT_NAME = "Customer";
            entityRoot.PARAMETER_SCOPE = scope;
            entityRoot.PARAMETER_TYPE_NAME = "STRING";
            entityRoot.PARAMETER_VALUE = "";
            entityList.Add(entityRoot);
            var firstLevelElts = xRoot.Elements();
            if (firstLevelElts.Count() > 0)
            {
                foreach (var item in firstLevelElts)
                {
                    string subName = item.Name.ToString();
                    ParameterEntity entity = new ParameterEntity();
                    entity.PARAMETER_DESCRIPTION = subName;
                    entity.PARAMETER_DISPLAYNAME = subName;
                    entity.PARAMETER_DIMPATH = "{CLUSTER,APPTYPE,APPID,STATION}";
                    entity.PARAMETER_NAME = rootName + "." + subName;
                    entity.PARAMETER_PARENT_NAME = rootName;
                    entity.PARAMETER_SCOPE = scope;
                    entity.PARAMETER_TYPE_NAME = "STRING";
                    entity.PARAMETER_VALUE = "";
                    entityList.Add(entity);
                    GetDescendantEntity(item, rootName + "." + subName);
                }
                int count = entityList.Count();
            }
            return entityList;
        }

        private void GetDescendantEntity(XElement element, string parentName)
        {
            var secondLevels = element.Elements();
            if (secondLevels.Count() > 0)
            {
                foreach (var item in secondLevels)
                {
                    string eleName = item.Name.ToString();
                    var subLevels = item.Elements();
                    if (subLevels.Count() > 0)
                    {
                        GetDescendantEntity(item, parentName + "." + eleName);
                    }
                    else
                    {
                        ParameterEntity entity = new ParameterEntity();
                        entity.PARAMETER_DESCRIPTION = item.Name.ToString();
                        entity.PARAMETER_DIMPATH = "{CLUSTER,APPTYPE,APPID,STATION}";
                        entity.PARAMETER_DISPLAYNAME = item.Name.ToString();
                        entity.PARAMETER_NAME = parentName + "." + item.Name.ToString();
                        entity.PARAMETER_PARENT_NAME = parentName;
                        entity.PARAMETER_SCOPE = scope;
                        entity.PARAMETER_TYPE_NAME = "STRING";
                        entity.PARAMETER_VALUE = item.Value;
                        entityList.Add(entity);
                    }
                }
            }
        }
    }
}
