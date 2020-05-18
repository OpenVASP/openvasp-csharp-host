using System.Threading.Tasks;
using OpenVASP.CSharpClient.Sessions;

namespace OpenVASP.Host.Persistence.Abstraction
{
    public interface ISessionsRepository
    {
        public Task CreateAsync(OriginatorSessionInfo info);
        public Task CreateAsync(BeneficiarySessionInfo info);
        public Task<VaspSessionInfo> GetAsync(string id);
    }
}