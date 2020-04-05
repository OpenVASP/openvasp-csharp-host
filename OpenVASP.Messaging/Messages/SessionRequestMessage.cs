using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Messaging.Messages
{
    public class SessionRequestMessage : MessageBase
    {
        public SessionRequestMessage(Message message, HandShakeRequest handshake, VaspInformation vasp)
        {
            MessageType = MessageType.SessionRequest;
            Message = message;
            HandShake = handshake;
            VASP = vasp;
        }

        public SessionRequestMessage(string sessionId, HandShakeRequest handshake, VaspInformation vasp)
        {
            MessageType = MessageType.SessionRequest;
            Message = new Message(Guid.NewGuid().ToString(), sessionId, "1");
            HandShake = handshake;
            VASP = vasp;
        }

        public HandShakeRequest HandShake { get; private set; }

        public Message Message { get; private set; }

        public VaspInformation VASP { get; private set; }
    }
}
