using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace JTcpNetwork
{
    public struct SendState
    {
        public short msgType;
        public BaseNetworkMessage packet;
    }
}
