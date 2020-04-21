using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class MessageBase
    {
        [JsonProperty("msg")]
        public Message Message { get; set; }
        
        [JsonProperty("comment")]
        public string Comment { get; set; } = string.Empty;
    }
}
