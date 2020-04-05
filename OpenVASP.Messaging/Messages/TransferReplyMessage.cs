using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferReplyMessage : MessageBase
    {
        public TransferReplyMessage(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            VaspInformation vasp)
        {
            MessageType = MessageType.TransferReply;
            Message = message;
            Originator = originator;
            Beneficiary = beneficiary;
            Transfer = transfer;
            VASP = vasp;
        }

        public TransferReplyMessage(
            string sessionId,
            TransferReplyMessageCode transferReplyMessageCode,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            VaspInformation vasp)
        {
            MessageType = MessageType.TransferReply;
            Message = new Message(Guid.NewGuid().ToString(), sessionId, GetMessageCode(transferReplyMessageCode));
            Originator = originator;
            Beneficiary = beneficiary;
            Transfer = transfer;
            VASP = vasp;
        }

        public Originator Originator { get; private set; }

        public Beneficiary Beneficiary { get; private set; }

        public TransferReply Transfer { get; private set; }

        public Message Message { get; private set; }

        public VaspInformation VASP { get; private set; }

        public static string GetMessageCode(TransferReplyMessageCode messageCode)
        {
            return ((int)messageCode).ToString();
        }
        public enum TransferReplyMessageCode
        {
            TransferAccepted = 1,
            TransferDeclinedRequestNotValid = 2,
            TransferDeclinedNoSuchBeneficiary = 3,
            TransferDeclinedVirtualAssetNotSupported = 4,
            TransferDeclinedTransferNotAuthorized = 5,
            TransferDeclinedTemporaryDisruptionOfService = 6,
        }
    }
}
