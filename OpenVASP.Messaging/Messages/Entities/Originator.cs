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
            PlaceOfBirth = placeOfBirth;
            NaturalPersonId = naturalPersonId;
            JuridicalPersonId = juridicalPersonId;
            BIC = bic;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vaan")]
        public string VAAN { get; set; }

        [JsonProperty("address")]
        public PostalAddress PostalAddress { get; set; }

        [JsonProperty("birth", NullValueHandling=NullValueHandling.Ignore)]
        public PlaceOfBirth PlaceOfBirth { get; set; }

        [JsonProperty("nat", NullValueHandling=NullValueHandling.Ignore)]
        public NaturalPersonId[] NaturalPersonId { get; set; }

        [JsonProperty("jur", NullValueHandling=NullValueHandling.Ignore)]
        public JuridicalPersonId[] JuridicalPersonId { get; set; }

        [JsonProperty("bic", NullValueHandling=NullValueHandling.Ignore)]
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