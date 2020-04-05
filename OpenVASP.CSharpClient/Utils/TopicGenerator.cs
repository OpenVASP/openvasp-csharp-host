using System;
using Nethereum.Hex.HexConvertors.Extensions;
using OpenVASP.Messaging;

namespace OpenVASP.CSharpClient.Utils
{
    public static class TopicGenerator
    {
        private static Random _random = new Random();

        public static string GenerateSessionTopic()
        {
            var bytes = new byte[4];
            _random.NextBytes(bytes);

            return bytes.ToHex(true);
        }
    }
}