using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenVASP.Host.Core.Models;
using OpenVASP.CSharpClient;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using PlaceOfBirth = OpenVASP.Host.Core.Models.PlaceOfBirth;
using PostalAddress = OpenVASP.Host.Core.Models.PostalAddress;
using Transaction = OpenVASP.Host.Core.Models.Transaction;

namespace OpenVASP.Host.Services
{
    public class TransactionsManager : IVaspCallbacks
    {
        private readonly List<Transaction> _outgoingTransactions;
        private readonly List<Transaction> _incomingTransactions;
        private readonly VaspSessionsManager _vaspSessionsManager;
        private readonly VaspInformation _vaspInformation;

        public TransactionsManager(VaspClient vaspClient, VaspInformation vaspInformation)
        {
            _vaspInformation = vaspInformation;
            _outgoingTransactions = new List<Transaction>();
            _incomingTransactions = new List<Transaction>();
            _vaspSessionsManager = new VaspSessionsManager(vaspClient, this);
        }

        public async Task<Transaction> CreateOutgoingTransactionAsync(
            string originatorFullName,
            string originatorVaan,
            PlaceOfBirth originatorPlaceOfBirth,
            PostalAddress originatorPostalAddress,
            string beneficiaryFullName,
            string beneficiaryVaan,
            VirtualAssetType asset,
            decimal amount,
            NaturalPersonId[] naturalPersonIds,
            JuridicalPersonId[] juridicalPersonIds,
            string bic)
        {
            var sanitizedBeneficiaryVaan = beneficiaryVaan.Replace(" ", "");
            var sanitizedOriginatorVaan = originatorVaan.Replace(" ", "");
            var beneficiaryVaspCode = sanitizedBeneficiaryVaan.Substring(0, 8);
            var beneficiaryCustomerSpecificNumber = sanitizedBeneficiaryVaan.Substring(8, 14);

            var transaction = new Transaction
            {
                Status = TransactionStatus.Created,
                OriginatorPostalAddress = originatorPostalAddress,
                OriginatorPlaceOfBirth = originatorPlaceOfBirth,
                Amount = amount,
                Asset = asset,
                Id = Guid.NewGuid().ToString(),
                CreationDateTime = DateTime.UtcNow,
                BeneficiaryVaan = sanitizedBeneficiaryVaan,
                OriginatorVaan = sanitizedOriginatorVaan,
                OriginatorFullName = originatorFullName,
                BeneficiaryFullName = beneficiaryFullName,
                OriginatorJuridicalPersonIds = juridicalPersonIds,
                OriginatorBic = bic,
                OriginatorNaturalPersonIds = naturalPersonIds,
                SessionId = await _vaspSessionsManager.CreateSessionAsync(
                    new Originator(
                        originatorFullName,
                        sanitizedOriginatorVaan,
                        new OpenVASP.Messaging.Messages.Entities.PostalAddress
                        (
                            originatorPostalAddress.Street,
                            originatorPostalAddress.Building,
                            originatorPostalAddress.AddressLine,
                            originatorPostalAddress.PostCode,
                            originatorPostalAddress.Town,
                            originatorPostalAddress.Country
                        ),
                        new OpenVASP.Messaging.Messages.Entities.PlaceOfBirth
                        (
                            originatorPlaceOfBirth.Date,
                            originatorPlaceOfBirth.Town,
                            originatorPlaceOfBirth.Country
                        ),
                        naturalPersonIds,
                        juridicalPersonIds,
                        bic),
                    VirtualAssetsAccountNumber.Create(beneficiaryVaspCode, beneficiaryCustomerSpecificNumber))
            };

            transaction.Status = TransactionStatus.SessionRequested;

            lock (_outgoingTransactions)
            {
                _outgoingTransactions.Add(transaction);
            }

            return transaction;
        }

        public async Task SessionRequestMessageReceivedAsync(string sessionId, SessionRequestMessage message)
        {
            var transaction = new Transaction
            {
                Status = TransactionStatus.SessionRequested,
                Id = Guid.NewGuid().ToString(),
                SessionId = sessionId,
                CreationDateTime = DateTime.UtcNow
            };

            lock (_incomingTransactions)
            {
                _incomingTransactions.Add(transaction);
            }
        }

        public async Task SendSessionReplyAsync(string id, SessionReplyMessage.SessionReplyMessageCode code)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.Id == id);

