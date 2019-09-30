using JCommon;
using JCommon.Extensions;
using System.Collections.Generic;

namespace JTcpNetwork
{
    internal class NetworkMessageHandlers
    {
        Dictionary<short, NetworkMessageDelegate> m_MsgHandlers = new Dictionary<short, NetworkMessageDelegate>();

        internal void RegisterHandlerSafe(short msgType, NetworkMessageDelegate handler)
        {
            if (handler == null)
            {
                if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("RegisterHandlerSafe id:" + msgType + " handler is null"); }
                return;
            }

            if (NetConfig.logFilter >= LogFilter.Developer) { Log.Debug("RegisterHandlerSafe id:" + msgType + " handler:" + handler.GetMethodName()); }
            if (m_MsgHandlers.ContainsKey(msgType))
            {
                if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("RegisterHandlerSafe id:" + msgType + " handler:" + handler.GetMethodName() + " conflict"); }
                return;
            }
            m_MsgHandlers.Add(msgType, handler);
        }

        public void RegisterHandler(short msgType, NetworkMessageDelegate handler)
        {
            if (handler == null)
            {
                if (NetConfig.logFilter >= LogFilter.Error) { Log.Error("RegisterHandler id:" + msgType + " handler is null"); }
                return;
            }
            if (m_MsgHandlers.ContainsKey(msgType))
            {
                if (NetConfig.logFilter >= LogFilter.Developer) { Log.Debug("RegisterHandler replacing " + msgType); }

                m_MsgHandlers.Remove(msgType);
            }
            if (NetConfig.logFilter >= LogFilter.Developer) { Log.Debug("RegisterHandler id:" + msgType + " handler:" + handler.GetMethodName()); }
            m_MsgHandlers.Add(msgType, handler);
        }

        public void UnregisterHandler(short msgType)
        {
            if(m_MsgHandlers.ContainsKey(msgType))
                m_MsgHandlers.Remove(msgType);
        }

        internal NetworkMessageDelegate GetHandler(short msgType)
        {
            if (m_MsgHandlers.TryGetValue(msgType, out NetworkMessageDelegate delegat))
                return delegat;
            else
                return null;
        }

        internal Dictionary<short, NetworkMessageDelegate> GetHandlers()
        {
            return m_MsgHandlers;
        }

        internal void ClearMessageHandlers()
        {
            m_MsgHandlers.Clear();
        }
    }
}
