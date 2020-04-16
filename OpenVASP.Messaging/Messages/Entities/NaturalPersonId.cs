using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class NaturalPersonId
    {
        [JsonProperty("natid_type")]
        public NaturalIdentificationType IdentificationType { get; private set; }

        [JsonProperty("natid")]
        public string Identifier { get; private set; }

        [JsonProperty("natid_country")]
        [JsonConverter(typeof(CountryConverter))]
        public Country IssuingCountry { get; private set; }

        [JsonProperty("natid_issuer")]
        public string NonStateIssuer { get; private set; }

        public NaturalPersonId(string identifier, NaturalIdentificationType identificationType, Country issuingCountry, string nonStateIssuer = "")
        {
            Identifier = identifier;
            IdentificationType = identificationType;
            IssuingCountry = issuingCountry;
            NonStateIssuer = nonStateIssuer;
        }
    }
}