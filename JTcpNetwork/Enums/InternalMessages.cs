namespace JTcpNetwork
{
    public class InternalMessages
    {
        /// <summary>
        /// CLIENT PROTOCOL THAT WILL VERIFY HEAND SHAKE
        /// </summary>
        public const short HeandShake_Client = 1;
        /// <summary>
        /// SERVER PROTOCOL THAT WILL VERIFY HEAND SHAKE
        /// </summary>
        public const short HeandShake_Server = 2;
        /// <summary>
        /// INTERNAL CONNECTED MESSAGE
        /// </summary>
        public const short CONNECTED = 10;
        /// <summary>
        /// INTERNALL DISCONNECTED MESSAGE
        /// </summary>
        public const short DISCONNECT = 12;
        /// <summary>
        /// INTERNALL RECIEVE MESSAGE
        /// </summary>
        public const short RECIEVE = 13;
        /// <summary>
        /// INTERNALL ERROR MESSAGE
        /// </summary>
        public const short ERROR = 14;
        /// <summary>
        /// DISCONNECT BUT WILL RECONNECT
        /// </summary>
        public const short DISCONNECT_BUT_WILL_RECONNECT = 15;
    }
}
