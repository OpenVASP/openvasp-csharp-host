using OpenVASP.Messaging.Messages.Entities;
using System;

namespace OpenVASP.Host.Models.Response
{
    public class TransactionDetailsModel
    {
        public string Id { set; get; }
        public string SessionId { set; get; }
        public string Asset { set; get; }
        public decimal Amount { set; get; }
        public string OriginatorPostalAddressPostCode { get; set; }
        public string OriginatorPlaceOfBirthCountryIso2Code { set; get; }
        public DateTime OriginatorPlaceOfBirthDate { set; get; }
        public string OriginatorPostalAddressTown { get; set; }
        public string OriginatorPostalAddressCountryIso2Code { get; set; }
        public string OriginatorPlaceOfBirthTown { set; get; }
        public string OriginatorPostalAddressStreet { get; set; }
        public string OriginatorPostalAddressBuilding { get; set; }
        public string OriginatorPostalAddressAddressLine { get; set; }
        public string OriginatorFullName { set; get; }
        public string OriginatorVaan { set; get; }
        public string CounterPartyVaspName { get; set; }
        public VaspInformation CounterPartyVasp { set; get; }
        public NaturalPersonId[] OriginatorNaturalPersonIds { set; get; }
        public JuridicalPersonId[] OriginatorJuridicalPersonIds { set; get; }
        public string OriginatorBic { set; get; }
        public string BeneficiaryFullName { set; get; }
        public string BeneficiaryVaan { set; get; }
        public DateTime CreationDateTime { set; get; }
        public string Status { set; get; }
        public string DestinationAddress { get; set; }
        public string TransactionHash { get; set; }
        public DateTime? TransactionDateTime { get; set; }
        public string SendingAddress { get; set; }
        public string SessionDeclineCode { set; get; }
        public string TransferDeclineCode { set; get; }
        public string TransactionType { set; get; }
    }
}