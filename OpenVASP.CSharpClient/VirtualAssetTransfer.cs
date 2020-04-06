using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.CSharpClient
{
    public class VirtualAssetTransfer
    {
        public string TransferAmount { get; set; }

        public TransferType TransferType { get; set; }

        public VirtualAssetType VirtualAssetType { get; set; }
    }
}