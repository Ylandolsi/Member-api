using System;
using System.ComponentModel.DataAnnotations;

namespace MemberOnly.Api.Models;

public class PostAdd
{


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
}