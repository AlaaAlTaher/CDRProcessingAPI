using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace CDRProcessingAPI.Models
{
    public class User
    {
        [SwaggerIgnore]
        public int Id { get; set; }




        public required string Name { get; set; }




        /// <summary>
        ///  MSISDN in E.164 format, e.g., 962790123456 (country code 962, network 79, and subscriber number). Min 11 and Max 15 Numbers total
        /// <summary>
        [Required, RegularExpression(@"^\d{11,15}$", ErrorMessage = "MSISDN must be between 11 and 15 digits.")]
        public required string MSISDN { get; set; }
    }

}
