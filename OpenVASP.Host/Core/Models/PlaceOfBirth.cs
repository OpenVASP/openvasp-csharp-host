using System;
using OpenVASP.Messaging.Messages.Entities;

namespace OpenVASP.Host.Core.Models
{
    public class PlaceOfBirth
    {
        public string Town { set; get; }
        
        public Country Country { set; get; }
        
        public DateTime Date { set; get; }
    }
}