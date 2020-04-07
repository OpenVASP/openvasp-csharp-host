using Autofac;
using Nethereum.Web3;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.ProtoMappers;
using OpenVASP.Host.Services;
using OpenVASP.Tests.Client;

namespace OpenVASP.Host.Modules
{
    public class ServiceModule : Module
    {
        private AppSettings _appSettings;
        private IEnsProvider _fakeEnsProvider;
        private WhisperSignService _signService;
        private IEthereumRpc _ethereumRpc;
        private IWhisperRpc _whisperRpc;
        private ITransportClient _transportClient;

        public ServiceModule(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            _fakeEnsProvider = new FakeEnsProvider();
            _signService = new WhisperSignService();
            _whisperRpc = new WhisperRpc(new Web3(_appSettings.WhisperRpcUri), new WhisperMessageFormatter());
            _ethereumRpc = new EthereumRpc(new Web3(_appSettings.EthereumRpcUri));
            _transportClient = new TransportClient(_whisperRpc, _signService, new WhisperMessageFormatter());

            var vaspInformationBuilder = new VaspInformationBuilder(_ethereumRpc);

            var (vaspInfo, vaspContractInfo) = vaspInformationBuilder.Create(
                _appSettings.VaspSmartContractAddress);

            var originator = VaspClient.Create(
                vaspInfo,
                vaspContractInfo,
                _appSettings.HandshakePrivateKeyHex,
                _appSettings.SignaturePrivateKeyHex,
                _ethereumRpc,
                _whisperRpc,
                _fakeEnsProvider,
                _signService,
                _transportClient);

            builder.RegisterInstance(vaspInfo);
            builder.RegisterInstance(vaspContractInfo);
            builder.RegisterInstance(originator);
            
            builder.RegisterType<TransactionsManager>()
                .SingleInstance()
                .AsSelf()
                .AutoActivate();
            
            base.Load(builder);
        }
    }
}