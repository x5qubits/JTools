namespace JTcpNetwork
{
    public class HandShakeMsg : BaseNetworkMessage
    {
        public uint Version = 0;
        public byte OP = 0;
        public override void Deserialize(NetworkReader reader)
        {
            Version = reader.ReadPackedUInt32();
            OP = reader.ReadByte();
        }
        public override void Serialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32(Version);
            writer.Write(OP);
        }
    }
}
