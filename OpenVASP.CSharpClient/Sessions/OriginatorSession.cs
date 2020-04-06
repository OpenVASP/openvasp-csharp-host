using System;
using System.Threading.Tasks;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.Messaging;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.Messaging.MessagingEngine;
using OpenVASP.Tests.Client.Sessions;

namespace OpenVASP.CSharpClient.Sessions
{
    public class OriginatorSession : VaspSession
    {
        private readonly VirtualAssetssAccountNumber _beneficiaryVaan;
        private readonly string _beneficiaryPubHandshakeKey;
        private readonly string _pubEncryptionKey;
        private readonly Originator _originator;

        private readonly TaskCompletionSource<SessionReplyMessage> _sessionReplyCompletionSource = new TaskCompletionSource<SessionReplyMessage>();
        private readonly TaskCompletionSource<TransferReplyMessage> _transferReplyCompletionSource = new TaskCompletionSource<TransferReplyMessage>();
        private readonly TaskCompletionSource<TransferConfirmationMessage> _transferConfirmationCompletionSource = new TaskCompletionSource<TransferConfirmationMessage>();


        public OriginatorSession(
            Originator originator,
            VaspContractInfo originatorVaspContractInfo,
            VaspInformation originatorVasp,
            VirtualAssetssAccountNumber beneficiaryVaan,
            string beneficiaryPubSigningKey,
            string beneficiaryPubHandshakeKey,
            string sharedEncryptionKey,
            string pubEncryptionKey,
            string privateSigningKey,
            IWhisperRpc whisperRpc,
            ITransportClient transportClient,
            ISignService signService)
            //IEnsProvider ensProvider)
            : base(
                originatorVaspContractInfo,
                originatorVasp,
                beneficiaryPubSigningKey,
                sharedEncryptionKey,
                privateSigningKey,
                whisperRpc,
                transportClient,
                signService)
        {
            this._beneficiaryVaan = beneficiaryVaan;
            this.SessionId = Guid.NewGuid().ToString();
            this._beneficiaryPubHandshakeKey = beneficiaryPubHandshakeKey;
            this._pubEncryptionKey = pubEncryptionKey;
            //this._ensProvider = ensProvider;
            this._originator = originator;

            _messageHandlerResolverBuilder.AddHandler(typeof(SessionReplyMessage),
                new SessionReplyMessageHandler((sessionReplyMessage, token) =>
                {
                    if (_sessionReplyCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
                    {
                        this.CounterPartyTopic = sessionReplyMessage.HandShake.TopicB;
                        _sessionReplyCompletionSource.SetResult(sessionReplyMessage);
                    }

                    return Task.CompletedTask;
                }));

            _messageHandlerResolverBuilder.AddHandler(typeof(TransferReplyMessage),
                new TransferReplyMessageHandler((transferReplyMessage, token) =>
                {
                    if (_transferReplyCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
                    {
                        _transferReplyCompletionSource.TrySetResult(transferReplyMessage);
                    }

                    return Task.CompletedTask;
                }));

            _messageHandlerResolverBuilder.AddHandler(typeof(TransferConfirmationMessage),
                new TransferConfirmationMessageHandler((transferDispatchMessage, token) =>
                {
                    if (_transferConfirmationCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
                    {
                        _transferConfirmationCompletionSource.TrySetResult(transferDispatchMessage);
                    }

                    return Task.CompletedTask;
                }));

            _messageHandlerResolverBuilder.AddHandler(typeof(TerminationMessage),
                new TerminationMessageHandler(async (message, token) =>
                {
                    _hasReceivedTerminationMessage = true;

                    await TerminateAsync(message.GetMessageCode());
                }));
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();

            //string beneficiaryVaspContractAddress = await _ensProvider.GetContractAddressByVaspCodeAsync(_beneficiaryVaan.VaspCode);
            //await _ethereumRpc.GetVaspContractInfoAync()

            var sessionRequestMessage = new SessionRequestMessage(
                this.SessionId,
                new HandShakeRequest(this._sessionTopic, this._pubEncryptionKey),
                this._vaspInfo);

            await _transportClient.SendAsync(new MessageEnvelope()
            {
                Topic = _beneficiaryVaan.VaspCode.Code,
                SigningKey = _privateSigningKey,
                EncryptionType = EncryptionType.Assymetric,
                EncryptionKey = _beneficiaryPubHandshakeKey
            }, sessionRequestMessage);

            await _sessionReplyCompletionSource.Task;
        }

        public async Task<TransferReplyMessage> TransferRequestAsync(TransferInstruction instruction)
        {
            if (_transferReplyCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
            {
                var transferRequest = new TransferRequestMessage(
                    this.SessionId,
                    _originator,
                    new Beneficiary("", _beneficiaryVaan.Vaan),
                    new TransferRequest(
                        instruction.VirtualAssetTransfer.VirtualAssetType,
                        instruction.VirtualAssetTransfer.TransferType,
                        instruction.VirtualAssetTransfer.TransferAmount),
                    _vaspInfo
                );

                await _transportClient.SendAsync(new MessageEnvelope()
                {
                    Topic = this.CounterPartyTopic,
                    SigningKey = _privateSigningKey,
                    EncryptionType = EncryptionType.Symmetric,
                    EncryptionKey = _sharedSymKeyId
                }, transferRequest);

            }

            return await _transferReplyCompletionSource.Task;
        }

        public async Task<TransferConfirmationMessage> TransferDispatchAsync(TransferReply transferReply, Transaction transaction)
        {
            if (_transferConfirmationCompletionSource.Task.Status == TaskStatus.WaitingForActivation)
            {
                var transferRequest = new TransferDispatchMessage(
                    this.SessionId,
                    this._originator,
                    new Beneficiary("", _beneficiaryVaan.Vaan),
                    transferReply,
                    transaction,
                    _vaspInfo
                );

                await _transportClient.SendAsync(new MessageEnvelope()
                {
                    Topic = this.CounterPartyTopic,
                    SigningKey = _privateSigningKey,
                    EncryptionType = EncryptionType.Symmetric,
                    EncryptionKey = _sharedSymKeyId
                }, transferRequest);
            }

            return await _transferConfirmationCompletionSource.Task;
        }
    }
}