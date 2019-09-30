namespace JTcpNetwork
{
    public abstract class BaseNetworkMessage
    {
        /// <summary>
        /// De-serialize the contents of the reader into this message
        /// </summary>
        public virtual void Deserialize(NetworkReader reader) { }

        /// <summary>
        ///  // Serialize the contents of this message into the writer
        /// </summary>
        public virtual void Serialize(NetworkWriter writer) { }
    }

}
