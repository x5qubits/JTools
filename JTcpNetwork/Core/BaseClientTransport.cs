using JCommon;
using System;
using System.Net;
using System.Net.Sockets;
using JCommon.Extensions;

namespace JTcpNetwork
{
    internal class BaseClientTransport : INetworkTransport
    {
        internal IPEndPoint remoteEP;
        internal int RecconectTry = 3;
        internal int m_RecconectTry = 0;
        internal int Port = 0;
        internal string IP = "127.0.0.1";
        internal bool Connecting = false;
        internal bool Connected = false;
        internal bool PermaDisconnected = false;
        internal float LastTryToConnect = 0;
        internal NetworkConnection connection;
        internal NetworkMessageHandlers m_MessageHandlers = new NetworkMessageHandlers();

        public BaseClientTransport(NetworkMessageHandlers m_MessageHandlersx)
        {
            RecconectTry = NetConfig.ReconnectAttempts;
            m_MessageHandlers.ClearMessageHandlers();
            m_MessageHandlers = m_MessageHandlersx;
        }

        public BaseClientTransport()
        {
            RecconectTry = NetConfig.ReconnectAttempts;
        }

        public NetworkConnection StartClient()
        {
            if (NetConfig.logFilter >= LogFilter.Log) Log.Debug("Created Client Version:" + NetConfig.Version);
            Connecting = true;
            IP = NetConfig.IP;
            Port = NetConfig.Port;
            StartConnect();
            Connected = false;
            return null;
        }

        public void StartConnect()
        {
            IPAddress ipAddress = IPAddress.Parse(IP);
            remoteEP = new IPEndPoint(ipAddress, Port);

            if (connection != null && !connection.m_Disposed)
            {
                connection.Dispose();
                connection = null;
            }

            connection = new NetworkConnection();
            connection.Init(true);

            if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Connecting to Server reconnect attmpt:" + m_RecconectTry);
            connection.m_socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = false
            };
            connection.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 1048576);
            connection.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 1048576);
            connection.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, 1);
            connection.m_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 0);
            connection.m_socket.BeginConnect(remoteEP, ConnectCallback, connection.m_socket);
            Connecting = true;
            PermaDisconnected = false;

        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            try
            {
                client.EndConnect(ar);
            }
            catch (SocketException x)
            {
                if (connection != null)
                    connection.Disconnect();
                if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Excepiton:" + x.GetErrorCode());
                Connecting = false;
                return;
            }

            catch (Exception e)
            {
                if (connection != null)
                    connection.Disconnect();
                if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Excepiton:" + e.ToString());
                Connecting = false;
                return;
            }
            try
            {

                connection.SetHandlers(m_MessageHandlers);
                connection.StartReceiving(client);
                HandShakeMsg packet = new HandShakeMsg
                {
                    Version = NetConfig.Version,
                    OP = 0,
                };
                connection.Send(InternalMessages.HeandShake_Server, packet);
                Connecting = false;
                Connected = true;
                m_RecconectTry = 0;
            }
            catch (Exception e)
            {
                if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Excepiton:" + e.ToString());
                Connecting = false;
            }
        }

        public bool Disconnect(NetworkConnection con)
        {
            NetworkClient.Instance.ClientConnected = false;
            if (con != null)
            {
                NetworkClient.PushMessage(new NetworkMessage
                {
                    msgType = RecconectTry == m_RecconectTry ? InternalMessages.DISCONNECT : InternalMessages.DISCONNECT_BUT_WILL_RECONNECT,
                    conn = con,
                    reader = new NetworkReader()
                });
                NetworkStatisiticsManager.Remove(con);
                con.Dispose();
                Connected = false;
                LastTryToConnect = Time.time + NetConfig.ReconnectTimeOut;
                return true;
            }
            LastTryToConnect = Time.time + NetConfig.ReconnectTimeOut;
            Connected = false;
            return false;
        }

        public void StartListening()
        {

        }

        public void SetOperational(bool count)
        {
            PermaDisconnected = count;
        }

        public void DoUpdate()
        {
            if (Connecting)
                return;

            if (PermaDisconnected)
                return;

            if (!Connected)
            {
                if (LastTryToConnect > Time.time)
                    return;

                if (RecconectTry > m_RecconectTry)
                {
                    LastTryToConnect = Time.time + NetConfig.ReconnectTimeOut;
                    m_RecconectTry++;
                    StartClient();
                }
                else
                {
                    if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug("Could not connect to the server.");
                    PermaDisconnected = true;
                }
            }
        }

        public void Send(uint connectionId, short msgType, BaseNetworkMessage msg)
        {
            if (connection != null && connection.ConnectionReady())
            {
                connection.Send(msgType, msg);
                return;
            }
            if (NetConfig.logFilter >= LogFilter.Error)
            {
                Log.Error("CLIENT BASE TRANSPORT Failed to send message to connection ID '" + connectionId + ", not found in connection list");
            }
        }

        public void SendToAll(short msgType, BaseNetworkMessage msg)
        {

        }

        public void Stop()
        {
            PermaDisconnected = true;
            NetworkClient.Instance.ClientConnected = false;
            Connected = false;
            NetworkStatisiticsManager.Remove(connection);
            connection.Dispose();
        }

        public void Reset()
        {
            RecconectTry = NetConfig.ReconnectAttempts;
            m_RecconectTry = 0;
        }
    }
}
