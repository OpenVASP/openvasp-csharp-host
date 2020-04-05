using System.Threading.Tasks;

namespace OpenVASP.CSharpClient.Interfaces
{
    public interface IEthereumRpc
    {

        /// <summary>
        /// Get information about VASP instance.
        /// </summary>
        /// <param name="vaspSmartContractAddress">Address of Ethereum smart contract</param>
        /// <returns></returns>
        Task<VaspContractInfo> GetVaspContractInfoAync(string vaspSmartContractAddress);
    }
}