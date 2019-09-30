using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JExtensions
{
    public class CDManager
    {
        private static object s_Sync = new object();
        static volatile CDManager s_Instance;
        public static CDManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new CDManager();
                        }
                    }
                }
                return s_Instance;
            }
        }

        Dictionary<string, float> AllCds = new Dictionary<string, float>();
        public static bool IsCD(float CoolDownSeconds, [CallerMemberName] string callerName = "")
        {
            return Instance.IsCoolDown(CoolDownSeconds, callerName);
        }

        public bool IsCoolDown(float CoolDownSeconds, [CallerMemberName] string callerName = "")
        {
            bool ret = false;
            lock (AllCds)
            {
                if (AllCds.TryGetValue(callerName, out float value))
                {
                    if (Time.time > value)
                    {
                        ret = false;
                        AllCds[callerName] = Time.time + CoolDownSeconds;
                    }
                    else
                    {
                        ret = true;
                    }
                }
                else
                {
                    ret = false;
                    AllCds[callerName] = Time.time + CoolDownSeconds;
                }
            }
            return ret;
        }
    }
}
