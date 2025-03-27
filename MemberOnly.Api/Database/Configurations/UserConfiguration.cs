using MemberOnly.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MemberOnly.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User> 
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Username);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FirstName).IsRequired(false).HasMaxLength(50);
        builder.Property(x => x.LastName).IsRequired(false).HasMaxLength(50);
    }
}