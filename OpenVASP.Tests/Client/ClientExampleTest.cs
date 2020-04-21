using System;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Web3;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Cryptography;
using OpenVASP.CSharpClient.Delegates;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Messaging;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using Xunit;
using Xunit.Abstractions;
using Transaction = OpenVASP.Messaging.Messages.Entities.Transaction;

namespace OpenVASP.Tests.Client
{
    public class ClientExampleTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IEnsProvider _fakeEnsProvider;
        private readonly WhisperSignService _signService;
        private readonly WhisperRpc _whisperRpc;

        public INodeClient NodeClient { get; set; }

        public VaspTestSettings Settings { get; set; }

        public ClientExampleTest(ITestOutputHelper testOutputHelper)
        {
            var all = Environment.GetEnvironmentVariables();
            string whisperRpcUrl = "http://144.76.25.187:8025"; //Environment.GetEnvironmentVariable( "WHISPER_RPC_URL");
            string ethereumRpcUrl = "https://ropsten.infura.io/v3/fb49e892176d413d85f993d0352a0971"; //Environment.GetEnvironmentVariable("ETHEREUM_RPC_URL");

            this._fakeEnsProvider = new FakeEnsProvider();
            this._testOutputHelper = testOutputHelper;
            this._signService = new WhisperSignService();
            this._whisperRpc = new WhisperRpc(new Web3(whisperRpcUrl), new WhisperMessageFormatter());

            var ethRpc = new EthereumRpc(new Web3(ethereumRpcUrl));
            
            NodeClient = new NodeClient()
            {
                EthereumRpc = ethRpc,
                WhisperRpc =_whisperRpc,
                TransportClient = new TransportClient(_whisperRpc, _signService, new WhisperMessageFormatter())
            };
            Settings = new VaspTestSettings()
            {
                PersonHandshakePrivateKeyHex =    "0xe7578145d518e5272d660ccfdeceedf2d55b90867f2b7a6e54dc726662aebac2",
                PersonSignaturePrivateKeyHex = "0x790a3437381e0ca44a71123d56dc64a6209542ddd58e5a56ecdb13134e86f7c6",
                VaspSmartContractAddressPerson = "0x6befaf0656b953b188a0ee3bf3db03d07dface61",
                VaspSmartContractAddressJuridical = "0x08FDa931D64b17c3aCFfb35C1B3902e0BBB4eE5C",
                JuridicalSignaturePrivateKeyHex = "0x6854a4e4f8945d9fa215646a820fe9a866b5635ffc7cfdac29711541f7b913f9",
                JuridicalHandshakePrivateKeyHex = "0x502eb0b1a40d5b788b2395394bc6ae47adae61e9f0a9584c4700132914a8ed04",
                PlaceOfBirth = new PlaceOfBirth(DateTime.UtcNow, "Town X", Country.List["DE"]),
                NaturalPersonIds = new NaturalPersonId[]
                {
                    new NaturalPersonId("ID", NaturalIdentificationType.PassportNumber, Country.List["DE"]), 
                },
                JuridicalIds = new JuridicalPersonId[]
                {
                    new JuridicalPersonId("ID", JuridicalIdentificationType.BankPartyIdentification, Country.List["DE"]),
                },
            };
        }

