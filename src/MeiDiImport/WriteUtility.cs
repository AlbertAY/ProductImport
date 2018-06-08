using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClProductImport
{
    public static class WriteUtility
    {
        /// <summary>
        /// 控制台打印出来，并且将答应内容记到日志文件上面去
        /// </summary>
        /// <param name="mes"></param>
        public static void WriteConsoleAndLog(object mes)
        {
            string strMsg = mes.ToString();
            Console.WriteLine(strMsg);
            Logger.GetLogger(strMsg).Debug(strMsg);
        }
        /// <summary>
        /// 保存文件到硬盘上面
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="fullPath"></param>
        public static void SaveToFile(MemoryStream ms, string fullPath)
        {
            string dirName = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dirName))//判断是否存在
            {
                Directory.CreateDirectory(dirName);//创建新路径
            }
            using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                byte[] data = ms.ToArray();
                fs.Write(data, 0, data.Length);
                fs.Flush();
                data = null;
            }
        }

    }
}
