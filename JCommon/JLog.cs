using JExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace JLog
{
    public class JLog
    {
        static object s_Sync = new object();

        static volatile JLog s_Instance;
        public static JLog Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new JLog();
                        }
                    }
                }
                return s_Instance;
            }
        }

        public static void Info(object msg = null, [CallerLineNumberAttribute] int lineNo = 0, [CallerMemberName] string caller = "", [CallerFilePath] string path = "")
        {
            if (msg != null)
                Instance.Log("INFO", lineNo, caller, path, msg.ToString());
            else
                Instance.Log("INFO", lineNo, caller, path, "NULL");
        }

        public static void Error(object msg = null, [CallerLineNumberAttribute] int lineNo = 0, [CallerMemberName] string caller = "", [CallerFilePath] string path = "")
        {
            if (msg != null)
                Instance.Log("ERROR", lineNo, caller, path, msg.ToString());
            else
                Instance.Log("ERROR", lineNo, caller, path, "NULL");
        }
        public static void Warning(object msg = null, [CallerLineNumberAttribute] int lineNo = 0, [CallerMemberName] string caller = "", [CallerFilePath] string path = "")
        {
            if (msg != null)
                Instance.Log("WARNING", lineNo, caller, path, msg.ToString());
            else
                Instance.Log("WARNING", lineNo, caller, path, "NULL");
        }

        public static void Debug(object msg = null, [CallerLineNumberAttribute] int lineNo = 0, [CallerMemberName] string caller = "", [CallerFilePath] string path = "")
        {
            if(msg != null)
                Instance.Log("DEBUG", lineNo, caller, path, msg.ToString());
            else
                Instance.Log("DEBUG", lineNo, caller, path, "NULL");
        }
        public static void Initialize(string path = null)
        {
            Instance.mInitialize(path);
        }

        protected bool Initiated = false;
        protected string CfgPath = "";
        protected Queue<string> writequeue = new Queue<string>();
        internal void mInitialize(string path = null)
        {
            if (path != null)
                CfgPath = path;
            else
                CfgPath = "JLog/Jlog.txt";

            CfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CfgPath);
            string p = Path.GetDirectoryName(CfgPath);
            if(!Directory.Exists(p)) {
                Directory.CreateDirectory(p);
            }
            Invoker.InvokeRepeating(Execute, 1f);
            Initiated = true;
        }

        internal void Log(string type, int lineNo, string caller, string path, string msg)
        {
            if(!Initiated)
            {
                throw new Exception("Please call JLog.Initialize first");
            }
            lock(writequeue)     
                writequeue.Enqueue(Time.UtcNowFormated+ " - [" + Path.GetFileNameWithoutExtension(path) + "][" + lineNo + "][" + caller +"] - "+ msg);
        }

        internal void Execute()
        {
            if (!Initiated) return;

            lock (writequeue)
            {
                if(writequeue.Count > 0)
                {
                    File.AppendAllLines(CfgPath, writequeue);
                    writequeue.Clear();
                }
            }
        }
    }
}
