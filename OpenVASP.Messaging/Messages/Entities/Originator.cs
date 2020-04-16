using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class Originator
    {
        public Originator(
            string name, 
            string vaan, 
            PostalAddress postalAddress, 
            PlaceOfBirth placeOfBirth, 
            NaturalPersonId[] naturalPersonId, 
            JuridicalPersonId[] juridicalPersonId, 
            string bic)
        {
            Name = name;
            VAAN = vaan;
            PostalAddress = postalAddress;
            PlaceOfBirth = placeOfBirth ?? default;
            NaturalPersonId = naturalPersonId ?? new NaturalPersonId[] {};
            JuridicalPersonId = juridicalPersonId ?? new JuridicalPersonId[] {};
            BIC = bic ?? string.Empty;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vaan")]
        public string VAAN { get; set; }

        [JsonProperty("address")]
        public PostalAddress PostalAddress { get; set; }

        [JsonProperty("birth")]
        public PlaceOfBirth PlaceOfBirth { get; set; }

        [JsonProperty("nat")]
        public NaturalPersonId[] NaturalPersonId { get; set; }

        [JsonProperty("jur")]
        public JuridicalPersonId[] JuridicalPersonId { get; set; }

        [JsonProperty("bic")]
        public string BIC { get; set; }

        public static Originator CreateOriginatorForNaturalPerson(
            string originatorName, 
            VirtualAssetsAccountNumber vaan,
            PostalAddress postalAddress,
            PlaceOfBirth placeOfBirth,
            NaturalPersonId[] naturalPersonIds)
        {
            var originator = new Originator(originatorName, vaan.Vaan, postalAddress, placeOfBirth, naturalPersonIds, null,null);

            return originator;
        }
    }
}