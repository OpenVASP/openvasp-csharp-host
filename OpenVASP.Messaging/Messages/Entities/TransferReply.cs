namespace OpenVASP.Messaging.Messages.Entities
{
    public class TransferReply
    {
        public TransferReply(
            VirtualAssetType virtualAssetType,
            TransferType transferType, 
            string amount,
            string destinationAddress)
        {
            VirtualAssetType = virtualAssetType;
            TransferType = transferType;
            Amount = amount;
            DestinationAddress = destinationAddress;
        }

        public string DestinationAddress { get; private set; }

        public VirtualAssetType VirtualAssetType { get; private set; }

        public TransferType TransferType { get; private set; }

        //ChooseType as BigInteger
        public string Amount { get; private set; }
    }
}