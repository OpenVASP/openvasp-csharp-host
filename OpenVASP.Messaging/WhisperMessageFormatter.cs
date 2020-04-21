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
            switch (messageBase.Message.MessageType)
            {
                case MessageType.SessionRequest:
                {
                    var str = JsonConvert.SerializeObject((SessionRequestMessage) messageBase);
                    Console.WriteLine("------------------------------");
                    Console.WriteLine("Message: SessionRequestMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                case MessageType.SessionReply:
                {
                    var str = JsonConvert.SerializeObject((SessionReplyMessage) messageBase);
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("Message: SessionReplyMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                case MessageType.TransferRequest:
                {
                    var str = JsonConvert.SerializeObject((TransferRequestMessage) messageBase);
                    Console.WriteLine("-------------------------------");
                    Console.WriteLine("Message: TransferRequestMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                case MessageType.TransferReply:
                {
                    var str = JsonConvert.SerializeObject((TransferReplyMessage) messageBase);
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("Message: TransferReplyMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                case MessageType.TransferDispatch:
                {
                    var str = JsonConvert.SerializeObject((TransferDispatchMessage) messageBase);
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine("Message: TransferDispatchMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                case MessageType.TransferConfirmation:
                {
                    var str = JsonConvert.SerializeObject((TransferConfirmationMessage) messageBase);
                    Console.WriteLine("------------------------------------");
                    Console.WriteLine("Message: TransferConfirmationMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                case MessageType.Termination:
                {
                    var str = JsonConvert.SerializeObject((TerminationMessage) messageBase);
                    Console.WriteLine("---------------------------");
                    Console.WriteLine("Message: TerminationMessage");
                    Console.WriteLine(str);
                    return Encoding.UTF8.GetBytes(str).ToHex(true);
                }

                default:
                    throw new ArgumentException(
                        $"Message of type {messageBase.GetType()} contains enum message type {messageBase.Message.MessageType}" +
                        $"which is not supported");
            }
        }

        public MessageBase Deserialize(string payload)
        {
            payload = payload.HexToUTF8String();
            
            Console.WriteLine("-----------------");
            Console.WriteLine("Incoming Payload:");
            Console.WriteLine(payload);
            
            var message = JsonConvert.DeserializeObject<MessageBase>(payload);
            switch (message.Message.MessageType)
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
    }
}