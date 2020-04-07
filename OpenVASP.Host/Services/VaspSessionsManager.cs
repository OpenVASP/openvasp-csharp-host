using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using OpenVASP.Host.Core.Services;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Services
{
    public class VaspSessionsManager
    {
        private readonly VaspClient _vaspClient;
        private readonly IOriginatorVaspCallbacks _originatorVaspCallbacks;
        private readonly ConcurrentDictionary<string, OriginatorSession> _originatorSessions =
            new ConcurrentDictionary<string, OriginatorSession>();
        private readonly ConcurrentDictionary<string, BeneficiarySession> _beneficiarySessions =
            new ConcurrentDictionary<string, BeneficiarySession>();

        public VaspSessionsManager(
            VaspClient vaspClient,
            IVaspCallbacks vaspCallbacks)
        {
            _vaspClient = vaspClient;

            _originatorVaspCallbacks = new OriginatorVaspCallbacks(
                async (message, originatorSession) =>
                {
                    await vaspCallbacks.SessionReplyMessageReceivedAsync(originatorSession.SessionId, message);
                },
                async (message, originatorSession) =>
                {
                    await vaspCallbacks.TransferReplyMessageReceivedAsync(originatorSession.SessionId, message);
                    if (message.Message.MessageCode == "5") //todo: handle properly.
                    {
                        await originatorSession.TerminateAsync(TerminationMessage.TerminationMessageCode.SessionClosedTransferOccured);
                        originatorSession.Wait();
                    }
                },
                async (message, originatorSession) =>
                {
                    await vaspCallbacks.TransferConfirmationMessageReceivedAsync(originatorSession.SessionId, message);
                    await originatorSession.TerminateAsync(TerminationMessage.TerminationMessageCode.SessionClosedTransferOccured);
                    originatorSession.Wait();
                });
            
            IVaspMessageHandler vaspMessageHandler = new VaspMessageHandlerCallbacks(
                vaspInfo => Task.FromResult(true),
                async (request, currentSession) =>
                {
                    _beneficiarySessions[currentSession.SessionId] = currentSession as BeneficiarySession;
                    await vaspCallbacks.TransferRequestMessageReceivedAsync(currentSession.SessionId, request);
                    return null;
                },
                async (dispatch, currentSession) =>
                {
                    await vaspCallbacks.TransferDispatchMessageReceivedAsync(currentSession.SessionId, dispatch);
                    return null;
                });
            
            _vaspClient.RunListener(vaspMessageHandler);
        }
        
        public async Task<string> CreateSessionAsync(Originator originator, VirtualAssetsAccountNumber beneficiaryVaan)
        {
            var originatorSession = await _vaspClient.CreateSessionAsync(originator, beneficiaryVaan, _originatorVaspCallbacks);

            _originatorSessions[originatorSession.SessionId] = originatorSession;

            return originatorSession.SessionId;
        }

        public async Task TransferRequestAsync(string sessionId, string beneficiaryName, VirtualAssetType type, decimal amount)
        {
            await _originatorSessions[sessionId]
                .TransferRequestAsync(
                    new TransferInstruction
                    {
                        VirtualAssetTransfer = new VirtualAssetTransfer
                        {
                            TransferType = TransferType.BlockchainTransfer,
                            VirtualAssetType = type,
                            TransferAmount = amount.ToString(CultureInfo.InvariantCulture)
                        },
                        BeneficiaryName = beneficiaryName
                    });
        }

        public async Task TransferReplyAsync(string sessionId, TransferReplyMessage message)
        {
            await _beneficiarySessions[sessionId].SendTransferReplyMessageAsync(message);
        }

        public async Task TransferDispatchAsync(string sessionId, TransferReply transferReply, string transactionHash, string sendingAddress)
        {
            await _originatorSessions[sessionId]
                .TransferDispatchAsync(
                    transferReply,
                    new Transaction(
                        transactionHash,
                        DateTime.UtcNow, 
                        sendingAddress));
        }

        public async Task TransferConfirmAsync(string sessionId, TransferConfirmationMessage message)
        {
            await _beneficiarySessions[sessionId].SendTransferConfirmationMessageAsync(message);
        }
    }
}