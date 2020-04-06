using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging
{
    public class MessageEnvelope
    {
        public string Topic { get; set; }
        public string SigningKey { get; set; }
        public EncryptionType EncryptionType { get; set; }
        public string EncryptionKey { get; set; }
    }
}