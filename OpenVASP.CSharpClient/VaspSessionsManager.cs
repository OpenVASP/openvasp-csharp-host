using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.CSharpClient
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
                    if (message.Message.MessageCode != "1") //todo: handle properly.
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
                async (request, currentSession) =>
                {
                    _beneficiarySessions[currentSession.SessionId] = currentSession as BeneficiarySession;
                    await vaspCallbacks.SessionRequestMessageReceivedAsync(currentSession.SessionId, request);
                },
                async (request, currentSession) =>
                {
                    await vaspCallbacks.TransferRequestMessageReceivedAsync(currentSession.SessionId, request);
                },
                async (dispatch, currentSession)
                    => await vaspCallbacks.TransferDispatchMessageReceivedAsync(currentSession.SessionId, dispatch));
            
            _vaspClient.RunListener(vaspMessageHandler);
        }
        
        public async Task<string> CreateSessionAsync(Originator originator, VirtualAssetsAccountNumber beneficiaryVaan)
        {
            var originatorSession = await _vaspClient.CreateSessionAsync(originator, beneficiaryVaan, _originatorVaspCallbacks);

            _originatorSessions[originatorSession.SessionId] = originatorSession;

            return originatorSession.SessionId;
        }

        public async Task SessionReplyAsync(string sessionId, SessionReplyMessage.SessionReplyMessageCode code)
        {
            await _beneficiarySessions[sessionId]
                .StartAsync(code);
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
                            TransferAmount = amount
                        },
                        BeneficiaryName = beneficiaryName
                    });
        }

        public async Task TransferReplyAsync(string sessionId, TransferReplyMessage message)
        {
            await _beneficiarySessions[sessionId].SendTransferReplyMessageAsync(message);
        }

        public async Task TransferDispatchAsync(string sessionId, TransferReply transferReply, string transactionHash, string sendingAddress, string beneficiaryName)
        {
            await _originatorSessions[sessionId]
                .TransferDispatchAsync(
                    transferReply,
                    new Transaction(
                        transactionHash,
                        DateTime.UtcNow, 
                        sendingAddress),
                    beneficiaryName);
        }

        public async Task TransferConfirmAsync(string sessionId, TransferConfirmationMessage message)
        {
            await _beneficiarySessions[sessionId].SendTransferConfirmationMessageAsync(message);
        }
    }
}