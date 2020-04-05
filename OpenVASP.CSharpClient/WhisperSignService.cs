using System;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.Messaging;

namespace OpenVASP.CSharpClient
{
    public class WhisperSignService : ISignService
    {
        private static readonly EthereumMessageSigner _signer = new EthereumMessageSigner();

        public WhisperSignService()
        {

        }

        public string SignPayload(string payload, string privateKey)
        {
            var sign = _signer.EncodeUTF8AndSign(payload, new EthECKey(privateKey));

            return sign.RemoveHexPrefix();
        }

        public bool VerifySign(string payload, string sign, string pubKey)
        {
            var expectedSigner = new EthECKey(pubKey.HexToByteArray(), false);
            var signerAddress = _signer.EncodeUTF8AndEcRecover(payload, sign);

            return expectedSigner.GetPublicAddress().Equals(signerAddress, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}