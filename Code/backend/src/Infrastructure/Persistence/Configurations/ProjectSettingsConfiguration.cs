using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProjectSettingsConfiguration : IEntityTypeConfiguration<ProjectSettings>
{
    public void Configure(EntityTypeBuilder<ProjectSettings> builder)
    {
        builder.ToTable("project_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.ProjectId).HasColumnName("project_id");
        builder.HasIndex(s => s.ProjectId).IsUnique();
        builder.Property(s => s.EmbeddingModel).HasColumnName("embedding_model").IsRequired();
        builder.Property(s => s.RagStrategy).HasColumnName("rag_strategy").IsRequired();
        builder.Property(s => s.AgentType).HasColumnName("agent_type").IsRequired();
        builder.Property(s => s.ChunksPerSearch).HasColumnName("chunks_per_search");
        builder.Property(s => s.FinalContextSize).HasColumnName("final_context_size");
        builder.Property(s => s.SimilarityThreshold).HasColumnName("similarity_threshold").HasColumnType("decimal");
        builder.Property(s => s.NumberOfQueries).HasColumnName("number_of_queries");
        builder.Property(s => s.RerankingEnabled).HasColumnName("reranking_enabled");
        builder.Property(s => s.RerankingModel).HasColumnName("reranking_model").IsRequired();
        builder.Property(s => s.VectorWeight).HasColumnName("vector_weight").HasColumnType("decimal");
        builder.Property(s => s.KeywordWeight).HasColumnName("keyword_weight").HasColumnType("decimal");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
