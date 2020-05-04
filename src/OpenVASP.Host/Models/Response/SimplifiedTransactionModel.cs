using System;
using OpenVASP.Host.Core.Models;

namespace OpenVASP.Host.Models.Response
{
    public class SimplifiedTransactionModel
    {
        public string Id { set; get; }
        public DateTime CreationDateTime { set; get; }
        
        public TransactionStatus Status { set; get; }
        public string StatusStringified { set; get; }
    }
}