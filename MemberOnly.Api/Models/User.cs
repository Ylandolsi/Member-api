using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MemberOnly.Api.Models;

public class User
{   

    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty; 
    
    
    [Required]
    [Key]
    public string Username 
    { 
        get => _username;
        set => _username = value?.Trim() ?? string.Empty;
    }
    private string _username = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty; 
    
    public bool HasCompletedAction { get; set; } = false; 
    
    public List<Post> Posts { get; set; } = new();


}