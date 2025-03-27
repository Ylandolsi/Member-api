using System;
using MemberOnly.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MemberOnly.Api.Database.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(post => post.Id);
        builder.Property(post => post.Id).ValueGeneratedOnAdd();
        builder.Property(post => post.Title).IsRequired().HasMaxLength(100);


        builder.HasOne(post => post.User)
            .WithMany(user => user.Posts)
            .HasForeignKey(post => post.Username)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
