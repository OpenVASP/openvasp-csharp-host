using Newtonsoft.Json;

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

        [JsonProperty("destination")]
        public string DestinationAddress { get; private set; }

        [JsonProperty("va")]
        public VirtualAssetType VirtualAssetType { get; private set; }

        [JsonProperty("ttype")]
        public TransferType TransferType { get; private set; }

        [JsonProperty("amount")]
        //ChooseType as BigInteger
        public string Amount { get; private set; }
    }
}