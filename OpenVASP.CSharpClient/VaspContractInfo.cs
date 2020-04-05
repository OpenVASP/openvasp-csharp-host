using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.CSharpClient
{
    public class VaspContractInfo
    {
        public string Name { get; set; }

        public VaspCode VaspCode { get; set; }

        public long[] Channgels { get; set; }

        public string HandshakeKey { get; set; }

        public string SigningKey { get; set; }

        public string OwnerAddress { get; set; }

        public PostalAddress Address { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }
    }
}