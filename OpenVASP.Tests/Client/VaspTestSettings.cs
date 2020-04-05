using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Tests.Client
{
    public class VaspTestSettings
    {
        public object NodeRPC;
        public string VaspSmartContractAddressPerson { get; set; }
        public string VaspSmartContractAddressJuridical { get; set; }

        public NaturalPersonId[] NaturalPersonIds { get; set; }
        public PlaceOfBirth PlaceOfBirth { get; set; }
        public JuridicalPersonId[] JuridicalIds { get; set; }
        public string Bic { get; set; }

        public string PersonHandshakePrivateKeyHex { get; set; }
        public string PersonSignaturePrivateKeyHex { get; set; }
        public string JuridicalHandshakePrivateKeyHex { get; set; }
        public string JuridicalSignaturePrivateKeyHex { get; set; }
    }
}