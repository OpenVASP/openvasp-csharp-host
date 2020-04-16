using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferConfirmationMessage : MessageBase
    {
        public static TransferConfirmationMessage Create(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            Transaction transaction,
            VaspInformation vasp)
        {
            return new TransferConfirmationMessage
            {
                MessageType = MessageType.TransferConfirmation,
                Message = message,
                Originator = originator,
                Beneficiary = beneficiary,
                Transfer = transfer,
                Transaction = transaction,
                VASP = vasp
            };
        }

        public static TransferConfirmationMessage Create(
            string sessionId,
            TransferConfirmationMessageCode messageCode,
            Originator originator,
            Beneficiary beneficiary,
            TransferReply transfer,
            Transaction transaction,
            VaspInformation vasp)
        {
            return new TransferConfirmationMessage
            {
                MessageType = MessageType.TransferConfirmation,
                Message = new Message(Guid.NewGuid().ToString(), sessionId, GetMessageCode(messageCode)),
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

        public static string GetMessageCode(TransferConfirmationMessageCode messageCode)
        {
            return ((int)messageCode).ToString();
        }
        public enum TransferConfirmationMessageCode
        {
            TransferConfirmed = 1,
            TransferNotConfirmedDispatchNotValid = 2,
            TransferNotConfirmedAssetsNotReceived = 3,
            TransferNotConfirmedWrongAmount = 4,
            TransferNotConfirmedWrongAsset = 5,
            TransferNotConfirmedTransactionDataMissmatch = 6,
        }
    }
}
