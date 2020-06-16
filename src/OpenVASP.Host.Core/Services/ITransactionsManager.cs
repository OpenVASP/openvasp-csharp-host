using System;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transaction = OpenVASP.Host.Core.Models.Transaction;

namespace OpenVASP.Host.Core.Services
{
    public interface ITransactionsManager
    {
        Task<List<Transaction>> GetOutgoingTransactionsAsync();

        Task<List<Transaction>> GetIncomingTransactionsAsync();

        Task<Transaction> GetAsync(string id);

        Task SendSessionReplyAsync(string id, SessionReplyMessage.SessionReplyMessageCode code);

        Task<Transaction> RegisterOutgoingTransactionAsync(
            Transaction transaction,
            VirtualAssetsAccountNumber virtualAssetsAccountNumber);

        Task SendTransferReplyAsync(
            string id,
            string destinationAddress,
            TransferReplyMessage.TransferReplyMessageCode code);

        Task SendTransferConfirmAsync(string id);

        Task SendTransferDispatchAsync(
            string id,
            string sendingAddress,
            string transactionHash,
            DateTime transactionDateTime);
    }
}
