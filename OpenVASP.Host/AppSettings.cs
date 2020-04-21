using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host
{
    public class AppSettings
    {
        public string InstanceName { set; get; }
        
        public string EthereumRpcUri { set; get; }
        
        public string WhisperRpcUri { set; get; }
        
        public string HandshakePrivateKeyHex { set; get; }
        
        public string SignaturePrivateKeyHex { set; get; }
        
        public string VaspSmartContractAddress { set; get; }
        
        public PlaceOfBirth VaspPlaceOfBirth { set; get; }
        
        public string VaspBic { set; get; }
        
        public JuridicalPersonId[] VaspJuridicalIds { set; get; }
        
        public NaturalPersonId[] VaspNaturalIds { set; get; }
    }
}