using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClProductImport
{
    public class ConfigUtility
    {
        public static string GetConfigValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
           string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("文件路径未配置");
            }
            return value;
        }

        public static string GetConnString(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            string value = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception("数据库连接字符串未配置");
            }
            return value;
        }
    }
}
