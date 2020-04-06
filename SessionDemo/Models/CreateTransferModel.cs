using System;
using System.ComponentModel.DataAnnotations;
using OpenVASP.Messaging.Messages.Entities;

namespace SessionDemo.Models
{
    public class CreateTransferModel
    {
        public string BeneficiaryName { set; get; }
        
        public string BeneficiaryVaan { set; get; }
        
        public string OriginatorName { get; set; }
        
        public string PostalAddressStreetName { set; get; }
        
        public int PostalAddressBuildingNumber { set; get; }
        
        public string PostalAddressAddressLine { set; get; }
        
        public string PostalAddressPostCode { set; get; }
        
        public string PostalAddressTownName { set; get; }
        
        public DateTime DateOfBirth { set; get; }
        
        public string PostalAddressCountry { set; get; }
        
        public VirtualAssetType AssetType { set; get; }
        
        public decimal TransactionAmount { set; get; }
    }
}