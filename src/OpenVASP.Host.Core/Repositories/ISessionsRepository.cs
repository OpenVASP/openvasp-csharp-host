using System.Threading.Tasks;
using OpenVASP.CSharpClient.Sessions;

namespace OpenVASP.Host.Core.Repositories
{
    public interface ISessionsRepository
    {
        Task CreateAsync(OriginatorSessionInfo info);
        Task CreateAsync(BeneficiarySessionInfo info);
        Task<VaspSessionInfo> GetAsync(string id);
    }
}