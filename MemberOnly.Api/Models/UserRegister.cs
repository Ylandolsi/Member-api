using System;
using System.ComponentModel.DataAnnotations;

namespace MemberOnly.Api.Models;

public class UserRegister
{   

    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty; 
    
    
    [Required]
    [MinLength(5)]
    public string Username 
    { 
        get => _username;
        set => _username = value?.Trim() ?? string.Empty;
    }
    private string _username = string.Empty;
    
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Password must contain at least 8 characters, one uppercase, one lowercase, one number and one special character")]
    public string Password { get; set; } = string.Empty; 
    



}