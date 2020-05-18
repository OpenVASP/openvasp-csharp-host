using System.Collections.Generic;
using System.Threading.Tasks;
using OpenVASP.Host.Core.Models;

namespace OpenVASP.Host.Persistence.Abstraction
{
    public interface ITransactionsRepository
    {
        public Task CreateAsync(Transaction transaction);
        public Task UpdateAsync(Transaction transaction);
        public Task<Transaction> GetAsync(string id);
        public Task<Transaction> GetBySessionIdAsync(string sessionId);
        public Task<List<Transaction>> GetAsync(TransactionType type);
    }
}