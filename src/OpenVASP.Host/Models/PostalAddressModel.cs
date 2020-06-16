using System.ComponentModel.DataAnnotations;

namespace OpenVASP.Host.Models
{
    /// <summary>
    /// Represents postal address of the originator.
    /// </summary>
    public class PostalAddressModel
    {
        /// <summary>
        /// Street.
        /// </summary>
        [Required]
        public string Street { get; set; }

        /// <summary>
        /// Building number.
        /// </summary>
        [Required]
        public string Building { get; set; }

        /// <summary>
        /// Address line.
        /// </summary>
        [Required]
        public string AddressLine { get; set; }

        /// <summary>
        /// Post code.
        /// </summary>
        [Required]
        public string PostCode { get; set; }

        /// <summary>
        /// Town.
        /// </summary>
        [Required]
        public string Town { get; set; }

        /// <summary>
        /// Country.
        /// </summary>
        [Required]
        public string CountryIso2Code { get; set; }
    }
}