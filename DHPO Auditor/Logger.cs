using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHPO_Auditor
{
    class Logger
    {
        public static string baseDir = System.Configuration.ConfigurationManager.AppSettings.Get("basedir");

        public static void Info(string data)
        {
            Console.WriteLine(data);
            using (StreamWriter writer = File.AppendText(baseDir + @"\Infolog.csv"))
            {
                writer.Write(System.DateTime.Now + " : " + data + "\n");
            }
        }
        public static void Info(Exception data)
        {
            Console.WriteLine(data);
            using (StreamWriter writer = File.AppendText(baseDir + @"\Infolog.csv"))
            {
                writer.Write(System.DateTime.Now + " : " + data + "\n");
            }
        }
    }
}
