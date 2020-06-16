using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Core.Models
{
    public class PostalAddress
    {
        public string Street { get; set; }

        public string Building { get; set; }

        public string AddressLine { get; set; }

        public string PostCode { get; set; }

        public string Town { get; set; }

        public Country Country { get; set; }
    }
}