using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class JuridicalPersonId
    {
        [JsonProperty("jurid_type")]
        public JuridicalIdentificationType IdentificationType { get; private set; }

        [JsonProperty("jurid")]
        public string Identifier { get; private set; }

        [JsonProperty("jurid_country")]
        [JsonConverter(typeof(CountryConverter))]
        public Country IssuingCountry { get; private set; }

        [JsonProperty("jurid_issuer")]
        public string NonStateIssuer { get; private set; }

        public JuridicalPersonId(
            string identifier, 
            JuridicalIdentificationType identificationType,
            Country issuingCountry,
            string nonStateIssuer = "")
        {
            IssuingCountry = issuingCountry;
            Identifier = identifier;
            IdentificationType = identificationType;
            NonStateIssuer = nonStateIssuer;
        }
    }
}