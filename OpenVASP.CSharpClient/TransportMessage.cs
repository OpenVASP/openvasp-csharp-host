using OpenVASP.Messaging.Messages;

namespace OpenVASP.CSharpClient
{
    public class TransportMessage
    {
        private TransportMessage(MessageBase message, string payload, string signature)
        {
            this.Message = message;
            this.Payload = payload;
            this.Signature = signature;
        }

        public string Payload { get; }

        public string Signature { get; }

        public MessageBase Message { get; }

        public static TransportMessage CreateMessage(MessageBase messageBase, string payload, string signature)
        {
            var message = new TransportMessage(messageBase, payload, signature);

            return message;
        }
    }
}