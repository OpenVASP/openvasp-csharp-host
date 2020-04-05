using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtocolMessages.Messages;

namespace OpenVASP.ProtoMappers.Mappers
{
    public static class TransferConfirmationMessageMapper
    {
        #region TO_PROTO
        public static ProtoTransferConfirmationMessage MapToProto(TransferConfirmationMessage message)
        {
            var proto = new ProtoTransferConfirmationMessage()
            {
                Comment = message.Comment,
                Transfer = Mapper.MapTransferToProto(message.Transfer),
                Transaction = Mapper.MapTranactionToProto(message.Transaction),
                Beneficiary = Mapper.MapBeneficiaryToProto(message.Beneficiary),
                Originator = Mapper.MapOriginatorToProto(message.Originator),
                Message = Mapper.MapMessageToProto(message.MessageType, message.Message),
                VaspInfo = Mapper.MapVaspInformationToProto(message.VASP)
            };

            return proto;
        }

        #endregion TO_PROTO

        #region FROM_PROTO

        public static TransferConfirmationMessage MapFromProto(ProtoTransferConfirmationMessage message)
        {
            var messageIn = new Message(
                message.Message.MessageId,
                message.Message.SessionId,
                message.Message.MessageCode);
            var originator = Mapper.MapOriginatorFromProto(message.Originator);
            var beneficiary = Mapper.MapBeneficiaryFromProto(message.Beneficiary);
            var transfer = Mapper.MapTransferFromProto(message.Transfer);
            var transaction = Mapper.MapTranactionFromProto(message.Transaction);
            var vasp = Mapper.MapVaspInformationFromProto(message.VaspInfo);

            var obj = new TransferConfirmationMessage(messageIn, originator, beneficiary, transfer, transaction, vasp)
            {
                Comment = message.Comment,
            };

            return obj;
        }

        #endregion FROM_PROTO
    }
}
