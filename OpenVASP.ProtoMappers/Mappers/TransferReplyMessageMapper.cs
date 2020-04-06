using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtocolMessages.Messages;

namespace OpenVASP.ProtoMappers.Mappers
{
    public static class TransferReplyMessageMapper
    {
        #region TO_PROTO
        public static ProtoTransferReplyMessage MapToProto(TransferReplyMessage message)
        {
            var proto = new ProtoTransferReplyMessage()
            {
                Comment = message.Comment,
                Transfer = Mapper.MapTransferToProto(message.Transfer),
                Beneficiary = Mapper.MapBeneficiaryToProto(message.Beneficiary),
                Originator = Mapper.MapOriginatorToProto(message.Originator),
                Message = Mapper.MapMessageToProto(message.MessageType, message.Message),
                VaspInfo = Mapper.MapVaspInformationToProto(message.VASP)
            };

            return proto;
        }

        #endregion TO_PROTO

        #region FROM_PROTO

        public static TransferReplyMessage MapFromProto(ProtoTransferReplyMessage message)
        {
            var messageIn = new Message(
                message.Message.MessageId,
                message.Message.SessionId,
                message.Message.MessageCode);
            var originator = Mapper.MapOriginatorFromProto(message.Originator);
            var beneficiary = Mapper.MapBeneficiaryFromProto(message.Beneficiary);
            var transfer = Mapper.MapTransferFromProto(message.Transfer);
            var vasp = Mapper.MapVaspInformationFromProto(message.VaspInfo);

            var obj = new TransferReplyMessage(messageIn, originator, beneficiary, transfer, vasp)
            {
                Comment = message.Comment,
            };

            return obj;
        }

        #endregion FROM_PROTO
    }
}
