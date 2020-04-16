using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class HandShakeRequest
    {
        [JsonProperty("topica")]
        public string TopicA { get; private set; }

        [JsonProperty("ecdhpk")]
        public string EcdhPubKey { get; private set; }

        public HandShakeRequest(string topicA, string ecdhPubKey)
        {
            TopicA = topicA;
            EcdhPubKey = ecdhPubKey;
        }
    }

    public class HandShakeResponse
    {
        [JsonProperty("topicb")]
        public string TopicB { get; private set; }

        public HandShakeResponse(string topicB)
        {
            TopicB = topicB;
        }
    }
}