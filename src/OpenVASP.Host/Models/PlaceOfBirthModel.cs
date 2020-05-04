using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.Host.Models
{
    /// <summary>
    /// Represents place of birth of the originator.
    /// </summary>
    public class PlaceOfBirthModel
    {
        /// <summary>
        /// Town.
        /// </summary>
        [Required]
        public string Town { set; get; }
        
        /// <summary>
        /// Country
        /// </summary>
        [Required]
        public string Country { set; get; }
        
        /// <summary>
        /// Date of birth.
        /// </summary>
        [Required]
        [JsonConverter(typeof(CustomDateConverter))]
        public DateTime Date { set; get; }
    }
    
    public class CustomDateConverter : IsoDateTimeConverter
    {
        public CustomDateConverter()
        {
            base.DateTimeFormat = "dd/MM/yyyy";
        }
    }
}