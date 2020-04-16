using System.Linq;
using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class VaspInformation
    {
        public VaspInformation(
            string name, 
            string vaspIdentity, 
            string vaspPublickKey, 
            PostalAddress postalAddress, 
            PlaceOfBirth placeOfBirth, 
            NaturalPersonId[] naturalPersonIds, 
            JuridicalPersonId[] juridicalPersonIds, 
            string bic)
        {
            Name = name;
            VaspIdentity = vaspIdentity;
            VaspPublickKey = vaspPublickKey;
            PostalAddress = postalAddress;
            PlaceOfBirth = placeOfBirth;
            NaturalPersonIds = naturalPersonIds;
            JuridicalPersonIds = juridicalPersonIds;
            BIC = bic;
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string VaspIdentity { get; set; }

        [JsonProperty("pk")]
        public string VaspPublickKey { get; set; }

        [JsonProperty("address")]
        public PostalAddress PostalAddress { get; set; }

        [JsonProperty("birth")]
        public PlaceOfBirth PlaceOfBirth { get; set; }

        [JsonProperty("nat")]
        public NaturalPersonId[] NaturalPersonIds { get; set; }

        [JsonProperty("jur")]
        public JuridicalPersonId[] JuridicalPersonIds { get; set; }

        [JsonProperty("bic")]
        public string BIC { get; set; }

        public string GetVaspCode()
        {
            var vaspCode = new string(VaspIdentity.Skip(VaspIdentity.Length - 8).Take(8).ToArray());

            return vaspCode;
        }
    }
}