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
            var ethereumRpc = new EthereumRpc(new Web3(_appSettings.EthereumRpcUri));
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
            builder.RegisterType<WhisperMessageFormatter>()
                .As<IMessageFormatter>()
                .SingleInstance();
            builder.RegisterType<WhisperRpc>()
                .As<IWhisperRpc>()
                .SingleInstance()
                .WithParameter(TypedParameter.From((IWeb3)new Web3(_appSettings.WhisperRpcUri)));
            builder.RegisterType<EnsProvider>()
                .As<IEnsProvider>()
                .SingleInstance();
            builder.RegisterType<WhisperSignService>()
                .As<ISignService>()
                .SingleInstance();
            builder.RegisterType<WhisperTransportClient>()
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