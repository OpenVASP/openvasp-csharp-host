using System.ComponentModel.DataAnnotations;

namespace OpenVASP.Host.Models.Request
{
    public class CreateOutgoingTransactionRequestModel
    {
        /// <summary>
        /// Beneficiary full name.
        /// </summary>
        [Required]
        public string BeneficiaryFullName { set; get; }
        
        /// <summary>
        /// Beneficiary VAAN code.
        /// </summary>
        [Required]
        public string BeneficiaryVaan { set; get; }
        
        /// <summary>
        /// Originator full name.
        /// </summary>
        [Required]
        public string OriginatorFullName { set; get; }
        
        /// <summary>
        /// Originator VAAN code.
        /// </summary>
        [Required]
        public string OriginatorVaan { set; get; }
        
        /// <summary>
        /// Originator postal address.
        /// </summary>
        [Required]
        public PostalAddressModel OriginatorPostalAddress { set; get; }
        
        /// <summary>
        /// Originator place of birth.
        /// </summary>
        [Required]
        public PlaceOfBirthModel OriginatorPlaceOfBirth { set; get; }
        
        /// <summary>
        /// The asset of the transaction.
        /// </summary>
        [Required]
        public string Asset { set; get; }
        
        /// <summary>
        /// The amount to transfer.
        /// </summary>
        [Required]
        public decimal Amount { set; get; }
    }
}