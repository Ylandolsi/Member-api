using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MemberOnly.Api.Models;

public class UserInfo
{   

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty; 
    
    public string Username { get; set; } = string.Empty;

    public bool HasCompletedAction { get; set; } = false; 
    
    public List<Post> Posts { get; set; } = new();


}