using JCommon;
using System.Collections.Generic;

namespace JTcpNetwork
{
    /// <summary>
    /// Thread safe class TCP Client
    /// </summary>
    public class NetworkClient
    {
        public static bool hasStarted = false;
        static object s_Sync = new object();
        static volatile NetworkClient s_Instance;
        public static NetworkClient Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Sync)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new NetworkClient();
                        }
                    }
                }
                return s_Instance;
            }
        }
        public bool ClientConnected = false;
        public static float Ticks = 10;
        public static bool Connected => Instance.ClientConnected;
        internal Queue<NetworkMessage> recieved = new Queue<NetworkMessage>();
        public NetworkConnection connection;
#if UNITY_5_3_OR_NEWER
        readonly bool AutoUpdate = false;
#else
        readonly bool AutoUpdate = true;
#endif
        private INetworkTransport m_activeTransport = null;
        private NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();

        #region Constructor
         NetworkClient()
        {
            if(AutoUpdate)
                Invoker.InvokeRepeating(UpdateRecieve, (1f / Ticks));
        }
        #endregion

        #region EXPOSED
        public static void Start(string ip, int port = -1)
        {
            Instance.StartClient(ip, port);
        }

        public static void Start(string ip)
        {
            Instance.StartClient(ip);
        }

        public static void Start(int port = -1)
        {
            Instance.StartClient(null, port);
        }

        public static void Start()
        {
            Instance.StartClient();
        }

        public static void Send(short msgType, BaseNetworkMessage msg, bool diff = false)
        {
            Instance.InternalSend(msgType, msg);
        }

        public static void PushMessage(NetworkMessage msg)
        {
            Instance.AddMsg(msg);
        }

        public static void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            Instance.m_MessageHandlers.RegisterHandlerSafe(msgType, handler);
        }

        public static void CleanHandlers()
        {
            Instance.ClearMessageHandlers();
        }

        public static void RemoveHandler(short msgType)
        {
            Instance.RemoveHandlerInternal(msgType);
        }

        public static void DisableReconnect()
        {
            Instance.SetOperational(false);
        }

        public static void Stop()
        {
            Instance.StopConnecting();
        }

        public static void Update()
        {
            Instance.UpdateRecieve();
        }

        public static void Disconnect(NetworkConnection con)
        {
            Instance.Discconnectx(con);
        }

        public static void ResetForReconnect()
        {
            Instance.Reset();
        }

        #endregion

        #region INTERNAL
        internal void Reset()
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.Reset();
            }
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

        internal void Discconnectx(NetworkConnection con)
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.Disconnect(con);
                if (NetConfig.logFilter >= LogFilter.Log && con != null) Log.Debug("Disconnected :" + con.connectionId);
            }
        }

        internal bool StopConnecting()
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.Stop();
                return true;
            }

            return false;
        }

        internal void AddMsg(NetworkMessage msg)
        {
            lock (recieved)
                recieved.Enqueue(msg);
        }

        internal void StartClient(string ip = null, int port = -1)
        {
            if (ip != null)
                NetConfig.IP = ip;

            if (port != -1)
                NetConfig.Port = port;
            StartClient();
        }

        internal void StartClient()
        {
            INIT();

            if (m_activeTransport == null)
                m_activeTransport = new BaseClientTransport(m_MessageHandlers);

            m_activeTransport.SetOperational(true);
            m_activeTransport.StartClient();
        }

        internal void RemoveHandlerInternal(short msgType)
        {
            m_MessageHandlers.UnregisterHandler(msgType);
        }

        internal void ClearMessageHandlers()
        {
            m_MessageHandlers.ClearMessageHandlers();
        }

        internal void RegisterInternalHandlers()
        {
            m_MessageHandlers.RegisterHandler(InternalMessages.HeandShake_Client, HandleClientHandShake);
        }

        internal void HandleClientHandShake(NetworkMessage netMsg)
        {
            HandShakeMsg packet = netMsg.ReadMessage<HandShakeMsg>();
            if (packet != null)
            {
                if (packet.OP == 0) //VER VERSION
                {
                    netMsg.conn.connectionId = packet.Version;
                    netMsg.conn.stage = PerStage.Connecting;
                    HandShakeMsg p = new HandShakeMsg
                    {
                        Version = (uint)NetConfig.Key,
                        OP = 1
                    };
                    netMsg.conn.Send(InternalMessages.HeandShake_Server, p);

                }
                else if (packet.OP == 1)
                {
                    if (packet.Version == NetConfig.Key)
                    {
                        netMsg.conn.stage = PerStage.Connected;
                        HandShakeMsg p = new HandShakeMsg
                        {
                            Version = (uint)NetConfig.Key,
                            OP = 2
                        };
                        netMsg.conn.Send(InternalMessages.HeandShake_Server, p);
                        ClientConnected = true;
                        connection = netMsg.conn;
                        PushMessage(new NetworkMessage
                        {
                            msgType = InternalMessages.CONNECTED,
                            conn = netMsg.conn,
                            reader = new NetworkReader()
                        });
                    }
                }
            }
        }

        internal void INIT()
        {
            if (!hasStarted)
            {
                RegisterInternalHandlers();
                hasStarted = true;
            }
        }

        internal void SetOperational(bool isOperational)
        {
            if (m_activeTransport != null)
            {
                m_activeTransport.SetOperational(false);
            }
        }

        internal bool InvokeHandler(NetworkMessage netMsg)
        {
            m_MessageHandlers.GetHandler(netMsg.msgType)?.Invoke(netMsg);
            return true;
        }

        float lastTick = 0;
        internal void UpdateRecieve()
        {
            if (Time.time > lastTick)
            {
                lastTick = Time.time + 0.1f;
                lock (recieved)
                {
                    if (recieved.Count > 0)
                        InvokeHandler(recieved.Dequeue());
                }
            }

            if (m_activeTransport != null)
                m_activeTransport.DoUpdate();
        }
        #endregion
    }
}
