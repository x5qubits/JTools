using JCommon;
using System;
using System.Collections.Generic;

namespace JTcpNetwork
{
    class PacketFarmer
    {
        List<byte> btbuf = new List<byte>();
        protected NetworkWriter m_Writer = new NetworkWriter();
        protected NetworkReader m_Reader = new NetworkReader();
        public const int PACKAGE_HEADER_LEN = 6;
        public const uint PACKAGE_HEADER_ID = 1860168941;

        public NetworkReader Accumulate(byte bt)
        {
            btbuf.Add(bt);
            if (btbuf.Count < PACKAGE_HEADER_LEN)
                return null;

            byte[] buff = btbuf.ToArray();
            uint head = ReadUInt32(buff, 0);
            uint len = ReadUInt16(buff, 4);
            int ptr = btbuf.Count - PACKAGE_HEADER_LEN;
            if (head == PACKAGE_HEADER_ID)
            {
                if (btbuf.Count >= 65536)
                {
                    btbuf.Clear();
                    return null;
                }

                if (len == ptr)
                {
                    if (NetConfig.logFilter >= LogFilter.Developer)
                        Log.Debug("Read:" + BitConverter.ToString(buff));

                    btbuf.Clear();
                    var x = new NetworkReader(buff);
                    x.ReadBytes(PACKAGE_HEADER_LEN);
                    return x;
                }
            }

            return null;
        }

        public uint ReadUInt32(byte[] m_buf, int pos)
        {
            uint value = 0;
            value |= m_buf[pos];
            value |= (uint)(m_buf[pos + 1] << 8);
            value |= (uint)(m_buf[pos + 2] << 16);
            value |= (uint)(m_buf[pos + 3] << 24);
            return value;
        }

        public ushort ReadUInt16(byte[] m_buf, int pos)
        {
            ushort value = 0;
            value |= m_buf[pos];
            value |= (ushort)(m_buf[pos + 1] << 8);
            return value;
        }

        public byte[] Write(ushort value, int pos = 0)
        {
            byte[] m_Buffer = new byte[2];
            m_Buffer[pos] = (byte)(value & 0xff);
            m_Buffer[pos + 1] = (byte)((value >> 8) & 0xff);
            return m_Buffer;
        }

        public byte[] Write(uint value, int pos = 0)
        {
            byte[] m_Buffer = new byte[4];
            m_Buffer[pos] = (byte)(value & 0xff);
            m_Buffer[pos + 1] = (byte)((value >> 8) & 0xff);
            m_Buffer[pos + 2] = (byte)((value >> 16) & 0xff);
            m_Buffer[pos + 3] = (byte)((value >> 24) & 0xff);
            return m_Buffer;
        }

        public byte[] ToBytes(short msgType, BaseNetworkMessage packet)
        {
            lock (m_Writer)
            {
                m_Writer.StartMessage(msgType);
                packet.Serialize(m_Writer);
                m_Writer.FinishMessage();
                byte[] buf = m_Writer.ToArray();
                List<byte> listbuf = new List<byte>();
                listbuf.AddRange(Write(PACKAGE_HEADER_ID));
                listbuf.AddRange(Write((ushort)buf.Length));
                listbuf.AddRange(buf);
                byte[] nesent = listbuf.ToArray();
                if (NetConfig.logFilter >= LogFilter.Developer)
                    Log.Debug("Write:" + BitConverter.ToString(nesent));
                return nesent;
            }
        }
    }
}
