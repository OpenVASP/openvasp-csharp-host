using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Models
{
    public class JuridicalPersonIdModel
    {
        public string Id { set; get; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public JuridicalIdentificationType Type { set; get; }
        
        public string CountryCode { set; get; }
        
        public string NonStateIssuer { set; get; }
    }
}