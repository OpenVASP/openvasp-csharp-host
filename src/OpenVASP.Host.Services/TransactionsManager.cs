using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Events;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Core.Repositories;
using OpenVASP.Host.Core.Services;
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
        private readonly IVaspClient _vaspClient;
        private readonly ITransactionDataService _transactionDataService;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IVaspCodeManager _vaspCodeManager;
        private readonly VaspInformation _vaspInformation;

        /// <summary>
        /// C-tor
        /// </summary>
        public TransactionsManager(
            VaspCode vaspCode,
            string handshakePrivateKeyHex,
            string signaturePrivateKeyHex,
            IEthereumRpc ethereumRpc,
            ISignService signService,
            IEnsProvider ensProvider,
            ITransportClient transportClient,
            ITransactionDataService transactionDataService,
            ISessionsRepository sessionsRepository,
            ITransactionsRepository transactionsRepository,
            IVaspCodeManager vaspCodeManager,
            VaspInformation vaspInformation,
            ILoggerFactory loggerFactory)
        {
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
            _vaspClient.BeneficiarySessionCreated += BeneficiarySessionCreatedAsync;

            _transactionDataService = transactionDataService;
            _sessionsRepository = sessionsRepository;
            _transactionsRepository = transactionsRepository;
            _vaspCodeManager = vaspCodeManager;
            _vaspInformation = vaspInformation;
        }

        public async Task<Transaction> RegisterOutgoingTransactionAsync(
            Transaction transaction,
            VirtualAssetsAccountNumber virtualAssetsAccountNumber)
        {
            transaction.Status = TransactionStatus.Created;

            var originatorSession = await _vaspClient.CreateOriginatorSessionAsync(virtualAssetsAccountNumber.VaspCode);

            _originatorSessionsDict.TryAdd(originatorSession.Id, originatorSession);

            transaction.SessionId = originatorSession.Id;

            await _transactionsRepository.CreateAsync(transaction);

            await _sessionsRepository.CreateAsync(originatorSession.SessionInfo);

            await originatorSession.SessionRequestAsync(_vaspInformation);

            transaction.Status = TransactionStatus.SessionRequested;

            await _transactionsRepository.UpdateAsync(transaction);

            return transaction;
        }

        public async Task SendSessionReplyAsync(string txId, SessionReplyMessage.SessionReplyMessageCode code)
        {
            var transaction = await _transactionsRepository.GetAsync(txId);

            if (transaction == null)
            {
                throw new NullReferenceException();
            }

            if (transaction.Status != TransactionStatus.SessionRequested)
                return; //todo: handle properly

            if (!_benefeciarySessionsDict.TryGetValue(transaction.SessionId, out var beneficiarySession))
                return; //todo: handle this case.

            await beneficiarySession.SessionReplyAsync(_vaspInformation, code);

            if (code == SessionReplyMessage.SessionReplyMessageCode.SessionAccepted)
            {
                transaction.Status = TransactionStatus.SessionConfirmed;
            }
            else
            {
                transaction.Status = TransactionStatus.SessionDeclined;
                transaction.SessionDeclineCode = SessionReplyMessage.GetMessageCode(code);
            }

            await _transactionsRepository.UpdateAsync(transaction);
        }

        public async Task SendTransferReplyAsync(
            string id,
            string destinationAddress,
            TransferReplyMessage.TransferReplyMessageCode code)
        {
            var transaction = await _transactionsRepository.GetAsync(id);

            if (transaction == null)
            {
                throw new NullReferenceException();
            }

            if (transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.

            if (!_benefeciarySessionsDict.TryGetValue(transaction.SessionId, out var beneficiarySession))
                return; //todo: handle this case.

            await beneficiarySession.TransferReplyAsync(
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

            await _transactionsRepository.UpdateAsync(transaction);
        }

        public async Task SendTransferDispatchAsync(
            string id,
            string sendingAddress,
            string transactionHash,
            DateTime transactionDateTime)
        {
            var transaction = await _transactionsRepository.GetAsync(id);

            if (transaction == null)
            {
                throw new NullReferenceException();
            }

            if (transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            if (!_originatorSessionsDict.TryGetValue(transaction.SessionId, out var originatorSession))
                return; //todo: handle this case.

            await originatorSession.TransferDispatchAsync(transactionHash, sendingAddress, transactionDateTime);

            transaction.TransactionHash = transactionHash;
            transaction.SendingAddress = sendingAddress;
            transaction.TransactionDateTime = transactionDateTime;
            transaction.Status = TransactionStatus.TransferDispatched;

            await _transactionsRepository.UpdateAsync(transaction);
        }

        public async Task SendTransferConfirmAsync(string id)
        {
            var transaction = await _transactionsRepository.GetAsync(id);

            if (transaction == null)
            {
                throw new NullReferenceException();
            }

            if (transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            if (!_benefeciarySessionsDict.TryGetValue(transaction.SessionId, out var beneficiarySession))
                return; //todo: handle this case.

            await beneficiarySession.TransferConfirmAsync(
                TransferConfirmationMessage.Create(
                    transaction.SessionId,
                    TransferConfirmationMessage.TransferConfirmationMessageCode.TransferConfirmed));

            transaction.Status = TransactionStatus.TransferConfirmed;

            await _transactionsRepository.UpdateAsync(transaction);
        }

        public async Task<List<Transaction>> GetOutgoingTransactionsAsync()
        {
            return await _transactionsRepository.GetAsync(TransactionType.Outgoing);
        }

        public async Task<List<Transaction>> GetIncomingTransactionsAsync()
        {
            return await _transactionsRepository.GetAsync(TransactionType.Incoming);
        }

        public async Task<Transaction> GetAsync(string id)
        {
            return await _transactionsRepository.GetAsync(id);
        }

        private async Task BeneficiarySessionCreatedAsync(BeneficiarySessionCreatedEvent evt)
        {
            var transaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Status = TransactionStatus.Created,
                SessionId = evt.SessionId,
                CreationDateTime = DateTime.UtcNow,
                Type = TransactionType.Incoming
            };

            await _sessionsRepository.CreateAsync(evt.Session.SessionInfo);

            _benefeciarySessionsDict[evt.Session.SessionInfo.Id] = evt.Session;

            await _transactionsRepository.CreateAsync(transaction);
        }

        private async Task SessionRequestMessageReceivedAsync(SessionMessageEvent<SessionRequestMessage> evt)
        {
            var transaction = await _transactionsRepository.GetBySessionIdAsync(evt.SessionId);

            if (transaction == null)
                return; //todo: handle this case.

            transaction.CounterPartyVasp = evt.Message.Vasp;
            transaction.Status = TransactionStatus.SessionRequested;

            await _transactionsRepository.UpdateAsync(transaction);

            if (_vaspCodeManager.IsAutoConfirmedVaspCode(transaction.CounterPartyVasp.GetVaspCode()))
                await SendSessionReplyAsync(transaction.Id, SessionReplyMessage.SessionReplyMessageCode.SessionAccepted);
        }

        private async Task SessionReplyMessageReceivedAsync(SessionMessageEvent<SessionReplyMessage> evt)
        {
            var transaction = await _transactionsRepository.GetBySessionIdAsync(evt.SessionId);

            if (!_originatorSessionsDict.TryGetValue(transaction.SessionId, out var originatorSession))
                return; //todo: handle this case.

            transaction.CounterPartyVasp = evt.Message.Vasp;

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

            await _transactionsRepository.UpdateAsync(transaction);
        }

        private async Task TransferRequestMessageReceivedAsync(SessionMessageEvent<TransferRequestMessage> evt)
        {
            var transaction = await _transactionsRepository.GetBySessionIdAsync(evt.SessionId);

            if (transaction == null || transaction.Status != TransactionStatus.SessionConfirmed)
                return; //todo: handle this case.

            transaction.Status = TransactionStatus.TransferRequested;
            _transactionDataService.FillTransactionData(transaction, evt.Message);

            await _transactionsRepository.UpdateAsync(transaction);
        }

        private async Task TransferReplyMessageReceivedAsync(SessionMessageEvent<TransferReplyMessage> evt)
        {
            var transaction = await _transactionsRepository.GetBySessionIdAsync(evt.SessionId);

            if (transaction == null || transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.

            if (evt.Message.Message.MessageCode == TransferReplyMessage.GetMessageCode(TransferReplyMessage.TransferReplyMessageCode.TransferAccepted))
            {
                transaction.Status = TransactionStatus.TransferAllowed;
                transaction.DestinationAddress = evt.Message.DestinationAddress;

                await _transactionsRepository.UpdateAsync(transaction);
            }
            else
            {
                transaction.Status = TransactionStatus.TransferForbidden;
                transaction.TransferDeclineCode = evt.Message.Message.MessageCode;
            }

            await _transactionsRepository.UpdateAsync(transaction);
        }

        private async Task TransferDispatchMessageReceivedAsync(SessionMessageEvent<TransferDispatchMessage> evt)
        {
            var transaction = await _transactionsRepository.GetBySessionIdAsync(evt.SessionId);

            if (transaction == null || transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            transaction.TransactionHash = evt.Message.Transaction.TransactionId;
            transaction.SendingAddress = evt.Message.Transaction.SendingAddress;
            transaction.TransactionDateTime = evt.Message.Transaction.DateTime;
            transaction.Status = TransactionStatus.TransferDispatched;

            await _transactionsRepository.UpdateAsync(transaction);
        }

        private async Task TransferConfirmationMessageReceivedAsync(SessionMessageEvent<TransferConfirmationMessage> evt)
        {
            var transaction = await _transactionsRepository.GetBySessionIdAsync(evt.SessionId);

            if (transaction == null || transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            transaction.Status = TransactionStatus.TransferConfirmed;

            await _transactionsRepository.UpdateAsync(transaction);

            if (!_originatorSessionsDict.TryGetValue(transaction.SessionId, out var originatorSession))
                return; //todo: handle this case.

            await originatorSession.TerminateAsync(TerminationMessage.TerminationMessageCode.SessionClosedTransferOccured);

            await _transactionsRepository.UpdateAsync(transaction);
        }
    }
}