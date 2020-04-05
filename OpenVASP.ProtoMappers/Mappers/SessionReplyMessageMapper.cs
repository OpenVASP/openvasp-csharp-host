using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtocolMessages.Messages;

namespace OpenVASP.ProtoMappers.Mappers
{
    public static class SessionReplyMessageMapper
    {
        #region TO_PROTO
        public static ProtoSessionReplyMessage MapToProto(SessionReplyMessage message)
        {
            var proto = new ProtoSessionReplyMessage()
            {
                Comment = message.Comment,
                TopicB = message.HandShake.TopicB,
                Message = Mapper.MapMessageToProto(message.MessageType, message.Message),
                VaspInfo = Mapper.MapVaspInformationToProto(message.VASP),
            };

            return proto;
        }

        #endregion TO_PROTO

        #region FROM_PROTO

        public static SessionReplyMessage MapFromProto(ProtoSessionReplyMessage message)
        {
            var messageIn = new Message(
                message.Message.MessageId,
                message.Message.SessionId,
                message.Message.MessageCode);
            var handshake = new HandShakeResponse(message.TopicB);
            var vasp = Mapper.MapVaspInformationFromProto(message.VaspInfo);

            var proto = new SessionReplyMessage(messageIn, handshake, vasp)
            {
                Comment = message.Comment,
            };

            return proto;
        }

        #endregion FROM_PROTO
    }
}
