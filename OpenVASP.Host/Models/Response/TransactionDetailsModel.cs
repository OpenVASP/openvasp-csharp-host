using System;
using Newtonsoft.Json.Converters;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Models.Response
{
    public class TransactionDetailsModel
    {
        public string TransactionType { set; get; }
        public string Id { set; get; }
        public string SessionId { set; get; }
        public string Asset { set; get; }
        public decimal Amount { set; get; }
        public string OriginatorPostalAddressStreet { get; set; }
        public int OriginatorPostalAddressBuilding { get; set; }
        public string OriginatorPostalAddressAddressLine { get; set; }
        public string OriginatorPostalAddressPostCode { get; set; }
        public string OriginatorPostalAddressTown { get; set; }
        public string OriginatorPostalAddressCountry { get; set; }
        public string OriginatorPlaceOfBirthTown { set; get; }
        public string OriginatorPlaceOfBirthCountry { set; get; }
        public DateTime OriginatorPlaceOfBirthDate { set; get; }
        public string OriginatorFullName { set; get; }
        public string OriginatorVaan { set; get; }
        public string BeneficiaryFullName { set; get; }
        public string BeneficiaryVaan { set; get; }
        public DateTime CreationDateTime { set; get; }
        public string StatusStringified { set; get; }
        public string DestinationAddress { get; set; }
        public string TransactionHash { get; set; }
        public string SendingAddress { get; set; }
    }
}