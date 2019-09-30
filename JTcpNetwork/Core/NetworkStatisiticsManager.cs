using JCommon;

namespace JTcpNetwork
{
    public static class NetworkStatisiticsManager
    {
        public static void Remove(NetworkConnection con)
        {
            if (!NetConfig.UseStatistics)
                return;

            Log.Info(con.ToString());
        }
    }
}
