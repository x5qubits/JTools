namespace JTcpNetwork
{
    public static class NetConfig
    {
        public static LogFilter logFilter = LogFilter.Log;
        public static int Port = 10001;
        public static string IP = "127.0.0.1";   
        public static bool UseStatistics = true;
        public static short Key = 1985;
        public static uint Version = 1;
        public static int ReconnectAttempts = 3;
        public static int ReconnectTimeOut = 5;
    }
}
