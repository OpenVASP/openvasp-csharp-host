using System;
using Autofac;
using Nethereum.Web3;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
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
            var ensProvider = new EnsProvider();
            var signService = new WhisperSignService();
            var whisperRpc = new WhisperRpc(new Web3(_appSettings.WhisperRpcUri), new WhisperMessageFormatter());
            var ethereumRpc = new EthereumRpc(new Web3(_appSettings.EthereumRpcUri));
            var transportClient = new WhisperTransportClient(whisperRpc, signService, new WhisperMessageFormatter());

            var vaspInformationBuilder = new VaspInformationBuilder(ethereumRpc);

            VaspInformation vaspInfo;
            VaspCode vaspCode;

            if (_appSettings.VaspBic != null)
            {
                (vaspInfo, vaspCode) = vaspInformationBuilder
                    .CreateForBankAsync(_appSettings.VaspSmartContractAddress, _appSettings.VaspBic)
                    .GetAwaiter()
                    .GetResult();
            }
            else if (_appSettings.VaspJuridicalIds != null)
            {
                (vaspInfo, vaspCode) = vaspInformationBuilder
                    .CreateForJuridicalPersonAsync(_appSettings.VaspSmartContractAddress, _appSettings.VaspJuridicalIds)
                    .GetAwaiter()
                    .GetResult();
            }
            else if (_appSettings.VaspNaturalIds != null)
            {
                (vaspInfo, vaspCode) = vaspInformationBuilder
                    .CreateForNaturalPersonAsync(_appSettings.VaspSmartContractAddress, _appSettings.VaspNaturalIds, _appSettings.VaspPlaceOfBirth)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                throw new ArgumentException("Invalid configuration.");
            }

            builder.RegisterInstance(vaspInfo);
            builder.RegisterInstance(vaspCode);
            builder.RegisterInstance(ethereumRpc)
                .As<IEthereumRpc>()
                .SingleInstance();
            builder.RegisterInstance(whisperRpc)
                .As<IWhisperRpc>()
                .SingleInstance();
            builder.RegisterInstance(ensProvider)
                .As<IEnsProvider>()
                .SingleInstance();
            builder.RegisterInstance(signService)
                .As<ISignService>()
                .SingleInstance();
            builder.RegisterInstance(transportClient)
                .As<ITransportClient>()
                .SingleInstance();
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