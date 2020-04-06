namespace OpenVASP.Messaging.Messages.Entities
{
    public class Message
    {
        public Message(string messageId, string sessionId, string messageCode)
        {
            MessageId = messageId;
            SessionId = sessionId;
            MessageCode = messageCode;
        }

        public string MessageId { get; private set; }

        public string SessionId { get; private set; }

        public string MessageCode { get; private set; }
    }
}