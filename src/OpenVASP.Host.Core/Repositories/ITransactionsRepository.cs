using System.Collections.Generic;
using System.Threading.Tasks;
using OpenVASP.Host.Core.Models;

namespace OpenVASP.Host.Core.Repositories
{
    public interface ITransactionsRepository
    {
        Task CreateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task<Transaction> GetAsync(string id);
        Task<Transaction> GetBySessionIdAsync(string sessionId);
        Task<List<Transaction>> GetAsync(TransactionType type);
    }
}