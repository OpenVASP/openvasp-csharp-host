#Instruction for C# classes regeneration from proto contracts

Call protoc with listing all related proto contracts. 

```
protoc -I="D:\OpenVaspRepo\OpenVasp-CsharpClient\src\OpenVASP.ProtocolMessages\proto" --csharp_out="D:\OpenVaspRepo\OpenVasp-CsharpClient\src\OpenVASP.ProtocolMessages\Messages" ProtoJuridicalPersonId.proto ProtoNaturalPersonId.proto ProtoPlaceOfBirth.proto ProtoPostalAddress.proto ProtoMessage.proto ProtoSessionRequestMessage.proto ProtoVaspInfo.proto ProtoMessageWrapper.proto ProtoSessionReplyMessage.proto ProtoBeneficiary.proto ProtoTransferRequest.proto ProtoOriginator.proto ProtoTransferRequestMessage.proto ProtoTransferReply.proto ProtoTransferReplyMessage.proto ProtoTransaction.proto ProtoTransferDispatchMessage.proto ProtoTransferConfirmationMessage.proto ProtoTerminationMessage.proto
```