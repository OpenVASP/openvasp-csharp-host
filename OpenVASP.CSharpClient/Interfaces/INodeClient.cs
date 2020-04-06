namespace OpenVASP.CSharpClient.Interfaces
{
    public interface INodeClient
    {
        IEthereumRpc EthereumRpc { get; }
        IWhisperRpc WhisperRpc { get; set; }

        ITransportClient TransportClient { get; set; }
    }
}