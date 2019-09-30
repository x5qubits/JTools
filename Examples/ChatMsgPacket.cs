using JTcpNetwork;

namespace Tests
{
    public class ChatMsgPacket : BaseNetworkMessage
    {
        public uint ClientId;
        public string Msg;
        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(ClientId);
            writer.Write(Msg);
        }

        public override void Deserialize(NetworkReader reader)
        {
            ClientId = reader.ReadPackedUInt32();
            Msg = reader.ReadString();
        }
    }
}
