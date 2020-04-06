using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class SessionReplyMessage : MessageBase
    {
        public SessionReplyMessage(Message message, HandShakeResponse handshake, VaspInformation vasp)
        {
            MessageType = MessageType.SessionReply;
            Message = message;
            HandShake = handshake;
            VASP = vasp;
        }

        public SessionReplyMessage(string sessionId, HandShakeResponse handshake, VaspInformation vasp)
        {
            MessageType = MessageType.SessionReply;
            Message = new Message(Guid.NewGuid().ToString(), sessionId, "1");
            HandShake = handshake;
            VASP = vasp;
        }

        public HandShakeResponse HandShake { get; private set; }

        public Message Message { get; private set; }

        public VaspInformation VASP { get; private set; }

        public static string GetMessageCode(SessionReplyMessageCode messageCode)
        {
            return ((int)messageCode).ToString();
        }
        public enum SessionReplyMessageCode
        {
            SessionAccepted = 1,
            SessionDeclinedRequestNotValid = 2,
            SessionDeclinedOriginatorVaspCouldNotBeAuthenticated = 3,
            SessionDeclinedOriginatorVaspDeclined = 4,
            SessionDeclinedTemporaryDisruptionOfService = 5,
        }
    }
}
