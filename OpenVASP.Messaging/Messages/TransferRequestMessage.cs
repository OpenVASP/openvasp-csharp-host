using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TransferRequestMessage : MessageBase
    {
        public static TransferRequestMessage Create(
            Message message,
            Originator originator,
            Beneficiary beneficiary,
            TransferRequest transfer,
            VaspInformation vasp)
        {
            return new TransferRequestMessage
            {
                MessageType = MessageType.TransferRequest,
                Message = message,
                Originator = originator,
                Beneficiary = beneficiary,
                Transfer = transfer,
                VASP = vasp
            };
        }

        public static TransferRequestMessage Create(
            string sessionId,
            Originator originator,
            Beneficiary beneficiary,
            TransferRequest transfer,
            VaspInformation vasp)
        {
            return new TransferRequestMessage
            {
                MessageType = MessageType.TransferRequest,
                Message = new Message(Guid.NewGuid().ToString(), sessionId, "1"),
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
        public TransferRequest Transfer { get; private set; }

        [JsonProperty("msg")]
        public Message Message { get; private set; }

        [JsonProperty("vasp")]
        public VaspInformation VASP { get; private set; }
    }
}
