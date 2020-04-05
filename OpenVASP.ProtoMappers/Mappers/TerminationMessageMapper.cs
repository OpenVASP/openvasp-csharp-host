using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtocolMessages.Messages;

namespace OpenVASP.ProtoMappers.Mappers
{
    public static class TerminationMessageMapper
    {
        #region TO_PROTO
        public static ProtoTerminationMessage MapToProto(TerminationMessage message)
        {
            var proto = new ProtoTerminationMessage()
            {
                Comment = message.Comment,
                Message = Mapper.MapMessageToProto(message.MessageType, message.Message),
                VaspInfo = Mapper.MapVaspInformationToProto(message.VASP)
            };

            return proto;
        }

        #endregion TO_PROTO

        #region FROM_PROTO

        public static TerminationMessage MapFromProto(ProtoTerminationMessage message)
        {
            var messageIn = new Message(
                message.Message.MessageId,
                message.Message.SessionId,
                message.Message.MessageCode);
            var vasp = Mapper.MapVaspInformationFromProto(message.VaspInfo);

            var obj = new TerminationMessage(messageIn, vasp)
            {
                Comment = message.Comment,
            };

            return obj;
        }

        #endregion FROM_PROTO
    }
}
