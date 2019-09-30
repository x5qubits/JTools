using JCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace JTcpNetwork
{
    internal class BaseServerTransport : INetworkTransport
    {
        protected Socket _receiveSocket;
        private Dictionary<uint, NetworkConnection> m_Connections = new Dictionary<uint, NetworkConnection>();
        protected NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();
        private static int count = 0;

        public static uint IncrementCount()
        {
            int newValue = Interlocked.Increment(ref count);
            return unchecked ((uint)newValue);
        }

        public BaseServerTransport(NetworkMessageHandlers m_MessageHandlersx)
        {
            m_MessageHandlers.ClearMessageHandlers();
            m_MessageHandlers = m_MessageHandlersx;
        }

        public void StartListening()
        {
            IPAddress ipAddress = IPAddress.Parse(NetConfig.IP);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, NetConfig.Port);
            _receiveSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _receiveSocket.Bind(localEndPoint);
                _receiveSocket.Listen(100);
                if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Started to listen :" + ipAddress.ToString() + " Port:" + NetConfig.Port + " Protocol Version:" + NetConfig.Version);
                else Log.Debug("Server Started IP[" + ipAddress.ToString() + ":" + NetConfig.Port + "] Version:[" + NetConfig.Version+"]");
                _receiveSocket.BeginAccept(new AsyncCallback(AcceptCallback), _receiveSocket);
            }
            catch (Exception e)
            {
                if (NetConfig.logFilter >= LogFilter.Developer) Log.Error("Excepiton:" + e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            uint connectionId = IncrementCount();
            NetworkConnection per = new NetworkConnection
            {
                connectionId = connectionId
            };
            per.Init(false);
            per.SetHandlers(m_MessageHandlers);
            per.StartReceiving(handler);
            lock (m_Connections)
            {
                if (!m_Connections.ContainsKey(connectionId))
                    m_Connections.Add(connectionId, per);
            }
            // Signal the main thread to continue.  
            _receiveSocket.BeginAccept(new AsyncCallback(AcceptCallback), _receiveSocket);
        }

        public void Send(uint connectionId, short msgType, BaseNetworkMessage msg)
        {
            if (m_Connections.TryGetValue(connectionId, out NetworkConnection conection))
            {
                if (conection != null)
                {
                    conection.Send(msgType, msg);
                    return;
                }
            }
            if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("Failed to send message to connection ID '" + connectionId + ", not found in connection list"); }
        }

        public void SendToAll(short msgType, BaseNetworkMessage msg)
        {
            try
            {
                NetworkConnection[] connections = m_Connections.Values.ToArray();
                for(int i =0; i < connections.Length; i++)
                {
                    if (connections[i] != null && connections[i].ConnectionReady())
                    {
                        connections[i].Send(msgType, msg);
                    }
                }
            }
            catch (Exception e)
            {
                if (NetConfig.logFilter >= LogFilter.Error) Log.Error("Exception:" + e.ToString());
            }
        }

        public bool Disconnect(NetworkConnection con)
        {
            if (con != null)
            {
                uint conId = con.connectionId;
                NetworkServer.PushMessage(new NetworkMessage
                {
                    msgType = InternalMessages.DISCONNECT,
                    conn = con,
                    reader = new NetworkReader()
                });
                NetworkStatisiticsManager.Remove(con);
                lock (m_Connections)
                {
                    if(m_Connections.ContainsKey(conId))
                        m_Connections.Remove(conId);
                }
                con.Dispose();
            }
            return true;
        }

        public void SetOperational(bool count)
        {

        }

        public void DoUpdate()
        {

        }

        public NetworkConnection StartClient()
        {
            return null;
        }

        public void Stop()
        {
            NetworkConnection[] cons = m_Connections.Values.ToArray();
            for (int i = 0; i < cons.Length; i++)
            {
                if (cons[i] != null)
                {
                    Disconnect(cons[i]);
                }
            }

            m_Connections.Clear();
        }

        public void Reset()
        {

        }
    }
}
