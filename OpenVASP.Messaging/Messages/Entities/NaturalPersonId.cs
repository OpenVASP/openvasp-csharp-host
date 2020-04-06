namespace OpenVASP.Messaging.Messages.Entities
{
    public class NaturalPersonId
    {
        public NaturalIdentificationType IdentificationType { get; private set; }

        public string Identifier { get; private set; }

        public Country IssuingCountry { get; private set; }

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