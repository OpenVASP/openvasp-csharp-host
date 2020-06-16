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
        public VaspInformation CounterPartyVasp { set; get; }
        public NaturalPersonId[] OriginatorNaturalPersonIds { set; get; }

        public JuridicalPersonId[] OriginatorJuridicalPersonIds { set; get; }

        public string OriginatorBic { set; get; }
        public string BeneficiaryFullName { set; get; }
        public string BeneficiaryVaan { set; get; }
        public DateTime CreationDateTime { set; get; }
        public DateTime TransactionDateTime { set; get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionStatus Status { set; get; }
        public string DestinationAddress { get; set; }
        public string TransactionHash { get; set; }
        public string SendingAddress { get; set; }
        public string SessionDeclineCode { set; get; }
        public string TransferDeclineCode { set; get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionType Type { set; get; }
    }
}