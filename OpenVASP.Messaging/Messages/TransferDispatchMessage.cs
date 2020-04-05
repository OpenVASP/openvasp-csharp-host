using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferDispatchMessage : MessageBase
    {
        public TransferDispatchMessage(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            Transaction transaction,
            VaspInformation vasp)
        {
            MessageType = MessageType.TransferDispatch;
            Message = message;
            Originator = originator;
            Beneficiary = beneficiary;
            Transfer = transfer;
            Transaction = transaction;
            VASP = vasp;
        }

        public TransferDispatchMessage(
            string sessionId,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            Transaction transaction,
            VaspInformation vasp)
        {
            MessageType = MessageType.TransferDispatch;
            Message = new Message(Guid.NewGuid().ToString(), sessionId, "1");
            Originator = originator;
            Beneficiary = beneficiary;
            Transfer = transfer;
            Transaction = transaction;
            VASP = vasp;
        }

        public Originator Originator { get; private set; }

        public Beneficiary Beneficiary { get; private set; }

        public TransferReply Transfer { get; private set; }

        public Transaction Transaction { get; private set; }

        public Message Message { get; private set; }

        public VaspInformation VASP { get; private set; }
    }
}
