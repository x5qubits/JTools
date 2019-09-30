using JCommon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using JCommon.Extensions;

namespace JTcpNetwork
{
    public class NetworkConnection : IDisposable
    {
        public Socket m_socket;
        public float lastReceivedTime;
        float lastSentTime;
        protected IPEndPoint tcpEndPoint;
        public PerStage stage = PerStage.NotConnected;
        public uint connectionId = 0;
        Dictionary<short, NetworkMessageDelegate> m_MessageHandlersDict = new Dictionary<short, NetworkMessageDelegate>();
        const int k_MaxMessageLogSize = 150;
        const int MaxOutGowingMsg = 50;
        public bool m_Disposed = false;
        PacketFarmer PacketFarmer;
        internal NetworkMessageHandlers m_MessageHandlers;
        public int BytesSent = 0;
        public int BitesRev = 0;
        public int PacketsSend = 0;
        public int PacketsRec = 0;
        public int ReadError = 0;
        public int SendError = 0;
        public bool isClient = false;
        readonly object m_lockread = new object();
        readonly object m_lockwrite = new object();
        byte[] m_Connbuffer;
        AsyncCallback asyncrec;
        AsyncCallback asyncsend;
        NetworkEncoder NetworkEncoder;
        public bool IsConnected { get { return m_socket != null && m_socket.Connected; } }

        public NetworkConnection()
        {
            asyncrec = new AsyncCallback(EndReceive);
            asyncsend = new AsyncCallback(EndSend);
            PacketFarmer = new PacketFarmer();
            m_Connbuffer = new byte[1024];
            NetworkEncoder = new NetworkEncoder();
        }

        ~NetworkConnection()
        {
            Dispose(false);
        }

        public bool ConnectionReady()
        {
            return stage == PerStage.Connected && m_socket != null && m_socket.Connected;
        }

        public void Init(bool isClient)
        {
            this.isClient = isClient;
        }

        public void StartReceiving(Socket socket)
        {
            if (socket != null)
            {
                m_socket = socket;
            }

            if (m_socket != null && m_socket.Connected)
            {
                stage = PerStage.Verifying;
                tcpEndPoint = (IPEndPoint)m_socket.RemoteEndPoint;
                try
                {
                    m_socket.BeginReceive(m_Connbuffer, 0, m_Connbuffer.Length, SocketFlags.None, asyncrec, null);
                }
                catch
                {
                    Disconnect();
                }
            }
        }
    #region RECEIVE
        private void EndReceive(IAsyncResult result)
        {
            if (stage == PerStage.NotConnected) return;
            lock (m_lockread)
            {
                try
                {
                    int len = m_socket.EndReceive(result);
                    if (len == 0) { Disconnect(); return; }

                    byte[] buf = NetworkEncoder.Decode(this.m_Connbuffer, len);
                    for (int i = 0; i < buf.Length; i++)
                    {
                        NetworkReader ds = PacketFarmer.Accumulate(buf[i]);
                        if (ds != null)
                        {
                            if (HandleReader(ds))
                            {
                                if (NetConfig.UseStatistics)
                                {
                                    PacketsRec += 1;
                                    BitesRev += len;
                                }
                            }
                        }
                    }
                    m_socket.BeginReceive(this.m_Connbuffer, 0, this.m_Connbuffer.Length, SocketFlags.None, asyncrec, null);
                }
                catch (ObjectDisposedException)
                {
                    // do nothing
                }
                catch (SocketException sk)
                {
                    if (NetConfig.logFilter >= LogFilter.Developer) Log.Error("Excepiton:" + sk.GetErrorCode());
                    Disconnect();
                }
                catch (Exception ex)
                {
                    if (NetConfig.logFilter >= LogFilter.Developer) Log.Error("Excepiton:" + ex.ToString());
                    Disconnect();
                }
            }
        }

