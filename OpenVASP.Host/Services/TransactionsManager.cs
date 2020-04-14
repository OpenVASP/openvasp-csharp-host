using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Core.Services;
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
        private readonly List<Core.Models.Transaction> _outgoingTransactions;
        private readonly List<Core.Models.Transaction> _incomingTransactions;
        private readonly VaspSessionsManager _vaspSessionsManager;
        private readonly VaspInformation _vaspInformation;

        public TransactionsManager(VaspClient vaspClient, VaspInformation vaspInformation)
        {
            _vaspInformation = vaspInformation;
            _outgoingTransactions = new List<Core.Models.Transaction>();
            _incomingTransactions = new List<Core.Models.Transaction>();
            _vaspSessionsManager = new VaspSessionsManager(vaspClient,this);
        }

        public async Task<Core.Models.Transaction> CreateOutgoingTransactionAsync(
            string originatorFullName,
            string originatorVaan,
            Core.Models.PlaceOfBirth originatorPlaceOfBirth,
            Core.Models.PostalAddress originatorPostalAddress,
            string beneficiaryFullName,
            string beneficiaryVaan,
            VirtualAssetType asset,
            decimal amount)
        {
            var sanitizedBeneficiaryVaan = beneficiaryVaan.Replace(" ", "");
            var sanitizedOriginatorVaan = originatorVaan.Replace(" ", "");
            var beneficiaryVaspCode = sanitizedBeneficiaryVaan.Substring(0,8);
            var beneficiaryCustomerSpecificNumber = sanitizedBeneficiaryVaan.Substring(8, 14);

            var transaction = new Core.Models.Transaction
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
                SessionId = await _vaspSessionsManager.CreateSessionAsync(
                    new Originator(
                        originatorFullName,
                        originatorVaan,
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
                        null,
                        null,
                        null
                    ),
                    VirtualAssetsAccountNumber.Create(beneficiaryVaspCode, beneficiaryCustomerSpecificNumber))
            };

            transaction.Status = TransactionStatus.SessionRequested;
            
            lock(_outgoingTransactions)
            {
                _outgoingTransactions.Add(transaction);
            }

            return transaction;
        }

        public async Task SessionReplyMessageReceivedAsync(string sessionId, SessionReplyMessage message)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if(transaction == null)
                return; //todo: handle this case.

            transaction.Status = TransactionStatus.SessionConfirmed;
            
            await _vaspSessionsManager.TransferRequestAsync(
                transaction.SessionId,
                transaction.BeneficiaryFullName,
                transaction.Asset,
                transaction.Amount);

            transaction.Status = TransactionStatus.TransferRequested;
        }

        public async Task TransferReplyMessageReceivedAsync(string sessionId, TransferReplyMessage message)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if(transaction == null || transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.
            
            transaction.Status = message.Message.MessageCode == "5"
                ? TransactionStatus.TransferForbidden
                : TransactionStatus.TransferAllowed;

            if (transaction.Status == TransactionStatus.TransferAllowed)
            {
                transaction.DestinationAddress = message.Transfer.DestinationAddress;
            }
        }

        public async Task SendTransferDispatchAsync(string id, string sendingAddress, string transactionHash)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.Id == id);

            if(transaction == null || transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            await _vaspSessionsManager.TransferDispatchAsync(
                transaction.SessionId,
                new TransferReply(
                    transaction.Asset,
                    TransferType.BlockchainTransfer,
                    transaction.Amount.ToString(CultureInfo.InvariantCulture),
                    transaction.DestinationAddress),
                transactionHash,
                sendingAddress);

            transaction.TransactionHash = transactionHash;
            transaction.SendingAddress = sendingAddress;
            transaction.Status = TransactionStatus.TransferDispatched;
        }

        public async Task TransferConfirmationMessageReceivedAsync(string sessionId, TransferConfirmationMessage message)
        {
            var transaction = _outgoingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if(transaction == null || transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            transaction.Status = TransactionStatus.TransferConfirmed;
        }

        public async Task SendTransferConfirmAsync(string id)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.Id == id);

            if(transaction == null || transaction.Status != TransactionStatus.TransferDispatched)
                return; //todo: handle this case.

            await _vaspSessionsManager.TransferConfirmAsync(transaction.SessionId, new TransferConfirmationMessage(
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
                    null,
                    null,
                    null),
                new Beneficiary(transaction.BeneficiaryFullName, transaction.BeneficiaryVaan),
                new TransferReply(
                    transaction.Asset,
                    TransferType.BlockchainTransfer,
                    transaction.Amount.ToString(CultureInfo.InvariantCulture),
                    transaction.DestinationAddress),
                new OpenVASP.Messaging.Messages.Entities.Transaction(
                    transaction.TransactionHash,
                    DateTime.UtcNow,
                    transaction.SendingAddress),
                _vaspInformation));

            transaction.Status = TransactionStatus.TransferConfirmed;
        }

        public async Task SendTransferReplyAsync(string id, string destinationAddress, bool shouldAllowTransfer)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.Id == id);

            if(transaction == null || transaction.Status != TransactionStatus.TransferRequested)
                return; //todo: handle this case.

            await _vaspSessionsManager.TransferReplyAsync(
                transaction.SessionId,
                new TransferReplyMessage(
                    transaction.SessionId,
                    shouldAllowTransfer 
                        ? TransferReplyMessage.TransferReplyMessageCode.TransferAccepted
                        : TransferReplyMessage.TransferReplyMessageCode.TransferDeclinedTransferNotAuthorized,
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
                        null,
                        null,
                        null), 
                    new Beneficiary(transaction.BeneficiaryFullName, transaction.BeneficiaryVaan),
                    new TransferReply(
                        transaction.Asset,
                        TransferType.BlockchainTransfer,
                        transaction.Amount.ToString(CultureInfo.InvariantCulture),
                        destinationAddress),
                    _vaspInformation));

            transaction.Status = shouldAllowTransfer 
                ? TransactionStatus.TransferAllowed
                : TransactionStatus.TransferForbidden;
            transaction.DestinationAddress = destinationAddress;
        }

        public Task TransferRequestMessageReceivedAsync(string sessionId, TransferRequestMessage message)
        {
            var transaction = new Core.Models.Transaction
            {
                Status = TransactionStatus.TransferRequested,
                OriginatorPostalAddress = new Core.Models.PostalAddress
                {
                    Street = message.Originator.PostalAddress.StreetName,
                    AddressLine = message.Originator.PostalAddress.AddressLine,
                    Building = message.Originator.PostalAddress.BuildingNumber,
                    Country = message.Originator.PostalAddress.Country,
                    PostCode = message.Originator.PostalAddress.PostCode,
                    Town = message.Originator.PostalAddress.TownName
                },
                OriginatorPlaceOfBirth = new Core.Models.PlaceOfBirth
                {
                    Country = message.Originator.PlaceOfBirth.CountryOfBirth,
                    Date = message.Originator.PlaceOfBirth.DateOfBirth,
                    Town = message.Originator.PlaceOfBirth.CityOfBirth
                },
                Amount = decimal.Parse(message.Transfer.Amount),
                Asset = message.Transfer.VirtualAssetType,
                Id = Guid.NewGuid().ToString(),
                CreationDateTime = DateTime.UtcNow,
                BeneficiaryVaan = message.Beneficiary.VAAN.Replace(" ", ""),
                OriginatorVaan = message.Originator.VAAN.Replace(" ", ""),
                OriginatorFullName = message.Originator.Name,
                BeneficiaryFullName = message.Beneficiary.Name,
                SessionId = sessionId
            };
            
            lock(_outgoingTransactions)
            {
                _incomingTransactions.Add(transaction);
            }

            return Task.CompletedTask;
        }

        public async Task TransferDispatchMessageReceivedAsync(string sessionId, TransferDispatchMessage message)
        {
            var transaction = _incomingTransactions.SingleOrDefault(x => x.SessionId == sessionId);

            if(transaction == null || transaction.Status != TransactionStatus.TransferAllowed)
                return; //todo: handle this case.

            transaction.TransactionHash = message.Transaction.TransactionId;
            transaction.SendingAddress = message.Transaction.SendingAddress;
            transaction.Status = TransactionStatus.TransferDispatched;
        }
        
        public async Task<IReadOnlyList<Core.Models.Transaction>> GetOutgoingTransactionsAsync()
        {
            return _outgoingTransactions;
        }

        public async Task<IReadOnlyList<Core.Models.Transaction>> GetIncomingTransactionsAsync()
        {
            return _incomingTransactions;
        }
    }
}