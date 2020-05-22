using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Persistence.Abstraction;

namespace OpenVASP.Host.Persistence.Implementation
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly object _lock = new object();
        private readonly List<Transaction> _transactions;

        public TransactionsRepository()
        {
            _transactions = new List<Transaction>();
        }

        public Task CreateAsync(Transaction transaction)
        {
            lock (_lock)
            {
                _transactions.Add(transaction);
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Transaction transaction)
        {
            lock (_lock)
            {
                var transactionInMemory = _transactions.FirstOrDefault(x => x.Id == transaction.Id);

                _transactions.Remove(transactionInMemory);
                _transactions.Add(transaction);
            }

            return Task.CompletedTask;
        }

        public Task<Transaction> GetAsync(string id)
        {
            lock (_lock)
            {
                return Task.FromResult(_transactions.SingleOrDefault(x => x.Id == id));
            }
        }

        public Task<Transaction> GetBySessionIdAsync(string sessionId)
        {
            lock (_lock)
            {
                return Task.FromResult(_transactions.SingleOrDefault(x => x.SessionId == sessionId));
            }
        }

        public Task<List<Transaction>> GetAsync(TransactionType type)
        {
            lock (_lock)
            {
                return Task.FromResult(
                    _transactions
                    .Where(x => x.Type == type)
                    .OrderByDescending(i => i.CreationDateTime)
                    .ToList());
            }
        }
    }
}