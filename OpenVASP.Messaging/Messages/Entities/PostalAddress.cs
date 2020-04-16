using Newtonsoft.Json;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class PostalAddress
    {
        public PostalAddress(string streetName, int buildingNumber, string addressLine, string postCode, string townName, Country country)
        {
            StreetName = streetName;
            BuildingNumber = buildingNumber.ToString(); //todo: remove int everywhere.
            AddressLine = addressLine;
            PostCode = postCode;
            TownName = townName;
            Country = country;
        }

        [JsonProperty("street")]
        public string StreetName { get; private set; }

        [JsonProperty("number")]
        public string BuildingNumber { get; private set; }

        [JsonProperty("adrline")]
        public string AddressLine { get; private set; }

        [JsonProperty("postcode")]
        public string PostCode { get; private set; }

        [JsonProperty("town")]
        public string TownName { get; private set; }

        [JsonProperty("country")]
        [JsonConverter(typeof(CountryConverter))]
        public Country Country { get; private set; }

    }
}