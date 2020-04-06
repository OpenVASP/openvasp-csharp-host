using System;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class Transaction
    {
        public Transaction(string transactionId, DateTime dateTime, string sendingAddress)
        {
            TransactionId = transactionId;
            DateTime = dateTime;
            SendingAddress = sendingAddress;
        }

        public string TransactionId { get; private set; }

        public DateTime DateTime { get; private set; }

        public string SendingAddress { get; private set; }
    }
}