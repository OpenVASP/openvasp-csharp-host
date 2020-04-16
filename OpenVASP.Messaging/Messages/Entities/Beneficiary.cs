using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class Beneficiary
    {
        public Beneficiary(string name, string vaan)
        {
            Name = name ?? "";
            VAAN = vaan;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vaan")]
        public string VAAN { get; set; }
    }
}