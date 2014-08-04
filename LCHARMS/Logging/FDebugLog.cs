using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace LCHARMS.Logging
{
    public static class FDebugLog
    {
        private static int pid = 0;
        public static bool Enabled = true;
        public static string FileName = "";
		public static Object SyncRoot = new Object();
        private static StreamWriter FileWriter = null;
        public static void WriteLog(string data)
        {
			lock (SyncRoot)
            {
                if (FileWriter == null)
                {
                    CreateFileWriter();
                }
                FileWriter.WriteLine(DateTime.Now.ToString() + ": " + data);
                FileWriter.Close();
                FileWriter = null;
            }
        }
        private static void CreateFileWriter()
        {
            if (FileName == "")
            {
                pid = Process.GetCurrentProcess().Id;
				if (!Directory.Exists("./logs/"))
					Directory.CreateDirectory("./logs/");
                FileName = @"./logs/" + DateTime.Now.Ticks.ToString() + "-" + pid.ToString() + "-LCHARMS-Debug.txt";
            }
            FileWriter = new System.IO.StreamWriter(FileName, true);
        }
    }
}
