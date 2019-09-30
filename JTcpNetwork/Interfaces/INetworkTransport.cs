namespace JTcpNetwork
{
    interface INetworkTransport
    {
        NetworkConnection StartClient();
        void StartListening();
        bool Disconnect(NetworkConnection con);
        void SetOperational(bool isOperational);
        void DoUpdate();
        void Send(uint connectionId, short msgType, BaseNetworkMessage msg);
        void SendToAll(short msgType, BaseNetworkMessage msg);
        void Stop();
        void Reset();
    }
}