        protected bool HandleReader(NetworkReader reader)
        {
            try
            {
                ushort sz = reader.ReadUInt16();
                short msgType = reader.ReadInt16();
                byte[] msgBuffer = reader.ReadBytes(sz);
                if (isClient)
                {
                    NetworkReader msgReader = new NetworkReader(msgBuffer);
                    if (m_MessageHandlersDict.ContainsKey(msgType))
                    {
                        NetworkClient.PushMessage(new NetworkMessage()
                        {
                            msgType = msgType,
                            reader = msgReader,
                            conn = this
                        });
                }
                else
                {
                    if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("Unknown message ID " + msgType + " connId:" + connectionId); }
                    if (NetConfig.UseStatistics)
                        ReadError += 1;
                }
            }
                else
                {
                NetworkReader msgReader = new NetworkReader(msgBuffer);
                NetworkMessageDelegate msgDelegate = null;
                if (m_MessageHandlersDict.ContainsKey(msgType))
                {
                    msgDelegate = m_MessageHandlersDict[msgType];
                }
                if (msgDelegate != null)
                {
                    msgDelegate(new NetworkMessage()
                    {
                        msgType = msgType,
                        reader = msgReader,
                        conn = this
                    });
                }
                else
                {
                    if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("Unknown message ID " + msgType + " connId:" + connectionId); }
                    if (NetConfig.UseStatistics)
                        ReadError += 1;
                }
            }
        }
            catch { }
            return true;
        }
    #endregion

    #region SEND
    Queue<SendState> ToSend = new Queue<SendState>();
    bool sendbegined;
    public void Send(short msgType, BaseNetworkMessage packet)
    {
        try
        {
            lock (ToSend)
                ToSend.Enqueue(new SendState() { msgType = msgType, packet = packet });

            if (sendbegined) return;
            sendbegined = true;
            BeginSend();
        }
        catch
        {
            if (NetConfig.UseStatistics)
                SendError += 1;
        }
    }

    protected void BeginSend()
    {
        if (ToSend.Count == 0)
        {
            sendbegined = false;
            return;
        }
        try
        {
            SendState state;
            lock (ToSend)
            {
                state = ToSend.Dequeue();
            }
            byte[] buf = NetworkEncoder.Encode(PacketFarmer.ToBytes(state.msgType, state.packet));
            m_socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, asyncsend, null);
        }
        catch (ObjectDisposedException)
        {
            // do nothing
        }
        catch (SocketException)
        {
            Disconnect();
        }
        catch (Exception e)
        {
            if (NetConfig.logFilter >= LogFilter.Error) Log.Error("Exception: " + e.ToString());
            Disconnect();
        }
    }

    private void EndSend(IAsyncResult ar)
    {
        try
        {
            int bytesSent = m_socket.EndSend(ar);
            if (NetConfig.UseStatistics)
            {
                BytesSent += bytesSent;
                PacketsSend += 1;
                lastSentTime = Time.time;
            }
            BeginSend();
            if (NetConfig.logFilter >= LogFilter.Developer) Log.Debug(string.Format("Sent {0} bytes.", bytesSent));

        }
        catch (ObjectDisposedException)
        {
            // do nothing
        }
        catch (SocketException)
        {
            Disconnect();
        }
        catch (Exception e)
        {
            if (NetConfig.logFilter >= LogFilter.Error) Log.Error("Exception: " + e.ToString());
            Disconnect();
        }
    }
    #endregion

    public void Dispose()
    {
        Dispose(true);
        // Take yourself off the Finalization queue
        // to prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    public void Disconnect()
    {
        stage = PerStage.NotConnected;
        try
        {
            if (m_socket != null)
                m_socket.Close();
        }
        catch { }
        if (isClient)
            NetworkClient.Disconnect(this);
        else
            NetworkServer.Disconnect(this);
    }

    protected void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!m_Disposed)
        {
            // If disposing equals true, dispose all managed 
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                PacketFarmer = null;
                m_Connbuffer = null;
                m_MessageHandlers = null;
                m_MessageHandlersDict = null;
                m_socket = null;
                tcpEndPoint = null;
            }
        }
        m_Disposed = true;
    }

    internal void SetHandlers(NetworkMessageHandlers handlers)
    {
        m_MessageHandlers = handlers;
        m_MessageHandlersDict = handlers.GetHandlers();
    }

    public void Reset()
    {
        ReadError = 0;
        SendError = 0;
        BytesSent = 0;
        BitesRev = 0;
        PacketsSend = 0;
        PacketsRec = 0;
        stage = PerStage.NotConnected;
    }

    public string IP
    {
        get
        {
            try
            {
                return tcpEndPoint.Address.ToString();
            }
            catch
            { }
            return "";
        }
    }

    public override string ToString()
    {
        return "connectionId:" + connectionId + " BytesSent:" + BytesSent + " BitesRev:" + BitesRev + " PacketsSend:" + PacketsSend + " PacketsRec:" + PacketsRec + " ReadErrors:" + ReadError + " SendError:" + SendError;
    }
}
}
