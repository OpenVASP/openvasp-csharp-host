namespace OpenVASP.Messaging.Messages.Entities
{
    public class TransferRequest
    {
        public TransferRequest(
            VirtualAssetType virtualAssetType,
            TransferType transferType, 
            string amount)
        {
            VirtualAssetType = virtualAssetType;
            TransferType = transferType;
            Amount = amount;
        }

        public VirtualAssetType VirtualAssetType { get; private set; }

        public TransferType TransferType { get; private set; }

        //ChooseType as BigInteger
        public string Amount { get; private set; }
    }

    public enum VirtualAssetType
    {
        BTC = 1,
        ETH = 2
    }

    public enum TransferType
    {
        BlockchainTransfer = 1,
    }
}