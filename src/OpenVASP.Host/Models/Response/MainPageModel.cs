using System.Collections.Generic;

namespace OpenVASP.Host.Models.Response
{
    public class MainPageModel
    {
        public List<string> Countries { set; get; }
        public List<string> Assets { set; get; }
        public List<SimplifiedTransactionModel> OutgoingTransactions { set; get; }
        public List<SimplifiedTransactionModel> IncomingTransactions { set; get; }
    }
}