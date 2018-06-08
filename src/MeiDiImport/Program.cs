using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClProductImport.WriteUtility;

namespace ClProductImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                WriteConsoleAndLog("导入开始");
                string path = ConfigUtility.GetConfigValue("filePath");
                ImportBusiness.ImportStart(path);
            }
            catch (Exception ex)
            {
                Logger.GetLogger(ex.Message).Error(ex);
                WriteConsoleAndLog(ex.ToString());
            }
            WriteConsoleAndLog("导入完成");
            Console.Read();
        }

    }
}

