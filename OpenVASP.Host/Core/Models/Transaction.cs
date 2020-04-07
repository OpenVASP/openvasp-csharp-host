using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Core.Models
{
    public class Transaction
    {
        public string Id { set; get; }
        public string SessionId { set; get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualAssetType Asset { set; get; }
        public decimal Amount { set; get; }
        public PlaceOfBirth OriginatorPlaceOfBirth { set; get; }
        public PostalAddress OriginatorPostalAddress { set; get; }
        public string OriginatorFullName { set; get; }
        public string OriginatorVaan { set; get; }
        public string BeneficiaryFullName { set; get; }
        public string BeneficiaryVaan { set; get; }
        public DateTime CreationDateTime { set; get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionStatus Status { set; get; }
        public string DestinationAddress { get; set; }
        public string TransactionHash { get; set; }
        public string SendingAddress { get; set; }
    }

    public enum TransactionStatus
    {
        Created,
        SessionRequested,
        SessionConfirmed,
        TransferRequested,
        TransferForbidden,
        TransferAllowed,
        TransferDispatched,
        TransferConfirmed
    }
}