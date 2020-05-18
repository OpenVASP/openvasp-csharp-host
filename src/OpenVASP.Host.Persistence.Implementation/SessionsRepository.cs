using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Host.Persistence.Abstraction;

namespace OpenVASP.Host.Persistence.Implementation
{
    public class SessionsRepository : ISessionsRepository
    {
        private readonly object _lock = new object();
        private readonly List<VaspSessionInfo> _sessions;

        public SessionsRepository()
        {
            _sessions = new List<VaspSessionInfo>();
        }

        public Task CreateAsync(OriginatorSessionInfo info)
        {
            lock (_lock)
            {
                _sessions.Add(info);
            }
            return Task.CompletedTask;
        }

        public Task CreateAsync(BeneficiarySessionInfo info)
        {
            lock (_lock)
            {
                _sessions.Add(info);
            }
            return Task.CompletedTask;
        }

        public Task<VaspSessionInfo> GetAsync(string id)
        {
            lock (_lock)
            {
                return Task.FromResult(_sessions.FirstOrDefault(x => x.Id == id));
            }
        }
    }
}