namespace OpenVASP.Messaging.Messages.Entities
{
    public class JuridicalPersonId
    {
        public JuridicalIdentificationType IdentificationType { get; private set; }

        public string Identifier { get; private set; }

        public Country IssuingCountry { get; private set; }

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