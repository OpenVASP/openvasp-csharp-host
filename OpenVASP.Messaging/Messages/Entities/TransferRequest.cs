using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class TransferRequest
    {
        public TransferRequest(
            VirtualAssetType virtualAssetType,
            TransferType transferType, 
            decimal amount)
        {
            VirtualAssetType = virtualAssetType;
            TransferType = transferType;
            Amount = amount;
        }

        [JsonProperty("va"), JsonConverter(typeof(StringEnumConverter))]
        public VirtualAssetType VirtualAssetType { get; private set; }

        [JsonProperty("ttype")]
        public TransferType TransferType { get; private set; }

        [JsonProperty("amount")]
        //ChooseType as BigInteger
        public decimal Amount { get; private set; }
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