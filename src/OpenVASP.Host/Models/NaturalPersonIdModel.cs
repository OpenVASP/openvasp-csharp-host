using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Models
{
    public class NaturalPersonIdModel
    {
        public string Id { set; get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public NaturalIdentificationType Type { set; get; }
        
        public string CountryCode { set; get; }
        
        public string NonStateIssuer { set; get; }
    }
}