using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferDispatchMessage : MessageBase
    {
        public static TransferDispatchMessage Create(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            Transaction transaction,
            VaspInformation vasp)
        {
            return new TransferDispatchMessage
            {
                MessageType = MessageType.TransferDispatch,
                Message = message,
                Originator = originator,
                Beneficiary = beneficiary,
                Transfer = transfer,
                Transaction = transaction,
                VASP = vasp
            };
        }

        public static TransferDispatchMessage Create(
            string sessionId,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            Transaction transaction,
            VaspInformation vasp)
        {
            return new TransferDispatchMessage
            {
                MessageType = MessageType.TransferDispatch,
                Message = new Message(Guid.NewGuid().ToString(), sessionId, "1"),
                Originator = originator,
                Beneficiary = beneficiary,
                Transfer = transfer,
                Transaction = transaction,
                VASP = vasp
            };
        }

        [JsonProperty("originator")]
        public Originator Originator { get; private set; }

        [JsonProperty("beneficiary")]
        public Beneficiary Beneficiary { get; private set; }

        [JsonProperty("transfer")]
        public TransferReply Transfer { get; private set; }

        [JsonProperty("transaction")]
        public Transaction Transaction { get; private set; }

        [JsonProperty("msg")]
        public Message Message { get; private set; }

        [JsonProperty("vasp")]
        public VaspInformation VASP { get; private set; }
    }
}
