using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtocolMessages.Messages;

namespace OpenVASP.ProtoMappers.Mappers
{
    public static class SessionRequestMessageMapper
    {
        #region TO_PROTO
        public static ProtoSessionRequestMessage MapToProto(SessionRequestMessage message)
        {
            var proto = new ProtoSessionRequestMessage()
            {
                Comment = message.Comment,
                EcdshPubKey = message.HandShake.EcdhPubKey,
                Message = Mapper.MapMessageToProto(message.MessageType, message.Message),
                TopicA = message.HandShake.TopicA,
                VaspInfo = Mapper.MapVaspInformationToProto(message.VASP),
            };

            return proto;
        }

        #endregion TO_PROTO

        #region FROM_PROTO

        public static SessionRequestMessage MapFromProto(ProtoSessionRequestMessage message)
        {
            var messageIn = new Message(
                message.Message.MessageId,
                message.Message.SessionId,
                message.Message.MessageCode);
            var handshake = new HandShakeRequest(message.TopicA, message.EcdshPubKey);
            var vasp = Mapper.MapVaspInformationFromProto(message.VaspInfo);

            var proto = new SessionRequestMessage(messageIn, handshake, vasp)
            {
                Comment = message.Comment,
            };

            return proto;
        }

        #endregion FROM_PROTO
    }
}
