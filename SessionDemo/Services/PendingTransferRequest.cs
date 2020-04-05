using System;
using OpenVASP.CSharpClient;
using OpenVASP.Messaging.Messages.Entities;

namespace SessionDemo.Services
{
    public class PendingTransferRequest
    {
        public string BeneficiaryName { set; get; }
        public string OriginatorName { set; get; }
        public DateTime OriginatorDateOfBirth { set; get; }
        public PostalAddress PostalAddress { set; get; }
        public VirtualAssetTransfer Transfer { set; get; }
    }
}