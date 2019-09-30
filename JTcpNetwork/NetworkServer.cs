using JCommon;
using System.Collections.Generic;

namespace JTcpNetwork
{
    /// <summary>
    /// Thread safe TCP Server
    /// </summary>
    public class NetworkServer
    {
        internal INetworkTransport m_activeTransport = null;
        internal NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();
        internal bool hasStarted = false;
        static object s_Sync = new object();
        static volatile NetworkServer s_Instance;
        public static NetworkServer Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new NetworkServer();
                        }
                    }
                }
                return s_Instance;
            }
        }

        public static void Start(string ip = null, int port = -1)
        {
            if(ip != null)
                NetConfig.IP = ip;

            if (port != -1)
                NetConfig.Port = port;

            Instance.StartServer();
        }
        public static void Start(string ip = null)
        {
            if (ip != null)
                NetConfig.IP = ip;

            Instance.StartServer();
        }

        public static void Start(int port = -1)
        {

            if (port != -1)
                NetConfig.Port = port;

            Instance.StartServer();
        }

        public static void Disconnect(NetworkConnection con)
        {
            Instance.DisconnectClient(con);
        }

        public static void SendToAll(short msgType, BaseNetworkMessage msg)
        {
            Instance.InternalSendToAll(msgType, msg);
        }

        public static void Send(short msgType, BaseNetworkMessage msg)
        {
            Instance.InternalSend(msgType, msg);
        }

        public static void Send(uint connectionId, short msgType, BaseNetworkMessage msg)
        {
            Instance.InternalSend(connectionId, msgType, msg);

        }

        public static void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            Instance.RegisterHandlerSafe(msgType, handler);
        }

        public static void PushMessage(NetworkMessage msg)
        {
            Instance.InvokeHandler(msg);
        }

        #region INTERNAL
        internal void INIT()
        {
            if (!hasStarted)
            {
                RegisterInternalHandlers();
                hasStarted = true;
            }
        }

        internal void StartServer(string ip, int port = -1)
        {
            NetConfig.IP = ip;
            if (port != -1)
                NetConfig.Port = port;
            StartServer();
        }

        internal void StartServer()
        {
            INIT();
            if (m_activeTransport == null)
                m_activeTransport = new BaseServerTransport(m_MessageHandlers);

            m_activeTransport.SetOperational(true);
            m_activeTransport.StartListening();
        }

        internal void DisconnectClient(NetworkConnection con)
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.Disconnect(con);
                if (NetConfig.logFilter >= LogFilter.Log && con != null) Log.Debug("Disconnected :" + con.connectionId);
            }
        }

        internal void Connected(NetworkConnection con)
        {
            InvokeHandler(new NetworkMessage()
            {
                msgType = InternalMessages.CONNECTED,
                conn = con,
                reader = new NetworkReader()
            });
        }

        internal void RegisterHandlerSafe(short msgType, NetworkMessageDelegate handler)
        {
            m_MessageHandlers.RegisterHandlerSafe(msgType, handler);
        }

        internal void AddConnection(NetworkConnection con)
        {
            Connected(con);
            if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Added Connection To Pool.");
        }

        internal void RegisterHandlerInternal(short msgType, NetworkMessageDelegate handler)
        {
            m_MessageHandlers.RegisterHandler(msgType, handler);
        }

        internal void RegisterInternalHandlers()
        {
            RegisterHandlerInternal(InternalMessages.HeandShake_Server, HandleServerHandShake);
        }

        internal void HandleServerHandShake(NetworkMessage netMsg)
        {
            HandShakeMsg packet = netMsg.ReadMessage<HandShakeMsg>();
            if (packet != null)
            {
                if (packet.OP == 0) //VER VERSION
                {
                    if (packet.Version == NetConfig.Version)
                    {
                        netMsg.conn.stage = PerStage.Verifying;
                        HandShakeMsg p = new HandShakeMsg
                        {
                            Version = netMsg.conn.connectionId,
                            OP = 0
                        };
                        netMsg.conn.Send(InternalMessages.HeandShake_Client, p);
                    }
                    else
                    {
                        netMsg.conn.Disconnect();
                    }
                }
                else if (packet.OP == 1)
                {
                    if (packet.Version == NetConfig.Key)
                    {
                        netMsg.conn.stage = PerStage.Connecting;
                        HandShakeMsg p = new HandShakeMsg
                        {
                            Version = (uint)NetConfig.Key,
                            OP = 1
                        };
                        netMsg.conn.Send(InternalMessages.HeandShake_Client, p);
                    }
                    else
                    {
                        netMsg.conn.Disconnect();
                    }
                }
                else if (packet.OP == 2)
                {
                    if (packet.Version == NetConfig.Key)
                    {
                        netMsg.conn.stage = PerStage.Connected;
                        if (NetConfig.logFilter >= LogFilter.Log) { Log.Debug("Connected:" + netMsg.conn.connectionId); }
                        AddConnection(netMsg.conn);
                    }
                    else
                    {
                        netMsg.conn.Disconnect();
                    }
                }
                else
                {
                    netMsg.conn.Disconnect();
                }
            }
        }

        internal void InternalSendToAll(short msgType, BaseNetworkMessage msg)
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.SendToAll(msgType, msg);
                return;
            }
            if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("Failed to send message to connection ID '" + 0 + ", not found in connection list"); }
        }

        internal void InternalSend(short msgType, BaseNetworkMessage msg)
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.Send(0, msgType, msg);
                return;
            }
            if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("Failed to send message to connection ID '" + 0 + ", not found in connection list"); }
        }

        internal void InternalSend(uint connectionId, short msgType, BaseNetworkMessage msg)
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.Send(connectionId, msgType, msg);
                return;
            }
            if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("Failed to send message to connection ID '" + connectionId + ", not found in connection list"); }
        }

        internal bool InvokeHandler(NetworkMessage netMsg)
        {
            m_MessageHandlers.GetHandler(netMsg.msgType)?.Invoke(netMsg);
            return true;
        }

        internal void Disable()
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.SetOperational(false);
            }
        }

        #endregion
    }
}
