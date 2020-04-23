using System;
using Autofac;
using Nethereum.Web3;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.Host.Services;
using OpenVASP.Messaging;
using OpenVASP.Messaging.Messages.Entities;

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

            VaspInformation vaspInfo;
            VaspContractInfo vaspContractInfo;

            if (_appSettings.VaspBic != null)
            {
                (vaspInfo, vaspContractInfo) = vaspInformationBuilder
                    .CreateForBankAsync(_appSettings.VaspSmartContractAddress, _appSettings.VaspBic)
                    .GetAwaiter()
                    .GetResult();
            }
            else if (_appSettings.VaspJuridicalIds != null)
            {
                (vaspInfo, vaspContractInfo) = vaspInformationBuilder
                    .CreateForJuridicalPersonAsync(_appSettings.VaspSmartContractAddress, _appSettings.VaspJuridicalIds)
                    .GetAwaiter()
                    .GetResult();
            }
            else if (_appSettings.VaspNaturalIds != null)
            {
                (vaspInfo, vaspContractInfo) = vaspInformationBuilder
                    .CreateForNaturalPersonAsync(_appSettings.VaspSmartContractAddress, _appSettings.VaspNaturalIds, _appSettings.VaspPlaceOfBirth)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                throw new ArgumentException("Invalid configuration.");
            }
            
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
            builder.RegisterInstance(_appSettings);
            
            builder.RegisterType<TransactionsManager>()
                .SingleInstance()
                .AsSelf()
                .AutoActivate();
            
            base.Load(builder);
        }
    }
}