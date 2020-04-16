using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferReplyMessage : MessageBase
    {
        public static TransferReplyMessage Create(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            VaspInformation vasp)
        {
            return new TransferReplyMessage
            {
                MessageType = MessageType.TransferReply,
                Message = message,
                Originator = originator,
                Beneficiary = beneficiary,
                Transfer = transfer,
                VASP = vasp
            };
        }

        public static TransferReplyMessage Create(
            string sessionId,
            TransferReplyMessageCode transferReplyMessageCode,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            VaspInformation vasp)
        {
            return new TransferReplyMessage
            {
                MessageType = MessageType.TransferReply,
                Message = new Message(Guid.NewGuid().ToString(), sessionId, GetMessageCode(transferReplyMessageCode)),
                Originator = originator,
                Beneficiary = beneficiary,
                Transfer = transfer,
                VASP = vasp
            };
        }

        [JsonProperty("originator")]
        public Originator Originator { get; private set; }

        [JsonProperty("beneficiary")]
        public Beneficiary Beneficiary { get; private set; }

        [JsonProperty("transfer")]
        public TransferReply Transfer { get; private set; }

        [JsonProperty("msg")]
        public Message Message { get; private set; }

        [JsonProperty("vasp")]
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
