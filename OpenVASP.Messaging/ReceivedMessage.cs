using System;

namespace OpenVASP.Messaging
{
    public class ReceivedMessage
    {
        public MessageEnvelope MessageEnvelope { get; set; }

        public string Payload { get; set; }
    }
}
