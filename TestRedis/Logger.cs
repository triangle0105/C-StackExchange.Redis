using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRedis
{
    public class Logger
    {
        public static void RecordLog(string text)
        {
            var strFileName = System.Configuration.ConfigurationManager.AppSettings["logfilepath"];

            if (File.Exists(strFileName))
            {
                using (var filestream = new FileStream(strFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var sw = new StreamWriter(filestream, Encoding.Default);
                    sw.Write(text);
                    sw.Close();
                }

            }
        }
    }
}
