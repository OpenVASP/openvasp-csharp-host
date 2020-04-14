using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.CSharpClient
{
    public class VaspSessionCounterparty
    {
        public VaspInformation VaspInfo { get; set; }
        public VirtualAssetsAccountNumber Vaan { get; set; }
    }
}