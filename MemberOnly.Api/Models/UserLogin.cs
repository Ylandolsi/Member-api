using System;

namespace MemberOnly.Api.Models;

public class UserLogin
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

}
