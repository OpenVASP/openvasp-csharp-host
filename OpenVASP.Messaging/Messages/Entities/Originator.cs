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

        public string Name { get; set; }

        public string VAAN { get; set; }

        public PostalAddress PostalAddress { get; set; }

        public PlaceOfBirth PlaceOfBirth { get; set; }

        public NaturalPersonId[] NaturalPersonId { get; set; }

        public JuridicalPersonId[] JuridicalPersonId { get; set; }

        public string BIC { get; set; }

        public static Originator CreateOriginatorForNaturalPerson(
            string originatorName, 
            VirtualAssetssAccountNumber vaan,
            PostalAddress postalAddress,
            PlaceOfBirth placeOfBirth,
            NaturalPersonId[] naturalPersonIds)
        {
            var originator = new Originator(originatorName, vaan.Vaan, postalAddress, placeOfBirth, naturalPersonIds, null,null);

            return originator;
        }
    }
}