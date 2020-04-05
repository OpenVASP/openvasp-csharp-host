using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtoMappers;
using OpenVASP.Tests.Client;
using SessionDemo.Models;
using Xunit;

namespace SessionDemo.Services
{
    public class VaspSessionService
    {
        private readonly string _ethereumRpcUrl = "https://ropsten.infura.io/v3/fb49e892176d413d85f993d0352a0971";
        private readonly string _whisperRpcUrl = "http://144.76.25.187:8025";
        private readonly IEnsProvider _fakeEnsProvider;
        private readonly WhisperSignService _signService;
        private readonly IEthereumRpc _ethereumRpc;
        private readonly IWhisperRpc _whisperRpc;
        private readonly ITransportClient _transportClient;
        
        private readonly string _personHandshakePrivateKeyHex =    "0xe7578145d518e5272d660ccfdeceedf2d55b90867f2b7a6e54dc726662aebac2";
        private readonly string _personSignaturePrivateKeyHex = "0x790a3437381e0ca44a71123d56dc64a6209542ddd58e5a56ecdb13134e86f7c6";
        private readonly string _vaspSmartContractAddressPerson = "0x6befaf0656b953b188a0ee3bf3db03d07dface61";
        private readonly string _vaspSmartContractAddressJuridical = "0x08FDa931D64b17c3aCFfb35C1B3902e0BBB4eE5C";
        private readonly string _juridicalSignaturePrivateKeyHex = "0x6854a4e4f8945d9fa215646a820fe9a866b5635ffc7cfdac29711541f7b913f9";
        private readonly string _juridicalHandshakePrivateKeyHex = "0x502eb0b1a40d5b788b2395394bc6ae47adae61e9f0a9584c4700132914a8ed04";

        private readonly NaturalPersonId[] _naturalPersonIds = new NaturalPersonId[]
        {
            new NaturalPersonId("ID", NaturalIdentificationType.PassportNumber, Country.List["DE"]),
        };

        private readonly JuridicalPersonId[] _juridicalPersonIds = new JuridicalPersonId[]
        {
            new JuridicalPersonId("ID", JuridicalIdentificationType.BankPartyIdentification, Country.List["DE"]),
        };

        public ConcurrentDictionary<string, VaspSessionWrapper> VaspSessions
            { get; } = new ConcurrentDictionary<string, VaspSessionWrapper>();
        
        public VaspSessionService()
        {
            _fakeEnsProvider = new FakeEnsProvider();
            _signService = new WhisperSignService();
            _whisperRpc = new WhisperRpc(new Web3(_whisperRpcUrl), new WhisperMessageFormatter());
            _ethereumRpc = new EthereumRpc(new Web3(_ethereumRpcUrl));
            _transportClient = new TransportClient(_whisperRpc, _signService, new WhisperMessageFormatter());
        }

        public void CreateSessionRequest(
            string beneficiaryName,
            string originatorName,
            PostalAddress originatorPostalAddress,
            PlaceOfBirth placeOfBirth,
            VirtualAssetType assetType,
            decimal transferAmount)
        {
            (VaspInformation vaspInfoPerson, VaspContractInfo vaspContractInfoPerson) = VaspInformationBuilder.CreateForNaturalPersonAsync(
                _ethereumRpc,
                _vaspSmartContractAddressPerson,
                _naturalPersonIds,
                placeOfBirth)
                .GetAwaiter()
                .GetResult();

            VaspClient originator = VaspClient.Create(
                vaspInfoPerson,
                vaspContractInfoPerson,
                _personHandshakePrivateKeyHex,
                _personSignaturePrivateKeyHex,
                _ethereumRpc,
                _whisperRpc,
                _fakeEnsProvider,
                _signService,
                _transportClient);

            (VaspInformation vaspInfoJuridical, VaspContractInfo vaspContractInfoJuridical) = VaspInformationBuilder.CreateForJuridicalPersonAsync(
                _ethereumRpc,
                _vaspSmartContractAddressJuridical,
                _juridicalPersonIds)
                .GetAwaiter()
                .GetResult();

            VaspClient beneficiary = VaspClient.Create(
                vaspInfoJuridical,
                vaspContractInfoJuridical,
                _juridicalHandshakePrivateKeyHex,
                _juridicalSignaturePrivateKeyHex,
                _ethereumRpc,
                _whisperRpc,
                _fakeEnsProvider,
                _signService,
                _transportClient);

            var originatorVaan =  VirtualAssetssAccountNumber.Create(   vaspInfoPerson.GetVaspCode(), "524ee3fb082809");
            var beneficiaryVaan = VirtualAssetssAccountNumber.Create(vaspInfoJuridical.GetVaspCode(), "524ee3fb082809");

            var vaspSession = new VaspSessionWrapper(
                beneficiaryName,
                originator,
                beneficiary);

            VaspSessions[vaspSession.Id] = vaspSession;
            
            var originatorDoc = Originator.CreateOriginatorForNaturalPerson(
                    originatorName,
                    originatorVaan,
                    originatorPostalAddress,
                    placeOfBirth,
                    new NaturalPersonId[]
                    {
                        new NaturalPersonId("Id", NaturalIdentificationType.NationalIdentityNumber, Country.List["DE"]), 
                    });
            
            vaspSession.StartSession(originatorDoc, beneficiaryVaan, assetType, transferAmount);
        }

        public void ReplyToTransferRequest(string id, bool shouldAllowTransferRequest)
        {
            VaspSessions[id].ReplyToTransferRequest(shouldAllowTransferRequest);
        }
    }
}