namespace OpenVASP.Messaging.Messages.Entities
{
    public class PostalAddress
    {
        public PostalAddress(string streetName, int buildingNumber, string addressLine, string postCode, string townName, Country country)
        {
            StreetName = streetName;
            BuildingNumber = buildingNumber;
            AddressLine = addressLine;
            PostCode = postCode;
            TownName = townName;
            Country = country;
        }

        public string StreetName { get; private set; }

        public int BuildingNumber { get; private set; }

        public string AddressLine { get; private set; }

        public string PostCode { get; private set; }

        public string TownName { get; private set; }

        public Country Country { get; private set; }

    }
}