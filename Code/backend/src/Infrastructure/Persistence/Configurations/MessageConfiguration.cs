using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.Content).HasColumnName("content").IsRequired();
        builder.Property(m => m.Role).HasColumnName("role").HasDefaultValue("user");
        builder.Property(m => m.ChatId).HasColumnName("chat_id");
        builder.Property(m => m.ClerkId).HasColumnName("clerk_id").IsRequired();
        builder.Property(m => m.CitationsJson).HasColumnName("citations").HasColumnType("json").HasDefaultValueSql("'[]'");
        builder.Property(m => m.TraceId).HasColumnName("trace_id");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne<Chat>()
            .WithMany()
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.ClerkId)
            .HasPrincipalKey(u => u.ClerkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
