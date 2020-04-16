using System;
using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Messaging
{
    public class WhisperMessageFormatter : IMessageFormatter
    {
        public string GetPayload(MessageBase messageBase)
        {
            switch (messageBase.MessageType)
            {
                case MessageType.SessionRequest:
                    {
                        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((SessionRequestMessage)messageBase))
                            .ToHex(true);
                    }

                case MessageType.SessionReply:
                    {
                        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((SessionReplyMessage)messageBase))
                            .ToHex(true);
                    }

                case MessageType.TransferRequest:
                {
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((TransferRequestMessage)messageBase))
                        .ToHex(true);
                }

                case MessageType.TransferReply:
                {
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((TransferReplyMessage)messageBase))
                        .ToHex(true);
                }

                case MessageType.TransferDispatch:
                {
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((TransferDispatchMessage)messageBase))
                        .ToHex(true);
                }

                case MessageType.TransferConfirmation:
                {
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((TransferConfirmationMessage)messageBase))
                        .ToHex(true);
                }

                case MessageType.Termination:
                {
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((TerminationMessage) messageBase))
                        .ToHex(true);
                }

                default:
                    throw new ArgumentException($"Message of type {messageBase.GetType()} contains enum message type {messageBase.MessageType}" +
                                                $"which is not supported");
            }
        }

        public MessageBase Deserialize(string payload)
        {
            try
            {
                payload = payload.HexToUTF8String();

                var message = JsonConvert.DeserializeObject<MessageBase>(payload);
                switch (message.MessageType)
                {
                    case MessageType.SessionRequest:
                        return JsonConvert.DeserializeObject<SessionRequestMessage>(payload);

                    case MessageType.SessionReply:
                    {
                        return JsonConvert.DeserializeObject<SessionReplyMessage>(payload);
                    }

                    case MessageType.TransferRequest:
                    {
                        return JsonConvert.DeserializeObject<TransferRequestMessage>(payload);
                    }

                    case MessageType.TransferReply:
                    {
                        return JsonConvert.DeserializeObject<TransferReplyMessage>(payload);
                    }

                    case MessageType.TransferDispatch:
                    {
                        return JsonConvert.DeserializeObject<TransferDispatchMessage>(payload);
                    }

                    case MessageType.TransferConfirmation:
                    {
                        return JsonConvert.DeserializeObject<TransferConfirmationMessage>(payload);
                    }

                    case MessageType.Termination:
                    {
                        return JsonConvert.DeserializeObject<TerminationMessage>(payload);
                    }

                    default:

                        //TODO: Probably log it
                        break;
                }

                return message;
            }
            catch (Exception e)
            {
                var a = e;
                throw;
            }
        }
    }
}