using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class TransferReply
    {
        public TransferReply(
            VirtualAssetType virtualAssetType,
            TransferType transferType, 
            decimal amount,
            string destinationAddress)
        {
            VirtualAssetType = virtualAssetType;
            TransferType = transferType;
            Amount = amount;
            DestinationAddress = destinationAddress;
        }

        [JsonProperty("destination")]
        public string DestinationAddress { get; private set; }

        [JsonProperty("va"), JsonConverter(typeof(StringEnumConverter))]
        public VirtualAssetType VirtualAssetType { get; private set; }

        [JsonProperty("ttype")]
        public TransferType TransferType { get; private set; }

        [JsonProperty("amount")]
        //ChooseType as BigInteger
        public decimal Amount { get; private set; }
    }
}