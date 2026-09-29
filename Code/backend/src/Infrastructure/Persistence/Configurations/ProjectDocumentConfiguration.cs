using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("project_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(d => d.ProjectId).HasColumnName("project_id");
        builder.Property(d => d.Filename).HasColumnName("filename").IsRequired();
        builder.Property(d => d.S3Key).HasColumnName("s3_key").IsRequired();
        builder.Property(d => d.FileSize).HasColumnName("file_size");
        builder.Property(d => d.FileType).HasColumnName("file_type").IsRequired();
        builder.Property(d => d.ProcessingStatus).HasColumnName("processing_status").HasDefaultValue("pending");
        builder.Property(d => d.TaskId).HasColumnName("task_id");
        builder.Property(d => d.SourceType).HasColumnName("source_type").HasDefaultValue("file");
        builder.Property(d => d.SourceUrl).HasColumnName("source_url");
        builder.Property(d => d.ProcessingDetailsJson).HasColumnName("processing_details").HasColumnType("json").HasDefaultValueSql("'{}'");
        builder.Property(d => d.ClerkId).HasColumnName("clerk_id").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.ClerkId)
            .HasPrincipalKey(u => u.ClerkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
