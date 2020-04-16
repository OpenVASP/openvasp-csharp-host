using System;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class SessionRequestMessage : MessageBase
    {
        public static SessionRequestMessage Create(Message message, HandShakeRequest handshake, VaspInformation vasp)
        {
            return new SessionRequestMessage
            {
                MessageType = MessageType.SessionRequest,
                Message = message,
                HandShake = handshake,
                VASP = vasp
            };
        }

        public static SessionRequestMessage Create(string sessionId, HandShakeRequest handshake, VaspInformation vasp)
        {
            return new SessionRequestMessage
            {
                MessageType = MessageType.SessionRequest,
                Message = new Message(Guid.NewGuid().ToString(), sessionId, "1"),
                HandShake = handshake,
                VASP = vasp
            };
        }

        [JsonProperty("handshake")]
        public HandShakeRequest HandShake { get; private set; }

        [JsonProperty("msg")]
        public Message Message { get; private set; }

        [JsonProperty("vasp")]
        public VaspInformation VASP { get; private set; }
    }
}
