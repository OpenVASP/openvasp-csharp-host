using System;
using Google.Protobuf;
using Nethereum.Hex.HexConvertors.Extensions;
using OpenVASP.Messaging;
using OpenVASP.Messaging.Messages;
using OpenVASP.ProtocolMessages.Messages;
using OpenVASP.ProtoMappers.Mappers;

namespace OpenVASP.ProtoMappers
{
    public class WhisperMessageFormatter : IMessageFormatter
    {
        public string GetPayload(MessageBase messageBase)
        {
            var wrapper = new ProtoMessageWrapper();

            switch (messageBase.MessageType)
            {
                case MessageType.SessionRequest:
                    {
                        var proto = SessionRequestMessageMapper.MapToProto((SessionRequestMessage)messageBase);
                        wrapper.SessionRequestMessage = proto;

                        break;
                    }

                case MessageType.SessionReply:
                    {
                        var proto = SessionReplyMessageMapper.MapToProto((SessionReplyMessage)messageBase);
                        wrapper.SessionReplyMessage = proto;

                        break;
                    }

                case MessageType.TransferRequest:
                {
                    var proto = TransferRequestMessageMapper.MapToProto((TransferRequestMessage)messageBase);
                    wrapper.TransferRequestMessage = proto;

                    break;
                }

                case MessageType.TransferReply:
                {
                    var proto = TransferReplyMessageMapper.MapToProto((TransferReplyMessage)messageBase);
                    wrapper.TransferReplyMessage = proto;

                    break;
                }

                case MessageType.TransferDispatch:
                {
                    var proto = TransferDispatchMessageMapper.MapToProto((TransferDispatchMessage)messageBase);
                    wrapper.TransferDispatchMessage = proto;

                    break;
                }

                case MessageType.TransferConfirmation:
                {
                    var proto = TransferConfirmationMessageMapper.MapToProto((TransferConfirmationMessage)messageBase);
                    wrapper.TransaferConfirmationMessage = proto;

                    break;
                }

                case MessageType.Termination:
                {
                    var proto = TerminationMessageMapper.MapToProto((TerminationMessage)messageBase);
                    wrapper.TerminationMessage = proto;

                    break;
                }

                default:
                    throw new ArgumentException($"Message of type {messageBase.GetType()} contains enum message type {messageBase.MessageType}" +
                                                $"which is not supported");
            }

            var payload = wrapper.ToByteArray().ToHex(prefix: true);

            return payload;
        }

        public MessageBase Deserialize(string payload)
        {
            var bytes = payload.HexToByteArray();
            var wrapper = ProtoMessageWrapper.Parser.ParseFrom(bytes);
            MessageBase message = null;
            switch (wrapper.MsgCase)
            {
                case ProtoMessageWrapper.MsgOneofCase.SessionRequestMessage:
                    {
                        message = SessionRequestMessageMapper.MapFromProto(wrapper.SessionRequestMessage);

                        break;
                    }

                case ProtoMessageWrapper.MsgOneofCase.SessionReplyMessage:
                    {
                        message = SessionReplyMessageMapper.MapFromProto(wrapper.SessionReplyMessage);

                        break;
                    }

                case ProtoMessageWrapper.MsgOneofCase.TransferRequestMessage:
                {
                    message = TransferRequestMessageMapper.MapFromProto(wrapper.TransferRequestMessage);

                    break;
                }

                case ProtoMessageWrapper.MsgOneofCase.TransferReplyMessage:
                {
                    message = TransferReplyMessageMapper.MapFromProto(wrapper.TransferReplyMessage);

                    break;
                }

                case ProtoMessageWrapper.MsgOneofCase.TransferDispatchMessage:
                {
                    message = TransferDispatchMessageMapper.MapFromProto(wrapper.TransferDispatchMessage);

                    break;
                }

                case ProtoMessageWrapper.MsgOneofCase.TransaferConfirmationMessage:
                {
                    message = TransferConfirmationMessageMapper.MapFromProto(wrapper.TransaferConfirmationMessage);

                    break;
                }

                case ProtoMessageWrapper.MsgOneofCase.TerminationMessage:
                {
                    message = TerminationMessageMapper.MapFromProto(wrapper.TerminationMessage);

                    break;
                }

                default:

                    //TODO: Probably log it
                    break;
            }

            return message;
        }
    }
}