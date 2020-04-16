using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class SessionReplyMessage : MessageBase
    {
        public static SessionReplyMessage Create(Message message, HandShakeResponse handshake, VaspInformation vasp)
        {
            return new SessionReplyMessage
            {
                MessageType = MessageType.SessionReply,
                Message = message,
                HandShake = handshake,
                VASP = vasp
            };
        }

        public static SessionReplyMessage Create(string sessionId, HandShakeResponse handshake, VaspInformation vasp)
        {
            return new SessionReplyMessage
            {
                MessageType = MessageType.SessionReply,
                Message = new Message(Guid.NewGuid().ToString(), sessionId, "1"),
                HandShake = handshake,
                VASP = vasp
            };
        }

        [JsonProperty("handshake")]
        public HandShakeResponse HandShake { get; private set; }

        [JsonProperty("msg")]
        public Message Message { get; private set; }

        [JsonProperty("vasp")]
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
