using Newtonsoft.Json;

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

        [JsonProperty("msgid")]
        public string MessageId { get; private set; }

        [JsonProperty("session")]
        public string SessionId { get; private set; }

        [JsonProperty("code")]
        public string MessageCode { get; private set; }
    }
}