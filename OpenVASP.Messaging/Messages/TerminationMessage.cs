using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TerminationMessage : MessageBase
    {
        public static TerminationMessage Create(Message message, VaspInformation vasp)
        {
            return new TerminationMessage
            {
                MessageType = MessageType.Termination,
                Message = message,
                VASP = vasp,
            };
        }

        public static TerminationMessage Create(string sessionId, TerminationMessageCode messageCode, VaspInformation vasp)
        {
            return new TerminationMessage
            {
                MessageType = MessageType.Termination,
                Message = new Message(
                Guid.NewGuid().ToString(),
                sessionId,
                GetMessageCode(messageCode)),
                VASP = vasp
            };
        }
        
        [JsonProperty("msg")]
        public Message Message { get; private set; }

        [JsonProperty("vasp")]
        public VaspInformation VASP { get; private set; }

        public TerminationMessageCode GetMessageCode()
        {
            Enum.TryParse<TerminationMessageCode>(this.Message.MessageCode, out var result);

            return result;
        }

        public static string GetMessageCode(TerminationMessageCode messageCode)
        {
            return ((int)messageCode).ToString();
        }

        public enum TerminationMessageCode
        {
            SessionClosedTransferOccured = 1,
            SessionClosedTransferDeclinedByBeneficiaryVasp = 2,
            SessionClosedTransferCancelledByOriginator = 3,
        }
    }
}