        [Fact]
        public async Task CreateSessionBetweenVASPs()
        {
            int sessionTerminationCounter = 0;
            SessionTermination sessionTerminationDelegate = @event => { sessionTerminationCounter++; };

            (VaspInformation vaspInfoPerson, VaspContractInfo vaspContractInfoPerson) = await VaspInformationBuilder.CreateForNaturalPersonAsync(
                NodeClient.EthereumRpc,
                Settings.VaspSmartContractAddressPerson,
                Settings.NaturalPersonIds,
                Settings.PlaceOfBirth);

            VaspClient originator = VaspClient.Create(
                vaspInfoPerson,
                vaspContractInfoPerson,
                Settings.PersonHandshakePrivateKeyHex,
                Settings.PersonSignaturePrivateKeyHex,
                NodeClient.EthereumRpc,
                NodeClient.WhisperRpc,
                _fakeEnsProvider,
                _signService,
                NodeClient.TransportClient);

            (VaspInformation vaspInfoJuridical, VaspContractInfo vaspContractInfoJuridical) = await VaspInformationBuilder.CreateForJuridicalPersonAsync(
                NodeClient.EthereumRpc,
                Settings.VaspSmartContractAddressJuridical,
                Settings.JuridicalIds);

            VaspClient beneficiary = VaspClient.Create(
                vaspInfoJuridical,
                vaspContractInfoJuridical,
                Settings.JuridicalHandshakePrivateKeyHex,
                Settings.JuridicalSignaturePrivateKeyHex,
                NodeClient.EthereumRpc,
                NodeClient.WhisperRpc,
                _fakeEnsProvider,
                _signService,
                NodeClient.TransportClient);

            var a = vaspInfoPerson.GetVaspCode();
            var b = vaspInfoJuridical.GetVaspCode();
            
            var originatorVaan =  VirtualAssetsAccountNumber.Create(   vaspInfoPerson.GetVaspCode(), "524ee3fb082809");
            var beneficiaryVaan = VirtualAssetsAccountNumber.Create(vaspInfoJuridical.GetVaspCode(), "524ee3fb082809");

            IVaspMessageHandler messageHandler = new VaspMessageHandlerCallbacks(
                (vaspInfo, vaspSession) => Task.FromResult(SessionReplyMessage.SessionReplyMessageCode.SessionAccepted),
                (request, currentSession) =>
                {
                    var message = TransferReplyMessage.Create(currentSession.SessionId, TransferReplyMessage.TransferReplyMessageCode.TransferAccepted,
                        request.Originator, 
                        new Beneficiary("Mr. Test",request.Beneficiary.VAAN), 
                        new TransferReply(
                            request.Transfer.VirtualAssetType, 
                            request.Transfer.TransferType, 
                            request.Transfer.Amount, 
                            "0x0"),
                        request.Vasp);

                    return Task.FromResult(message);
                },
                (dispatch, currentSession) =>
                {
                    var message = TransferConfirmationMessage.Create(currentSession.SessionId, 
                        TransferConfirmationMessage.TransferConfirmationMessageCode.TransferConfirmed,
                        dispatch.Originator,
                        dispatch.Beneficiary,
                        dispatch.Transfer,
                        dispatch.Transaction,
                        dispatch.Vasp);

                    return Task.FromResult(message);
                });

            beneficiary.RunListener(messageHandler);

            originator.SessionTerminated += sessionTerminationDelegate;
            beneficiary.SessionTerminated += sessionTerminationDelegate;

            // change enum to string and add constants
            //beneficiary.TransferRequest += request => new TransferReply(VirtualAssetType.ETH, TransferType.BlockchainTransfer, "10", "1223");
            //beneficiary.TransferDispatch += message => new TransferConfirmationMessage();

            var originatorDoc = Originator.CreateOriginatorForNaturalPerson(
                    "Test van der Test",
                    originatorVaan,
                    new PostalAddress(
                        "StreetX",
                                44,
                        "AddressLineX",
                        "510051",
                        "TownX",
                        Country.List["DE"]),
                    new PlaceOfBirth(DateTime.Today.AddYears(-30), "TownX", Country.List["DE"]),
                    new NaturalPersonId[]
                    {
                        new NaturalPersonId("Id", NaturalIdentificationType.NationalIdentityNumber, Country.List["DE"]), 
                    });
            
            var session = await originator.CreateSessionAsync(
                originatorDoc, beneficiaryVaan, new OriginatorVaspCallbacks(
                    (message, originatorSession) =>
                    {
                        return originatorSession.TransferRequestAsync(new TransferInstruction()
                        {
                            VirtualAssetTransfer = new VirtualAssetTransfer()
                            {
                                TransferType = TransferType.BlockchainTransfer,
                                VirtualAssetType = VirtualAssetType.ETH,
                                TransferAmount = 1000000000000000000
                            }
                        });
                    },
                    (message, originatorSession) =>
                    {
                        return originatorSession.TransferDispatchAsync(message.Transfer,
                            new Transaction("0xtxhash", DateTime.UtcNow, "0x0...a"), "");
                    },
                    async (message, originatorSession) =>
                    {
                        await originatorSession.TerminateAsync(TerminationMessage.TerminationMessageCode.SessionClosedTransferOccured);
                        originatorSession.Wait();
                        originator.Dispose();
                        beneficiary.Dispose();
                    }));

            Assert.Equal(1, originator.GetActiveSessions().Count);
            Assert.True(originator.GetActiveSessions().First() is OriginatorSession);

            Assert.Equal(1, beneficiary.GetActiveSessions().Count);
            Assert.True(beneficiary.GetActiveSessions().First() is BeneficiarySession);

            Assert.Equal(2, sessionTerminationCounter);

            _testOutputHelper.WriteLine("End of test");
        }
    }
}