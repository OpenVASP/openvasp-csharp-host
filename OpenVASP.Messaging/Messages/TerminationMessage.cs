using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class TerminationMessage : MessageBase
    {
        public TerminationMessage(Message message, VaspInformation vasp)
        {
            MessageType = MessageType.Termination;
            Message = message;
            VASP = vasp;
        }

        public TerminationMessage(string sessionId, TerminationMessageCode messageCode, VaspInformation vasp)
        {
            MessageType = MessageType.Termination;
            Message = new Message(
                Guid.NewGuid().ToString(),
                sessionId,
                GetMessageCode(messageCode));
            VASP = vasp;
        }

        public Message Message { get; private set; }

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
