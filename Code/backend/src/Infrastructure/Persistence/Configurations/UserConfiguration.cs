using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(u => u.ClerkId).HasColumnName("clerk_id").IsRequired();
        builder.HasIndex(u => u.ClerkId).IsUnique();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
    }
}
