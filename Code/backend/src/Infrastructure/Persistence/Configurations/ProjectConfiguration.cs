using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.ClerkId).HasColumnName("clerk_id").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.ClerkId)
            .HasPrincipalKey(u => u.ClerkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
