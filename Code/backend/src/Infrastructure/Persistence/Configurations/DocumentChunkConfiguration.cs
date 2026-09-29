using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("document_chunks");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.DocumentId).HasColumnName("document_id");
        builder.Property(c => c.Content).HasColumnName("content").IsRequired();
        builder.Property(c => c.ChunkIndex).HasColumnName("chunk_index");
        builder.Property(c => c.PageNumber).HasColumnName("page_number");
        builder.Property(c => c.CharCount).HasColumnName("char_count");
        builder.Property(c => c.TypeJson).HasColumnName("type").HasColumnType("json").HasDefaultValueSql("'{}'");
        builder.Property(c => c.OriginalContentJson).HasColumnName("original_content").HasColumnType("json").HasDefaultValueSql("'{}'");
        builder.Property(c => c.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(1536)")
            .HasConversion(
                v => new Pgvector.Vector(v),
                v => v.ToArray())
            .IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne<ProjectDocument>()
            .WithMany()
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
