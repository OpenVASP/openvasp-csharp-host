using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Core.Services;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Events;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using Transaction = OpenVASP.Host.Core.Models.Transaction;

namespace OpenVASP.Host.Services
{
    /// <summary>
    /// Transactions manager
    /// </summary>
    public class TransactionsManager : ITransactionsManager
    {
        private readonly ConcurrentDictionary<string, OriginatorSession> _originatorSessionsDict =
            new ConcurrentDictionary<string, OriginatorSession>();
        private readonly ConcurrentDictionary<string, BeneficiarySession> _benefeciarySessionsDict =
            new ConcurrentDictionary<string, BeneficiarySession>();
        private readonly ConcurrentDictionary<string, Transaction> _incomingTransactionsDict =
            new ConcurrentDictionary<string, Transaction>();
        private readonly ConcurrentDictionary<string, Transaction> _outgoingTransactionsDict = 
            new ConcurrentDictionary<string, Transaction>();
        private readonly VaspClient _vaspClient;
        private readonly VaspInformation _vaspInfo;
        private readonly ITransactionDataService _transactionDataService;

        /// <summary>
        /// C-tor
        /// </summary>
        public TransactionsManager(
            VaspInformation vaspInfo,
            VaspCode vaspCode,
            string handshakePrivateKeyHex,
            string signaturePrivateKeyHex,
            IEthereumRpc ethereumRpc,
            IWhisperRpc whisperRpc,
            ISignService signService,
            IEnsProvider ensProvider,
            ITransportClient transportClient,
            ITransactionDataService transactionDataService,
            ILoggerFactory loggerFactory)
        {
            _vaspInfo = vaspInfo;
            _vaspClient = VaspClient.Create(
                vaspCode,
                handshakePrivateKeyHex,
                signaturePrivateKeyHex,
                ethereumRpc,
                ensProvider,
                signService,
                transportClient,
                loggerFactory);
            _vaspClient.SessionRequestMessageReceived += SessionRequestMessageReceivedAsync;
            _vaspClient.SessionReplyMessageReceived += SessionReplyMessageReceivedAsync;
            _vaspClient.TransferReplyMessageReceived += TransferReplyMessageReceivedAsync;
            _vaspClient.TransferConfirmationMessageReceived += TransferConfirmationMessageReceivedAsync;
            _vaspClient.TransferRequestMessageReceived += TransferRequestMessageReceivedAsync;
            _vaspClient.TransferDispatchMessageReceived += TransferDispatchMessageReceivedAsync;

            _transactionDataService = transactionDataService;
        }

        public async Task<Transaction> RegisterOutgoingTransactionAsync(
            Transaction transaction,
            Originator originator,
            VirtualAssetsAccountNumber virtualAssetsAccountNumber)
        {
            transaction.Status = TransactionStatus.SessionRequested;

            var originatorSession = await _vaspClient.CreateOriginatorSessionAsync(virtualAssetsAccountNumber.VaspCode);
            _originatorSessionsDict.TryAdd(originatorSession.Id, originatorSession);

            transaction.SessionId = originatorSession.Id;

            _outgoingTransactionsDict.TryAdd(transaction.Id, transaction);

            return transaction;
        }

        public async Task SendSessionReplyAsync(string id, SessionReplyMessage.SessionReplyMessageCode code)
        {
            if (!_incomingTransactionsDict.TryGetValue(id, out var transaction))
                return; //todo: handle this case.

            if (transaction.Status != TransactionStatus.SessionRequested)
                return; //todo: handle properly

            if (!_benefeciarySessionsDict.TryGetValue(transaction.SessionId, out var beneficiarySession))
                return; //todo: handle this case.

            await beneficiarySession.SessionReplyAsync(_vaspInfo, code);

            if (code == SessionReplyMessage.SessionReplyMessageCode.SessionAccepted)
            {
                transaction.Status = TransactionStatus.SessionConfirmed;
            }
            else
            {
                transaction.Status = TransactionStatus.SessionDeclined;
                transaction.SessionDeclineCode = SessionReplyMessage.GetMessageCode(code);
            }
        }

