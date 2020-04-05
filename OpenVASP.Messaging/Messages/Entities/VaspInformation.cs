using System.Linq;

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

        public string Name { get; set; }

        public string VaspIdentity { get; set; }

        public string VaspPublickKey { get; set; }

        public PostalAddress PostalAddress { get; set; }

        public PlaceOfBirth PlaceOfBirth { get; set; }

        public NaturalPersonId[] NaturalPersonIds { get; set; }

        public JuridicalPersonId[] JuridicalPersonIds { get; set; }

        public string BIC { get; set; }

        public string GetVaspCode()
        {
            var vaspCode = new string(VaspIdentity.Skip(VaspIdentity.Length - 8).Take(8).ToArray());

            return vaspCode;
        }
    }
}