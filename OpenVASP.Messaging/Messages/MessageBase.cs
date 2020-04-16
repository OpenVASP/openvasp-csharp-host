using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages
{
    public class MessageBase
    {
        [JsonProperty("type")]
        public MessageType MessageType { get; protected set; }
        
        [JsonProperty("comment")]
        public string Comment { get; set; } = string.Empty;
    }
}