            if (transaction == null || transaction.Status != TransactionStatus.SessionRequested)
                return; //todo: handle properly

            await _vaspSessionsManager.SessionReplyAsync(transaction.SessionId, code);

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

        public async Task SessionReplyMessageReceivedAsync(string sessionId, SessionReplyMessage message)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if (transaction == null)
                return; //todo: handle this case.

            if (message.Message.MessageCode ==
                SessionReplyMessage.GetMessageCode(SessionReplyMessage.SessionReplyMessageCode.SessionAccepted))
            {
                transaction.Status = TransactionStatus.SessionConfirmed;

                await _vaspSessionsManager.TransferRequestAsync(
                    transaction.SessionId,
                    transaction.BeneficiaryFullName,
                    transaction.Asset,
                    transaction.Amount);

                transaction.Status = TransactionStatus.TransferRequested;
            }
            else
            {
                transaction.Status = TransactionStatus.SessionDeclined;
                transaction.SessionDeclineCode = message.Message.MessageCode;
            }
        }

        public async Task TransferReplyMessageReceivedAsync(string sessionId, TransferReplyMessage message)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if (transaction == null || transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.

            if (message.Message.MessageCode == TransferReplyMessage.GetMessageCode(TransferReplyMessage.TransferReplyMessageCode.TransferAccepted))
            {
                transaction.Status = TransactionStatus.TransferAllowed;
                transaction.DestinationAddress = message.Transfer.DestinationAddress;
            }
            else
            {
                transaction.Status = TransactionStatus.TransferForbidden;
                transaction.TransferDeclineCode = message.Message.MessageCode;
            }
        }

        public async Task SendTransferDispatchAsync(string id, string sendingAddress, string transactionHash)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.Id == id);

            if (transaction == null || transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            await _vaspSessionsManager.TransferDispatchAsync(
                transaction.SessionId,
                new TransferReply(
                    transaction.Asset,
                    TransferType.BlockchainTransfer,
                    transaction.Amount,
                    transaction.DestinationAddress),
                transactionHash,
                sendingAddress,
                transaction.BeneficiaryFullName);

            transaction.TransactionHash = transactionHash;
            transaction.SendingAddress = sendingAddress;
            transaction.Status = TransactionStatus.TransferDispatched;
        }

        public async Task TransferConfirmationMessageReceivedAsync(string sessionId,
            TransferConfirmationMessage message)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if (transaction == null || transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            transaction.Status = TransactionStatus.TransferConfirmed;
        }

        public async Task SendTransferConfirmAsync(string id)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.Id == id);

            if (transaction == null || transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            await _vaspSessionsManager.TransferConfirmAsync(transaction.SessionId, TransferConfirmationMessage.Create(
                transaction.SessionId,
                TransferConfirmationMessage.TransferConfirmationMessageCode.TransferConfirmed,
                new Originator(
                    transaction.OriginatorFullName,
                    transaction.OriginatorVaan,
                    new OpenVASP.Messaging.Messages.Entities.PostalAddress(
                        transaction.OriginatorPostalAddress.Street,
                        transaction.OriginatorPostalAddress.Building,
                        transaction.OriginatorPostalAddress.AddressLine,
                        transaction.OriginatorPostalAddress.PostCode,
                        transaction.OriginatorPostalAddress.Town,
                        transaction.OriginatorPostalAddress.Country),
                    new OpenVASP.Messaging.Messages.Entities.PlaceOfBirth(
                        transaction.OriginatorPlaceOfBirth.Date,
                        transaction.OriginatorPlaceOfBirth.Town,
                        transaction.OriginatorPlaceOfBirth.Country),
                    transaction.OriginatorNaturalPersonIds,
                    transaction.OriginatorJuridicalPersonIds,
                    transaction.OriginatorBic),
                new Beneficiary(transaction.BeneficiaryFullName, transaction.BeneficiaryVaan),
                new TransferReply(
                    transaction.Asset,
                    TransferType.BlockchainTransfer,
                    transaction.Amount,
                    transaction.DestinationAddress),
                new OpenVASP.Messaging.Messages.Entities.Transaction(
                    transaction.TransactionHash,
                    DateTime.UtcNow,
                    transaction.SendingAddress),
                _vaspInformation));

            transaction.Status = TransactionStatus.TransferConfirmed;
        }

        public async Task SendTransferReplyAsync(string id, string destinationAddress, TransferReplyMessage.TransferReplyMessageCode code)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.Id == id);

            if (transaction == null || transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.

            await _vaspSessionsManager.TransferReplyAsync(
                transaction.SessionId,
                TransferReplyMessage.Create(
                    transaction.SessionId,
                    code,
                    new Originator(
                        transaction.OriginatorFullName,
                        transaction.OriginatorVaan,
                        new OpenVASP.Messaging.Messages.Entities.PostalAddress(
                            transaction.OriginatorPostalAddress.Street,
                            transaction.OriginatorPostalAddress.Building,
                            transaction.OriginatorPostalAddress.AddressLine,
                            transaction.OriginatorPostalAddress.PostCode,
                            transaction.OriginatorPostalAddress.Town,
                            transaction.OriginatorPostalAddress.Country),
                        new OpenVASP.Messaging.Messages.Entities.PlaceOfBirth(
                            transaction.OriginatorPlaceOfBirth.Date,
                            transaction.OriginatorPlaceOfBirth.Town,
                            transaction.OriginatorPlaceOfBirth.Country),
                        transaction.OriginatorNaturalPersonIds,
                        transaction.OriginatorJuridicalPersonIds,
                        transaction.OriginatorBic),
                    new Beneficiary(transaction.BeneficiaryFullName, transaction.BeneficiaryVaan),
                    new TransferReply(
                        transaction.Asset,
                        TransferType.BlockchainTransfer,
                        transaction.Amount,
                        destinationAddress),
                    _vaspInformation));

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

        public Task TransferRequestMessageReceivedAsync(string sessionId, TransferRequestMessage message)
        {
            var transaction = _incomingTransactions.Single(x => x.SessionId == sessionId);


            transaction.Status = TransactionStatus.TransferRequested;
            transaction.OriginatorPostalAddress = new PostalAddress
            {
                Street = message.Originator.PostalAddress.StreetName,
                AddressLine = message.Originator.PostalAddress.AddressLine,
                Building = int.Parse(message.Originator.PostalAddress.BuildingNumber),
                Country = message.Originator.PostalAddress.Country,
                PostCode = message.Originator.PostalAddress.PostCode,
                Town = message.Originator.PostalAddress.TownName
            };
            transaction.OriginatorPlaceOfBirth = new PlaceOfBirth
            {
                Country = message.Originator.PlaceOfBirth.CountryOfBirth,
                Date = message.Originator.PlaceOfBirth.DateOfBirth,
                Town = message.Originator.PlaceOfBirth.CityOfBirth
            };
            transaction.OriginatorJuridicalPersonIds = message.Originator.JuridicalPersonId?.Select(
                    x => new JuridicalPersonId(x.Identifier, x.IdentificationType, x.IssuingCountry,
                        x.NonStateIssuer))
                .ToArray();
            transaction.OriginatorNaturalPersonIds = message.Originator.NaturalPersonId?.Select(
                    x => new NaturalPersonId(x.Identifier, x.IdentificationType, x.IssuingCountry,
                        x.NonStateIssuer))
                .ToArray();
            transaction.OriginatorBic = message.Originator.BIC;
            transaction.Amount = message.Transfer.Amount;
            transaction.Asset = message.Transfer.VirtualAssetType;
            transaction.BeneficiaryVaan = message.Beneficiary.VAAN.Replace(" ", "");
            transaction.OriginatorVaan = message.Originator.VAAN.Replace(" ", "");
            transaction.OriginatorFullName = message.Originator.Name;
            transaction.BeneficiaryFullName = message.Beneficiary.Name;

            return Task.CompletedTask;
        }

        public async Task TransferDispatchMessageReceivedAsync(string sessionId, TransferDispatchMessage message)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if (transaction == null || transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            transaction.TransactionHash = message.Transaction.TransactionId;
            transaction.SendingAddress = message.Transaction.SendingAddress;
            transaction.Status = TransactionStatus.TransferDispatched;
        }

        public async Task<IReadOnlyList<Transaction>> GetOutgoingTransactionsAsync()
        {
            return _outgoingTransactions;
        }

        public async Task<IReadOnlyList<Transaction>> GetIncomingTransactionsAsync()
        {
            return _incomingTransactions;
        }
    }
}