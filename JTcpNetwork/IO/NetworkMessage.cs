namespace JTcpNetwork
{
    public delegate void NetworkMessageDelegate(NetworkMessage netMsg);

    public class NetworkMessage
    {
        public const int MaxMessageSize = (64 * 1024) - 1;

        public short msgType;
        public NetworkConnection conn;
        public NetworkReader reader;

        public static string Dump(byte[] payload, int sz)
        {
            string outStr = "[";
            for (int i = 0; i < sz; i++)
            {
                outStr += (payload[i] + " ");
            }
            outStr += "]";
            return outStr;
        }

        public TMsg ReadMessage<TMsg>() where TMsg : BaseNetworkMessage, new()
        {
            var msg = new TMsg();
            msg.Deserialize(reader);
            return msg;
        }

        public void ReadMessage<TMsg>(TMsg msg) where TMsg : BaseNetworkMessage
        {
            msg.Deserialize(reader);
        }
    }
}
