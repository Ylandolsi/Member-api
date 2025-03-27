using System;
using System.ComponentModel.DataAnnotations;

namespace MemberOnly.Api.Models;

public class RefreshTokenFront
{
            [Required]
        public string UserName{ get; set; }
        
        [Required]

        public string RefreshToken { get; set; } = string.Empty;
        
        public DateTime Expires { get; set; }
        
        public bool IsActive { get; set; } = true;


}
