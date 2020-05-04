using System;
using Autofac;
using Nethereum.Web3;
using OpenVASP.CSharpClient;
using OpenVASP.Host.Core.Services;
using OpenVASP.Host.Services;
using OpenVASP.Messaging;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Modules
{
    internal class ServiceModule : Module
    {
        private AppSettings _appSettings;

        public ServiceModule(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var fakeEnsProvider = new FakeEnsProvider();
            var signService = new WhisperSignService();
            var whisperRpc = new WhisperRpc(new Web3(_appSettings.WhisperRpcUri), new WhisperMessageFormatter());
            var ethereumRpc = new EthereumRpc(new Web3(_appSettings.EthereumRpcUri));
            var transportClient = new WhisperTransportClient(whisperRpc, signService, new WhisperMessageFormatter());

            var vaspInformationBuilder = new VaspInformationBuilder(ethereumRpc);

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

            builder.RegisterInstance(vaspInfo);
            builder.RegisterInstance(vaspContractInfo);
            builder.RegisterInstance(_appSettings);
            builder.RegisterInstance(ethereumRpc);
            builder.RegisterInstance(whisperRpc);
            builder.RegisterInstance(fakeEnsProvider);
            builder.RegisterInstance(signService);
            builder.RegisterInstance(transportClient);

            builder.RegisterType<TransactionDataService>()
                .As<ITransactionDataService>()
                .SingleInstance();

            builder.RegisterType<TransactionsManager>()
                .SingleInstance()
                .As<ITransactionsManager>()
                .AutoActivate()
                .WithParameter("handshakePrivateKeyHex", _appSettings.HandshakePrivateKeyHex)
                .WithParameter("signaturePrivateKeyHex", _appSettings.SignaturePrivateKeyHex);

            base.Load(builder);
        }
    }
}