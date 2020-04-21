using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.CSharpClient
{
    public class VirtualAssetTransfer
    {
        public decimal TransferAmount { get; set; }

        public TransferType TransferType { get; set; }

        public VirtualAssetType VirtualAssetType { get; set; }
    }
}