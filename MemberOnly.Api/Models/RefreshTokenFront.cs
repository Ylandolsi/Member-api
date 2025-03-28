using System;
using System.ComponentModel.DataAnnotations;

namespace MemberOnly.Api.Models;

public class RefreshTokenFront
{
        [Required]
        public string UserName{ get; set; }
        
        [Required]

        public string RefreshToken { get; set; } = string.Empty;
        

}
