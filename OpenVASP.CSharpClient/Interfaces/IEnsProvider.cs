using System.Threading.Tasks;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.CSharpClient.Interfaces
{
    public interface IEnsProvider
    {
        /// <summary>
        /// Get Ethereum address of VASP smart contract by vasp code.
        /// </summary>
        /// <param name="vaspCode">VaspCode</param>
        /// <returns>Ethereum address of VASP smart contract </returns>
        Task<string> GetContractAddressByVaspCodeAsync(VaspCode vaspCode);
    }
}