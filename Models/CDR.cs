using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace CDRProcessingAPI.Models
{
    public class CDR
    {
        // needed to install Install-Package Swashbuckle.AspNetCore.Annotations to hide the id
        [SwaggerIgnore]
        public int Id { get; set; }



        /// <summary>
        /// Sender and Reciver shouldnt have same number.
        /// Caller MSISDN in E.164 format, e.g., 962790123456 (country code 962, network 79, and subscriber number). Min 11 and Max 15 Numbers total
        /// <summary>
        [Required, RegularExpression(@"^\d{11,15}$", ErrorMessage = "MSISDN must be between 11 and 15 digits.")]
        public string CallerMSISDN { get; set; }




        /// <summary>
        /// Same rules 
        /// <summary>
        [Required, RegularExpression(@"^\d{11,15}$", ErrorMessage = "MSISDN must be between 11 and 15 digits.")]
        public string ReceiverMSISDN { get; set; }




        /// <summary>
        /// This is In SECONDS
        /// <summary>
        public int Duration { get; set; } // in seconds
        public DateTime Timestamp { get; set; }




        /// <summary>
        /// Make sure your input be one of: (local|long-distance|international)
        /// <summary>
        [RegularExpression(@"^(local|long-distance|international)$", ErrorMessage = "CallType must be one of the following: local, long-distance, international.")]
        public required string CallType { get; set; } // e.g., "Local", "long-distance", "International"
    }

}
