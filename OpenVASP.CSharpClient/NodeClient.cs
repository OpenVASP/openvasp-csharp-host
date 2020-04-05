using OpenVASP.CSharpClient.Interfaces;

namespace OpenVASP.CSharpClient
{
    public class NodeClient : INodeClient
    {
        public IEthereumRpc EthereumRpc { get; set; }
        public IWhisperRpc WhisperRpc { get; set; }
        public ITransportClient TransportClient { get; set; }
    }
}