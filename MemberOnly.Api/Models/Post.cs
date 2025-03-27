using System;
using System.ComponentModel.DataAnnotations;

namespace MemberOnly.Api.Models;

public class Post
{
    public int Id { get; set; }

    private string _title = null!;
    [MinLength(5)]
    public string Title
    {
        get => _title;
        set => _title = value.Trim();
    }

    private string _content = null!;
    [MinLength(10)]
    public string Content
    {
        get => _content;
        set => _content = value.Trim();
    }

    public DateTime CreatedAt { get; set; }

    public string Username { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
