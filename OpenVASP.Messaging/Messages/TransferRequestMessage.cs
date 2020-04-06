using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferRequestMessage : MessageBase
    {
        public TransferRequestMessage(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferRequest transfer,
            VaspInformation vasp)
        {
            MessageType = MessageType.TransferRequest;
            Message = message;
            Originator = originator;
            Beneficiary = beneficiary;
            Transfer = transfer;
            VASP = vasp;
        }

        public TransferRequestMessage(
            string sessionId,
            Originator originator,
            Beneficiary beneficiary,
            TransferRequest transfer,
            VaspInformation vasp)
        {
            MessageType = MessageType.TransferRequest;
            Message = new Message(Guid.NewGuid().ToString(), sessionId, "1");
            Originator = originator;
            Beneficiary = beneficiary;
            Transfer = transfer;
            VASP = vasp;
        }

        public Originator Originator { get; private set; }

        public Beneficiary Beneficiary { get; private set; }

        public TransferRequest Transfer { get; private set; }

        public Message Message { get; private set; }

        public VaspInformation VASP { get; private set; }

    }
}