        public async Task SendTransferDispatchAsync(
            string id,
            string sendingAddress,
            string transactionHash)
        {
            if (!_outgoingTransactionsDict.TryGetValue(id, out var transaction))
                return; //todo: handle this case.

            if (transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            if (!_originatorSessionsDict.TryGetValue(transaction.SessionId, out var originatorSession))
                return; //todo: handle this case.

            await originatorSession.TransferDispatchAsync(transactionHash, sendingAddress);

            transaction.TransactionHash = transactionHash;
            transaction.SendingAddress = sendingAddress;
            transaction.Status = TransactionStatus.TransferDispatched;
        }

        public async Task SendTransferConfirmAsync(string id)
        {
            if (!_incomingTransactionsDict.TryGetValue(id, out var transaction))
                return; //todo: handle this case.

            if (transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            if (!_benefeciarySessionsDict.TryGetValue(transaction.SessionId, out var beneficiarySession))
                return; //todo: handle this case.

            await beneficiarySession.SendTransferConfirmationMessageAsync(
                TransferConfirmationMessage.Create(
                    transaction.SessionId,
                    TransferConfirmationMessage.TransferConfirmationMessageCode.TransferConfirmed));

            transaction.Status = TransactionStatus.TransferConfirmed;
        }

        public async Task SendTransferReplyAsync(
            string id,
            string destinationAddress,
            TransferReplyMessage.TransferReplyMessageCode code)
        {
            if (!_incomingTransactionsDict.TryGetValue(id, out var transaction))
                return; //todo: handle this case.

            if (transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.

            if (!_benefeciarySessionsDict.TryGetValue(transaction.SessionId, out var beneficiarySession))
                return; //todo: handle this case.

            await beneficiarySession.SendTransferReplyMessageAsync(
                TransferReplyMessage.Create(
                    transaction.SessionId,
                    code,
                    destinationAddress));

            if (code == TransferReplyMessage.TransferReplyMessageCode.TransferAccepted)
            {
                transaction.Status = TransactionStatus.TransferAllowed;
                transaction.DestinationAddress = destinationAddress;
            }
            else
            {
                transaction.Status = TransactionStatus.TransferForbidden;
                transaction.TransferDeclineCode = TransferReplyMessage.GetMessageCode(code);
            }
        }

        public Task<List<Transaction>> GetOutgoingTransactionsAsync()
        {
            return Task.FromResult(_outgoingTransactionsDict.Values.ToList());
        }

        public Task<List<Transaction>> GetIncomingTransactionsAsync()
        {
            return Task.FromResult(_incomingTransactionsDict.Values.ToList());
        }

        private Task SessionRequestMessageReceivedAsync(SessionMessageEvent<SessionRequestMessage> evt)
        {
            var transaction = new Transaction
            {
                Status = TransactionStatus.SessionRequested,
                Id = Guid.NewGuid().ToString(),
                SessionId = evt.SessionId,
                CreationDateTime = DateTime.UtcNow
            };

            _incomingTransactionsDict.TryAdd(transaction.Id, transaction);

            return Task.CompletedTask;
        }

        private async Task SessionReplyMessageReceivedAsync(SessionMessageEvent<SessionReplyMessage> evt)
        {
            if (!_outgoingTransactionsDict.TryGetValue(evt.SessionId, out var transaction))
                return; //todo: handle this case.

            if (!_originatorSessionsDict.TryGetValue(transaction.SessionId, out var originatorSession))
                return; //todo: handle this case.

            if (evt.Message.Message.MessageCode ==
                SessionReplyMessage.GetMessageCode(SessionReplyMessage.SessionReplyMessageCode.SessionAccepted))
            {
                transaction.Status = TransactionStatus.SessionConfirmed;

                await originatorSession.TransferRequestAsync(
                    _transactionDataService.GetOriginatorFromTx(transaction),
                    _transactionDataService.GetBeneficiaryFromTx(transaction),
                    transaction.Asset,
                    transaction.Amount);

                transaction.Status = TransactionStatus.TransferRequested;
            }
            else
            {
                transaction.Status = TransactionStatus.SessionDeclined;
                transaction.SessionDeclineCode = evt.Message.Message.MessageCode;
            }
        }

        private Task TransferReplyMessageReceivedAsync(SessionMessageEvent<TransferReplyMessage> evt)
        {
            if (!_outgoingTransactionsDict.TryGetValue(evt.SessionId, out var transaction))
                return Task.CompletedTask; //todo: handle this case.

            if (transaction.Status != TransactionStatus.TransferRequested)
                return Task.CompletedTask; //todo: handle this case.

            if (evt.Message.Message.MessageCode == TransferReplyMessage.GetMessageCode(TransferReplyMessage.TransferReplyMessageCode.TransferAccepted))
            {
                transaction.Status = TransactionStatus.TransferAllowed;
                transaction.DestinationAddress = evt.Message.DestinationAddress;
            }
            else
            {
                transaction.Status = TransactionStatus.TransferForbidden;
                transaction.TransferDeclineCode = evt.Message.Message.MessageCode;
            }

            return Task.CompletedTask;
        }

        private Task TransferConfirmationMessageReceivedAsync(SessionMessageEvent<TransferConfirmationMessage> evt)
        {
            if (!_outgoingTransactionsDict.TryGetValue(evt.SessionId, out var transaction))
                return Task.CompletedTask; //todo: handle this case.

            if (transaction.Status != TransactionStatus.TransferDispatched)
                return Task.CompletedTask; //todo: handle this case.

            transaction.Status = TransactionStatus.TransferConfirmed;

            return Task.CompletedTask;
        }

        private Task TransferRequestMessageReceivedAsync(SessionMessageEvent<TransferRequestMessage> evt)
        {
            if (!_incomingTransactionsDict.TryGetValue(evt.SessionId, out var transaction))
                return Task.CompletedTask; //todo: handle this case.

            transaction.Status = TransactionStatus.TransferRequested;
            _transactionDataService.FillTransactionData(transaction, evt.Message);

            return Task.CompletedTask;
        }

        private Task TransferDispatchMessageReceivedAsync(SessionMessageEvent<TransferDispatchMessage> evt)
        {
            if (!_incomingTransactionsDict.TryGetValue(evt.SessionId, out var transaction))
                return Task.CompletedTask; //todo: handle this case.

            if (transaction.Status != TransactionStatus.TransferAllowed)
                return Task.CompletedTask; //todo: handle this case.

            transaction.TransactionHash = evt.Message.Transaction.TransactionId;
            transaction.SendingAddress = evt.Message.Transaction.SendingAddress;
            transaction.Status = TransactionStatus.TransferDispatched;

            return Task.CompletedTask;
        }
    }
}