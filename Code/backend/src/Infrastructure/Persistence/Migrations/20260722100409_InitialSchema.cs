using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    clerk_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.UniqueConstraint("AK_users_clerk_id", x => x.clerk_id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    clerk_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_users_clerk_id",
                        column: x => x.clerk_id,
                        principalTable: "users",
                        principalColumn: "clerk_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clerk_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.id);
                    table.ForeignKey(
                        name: "FK_chats_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chats_users_clerk_id",
                        column: x => x.clerk_id,
                        principalTable: "users",
                        principalColumn: "clerk_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    filename = table.Column<string>(type: "text", nullable: false),
                    s3_key = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<int>(type: "integer", nullable: false),
                    file_type = table.Column<string>(type: "text", nullable: false),
                    processing_status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    task_id = table.Column<string>(type: "text", nullable: true),
                    source_type = table.Column<string>(type: "text", nullable: false, defaultValue: "file"),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    processing_details = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    clerk_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_documents", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_documents_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_documents_users_clerk_id",
                        column: x => x.clerk_id,
                        principalTable: "users",
                        principalColumn: "clerk_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding_model = table.Column<string>(type: "text", nullable: false),
                    rag_strategy = table.Column<string>(type: "text", nullable: false),
                    agent_type = table.Column<string>(type: "text", nullable: false),
                    chunks_per_search = table.Column<int>(type: "integer", nullable: false),
                    final_context_size = table.Column<int>(type: "integer", nullable: false),
                    similarity_threshold = table.Column<decimal>(type: "decimal", nullable: false),
                    number_of_queries = table.Column<int>(type: "integer", nullable: false),
                    reranking_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    reranking_model = table.Column<string>(type: "text", nullable: false),
                    vector_weight = table.Column<decimal>(type: "decimal", nullable: false),
                    keyword_weight = table.Column<decimal>(type: "decimal", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_settings_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false, defaultValue: "user"),
                    chat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clerk_id = table.Column<string>(type: "text", nullable: false),
                    citations = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'[]'"),
                    trace_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_users_clerk_id",
                        column: x => x.clerk_id,
                        principalTable: "users",
                        principalColumn: "clerk_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_chunks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    chunk_index = table.Column<int>(type: "integer", nullable: false),
                    page_number = table.Column<int>(type: "integer", nullable: true),
                    char_count = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    original_content = table.Column<string>(type: "json", nullable: false, defaultValueSql: "'{}'"),
                    embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_chunks", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_chunks_project_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "project_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chats_clerk_id",
                table: "chats",
                column: "clerk_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_project_id",
                table: "chats",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_document_id",
                table: "document_chunks",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id",
                table: "messages",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_clerk_id",
                table: "messages",
                column: "clerk_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_documents_clerk_id",
                table: "project_documents",
                column: "clerk_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_documents_project_id",
                table: "project_documents",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_settings_project_id",
                table: "project_settings",
                column: "project_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_clerk_id",
                table: "projects",
                column: "clerk_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_clerk_id",
                table: "users",
                column: "clerk_id",
                unique: true);

            migrationBuilder.Sql(@"
                ALTER TABLE document_chunks ADD COLUMN fts tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED;
                CREATE INDEX document_chunks_fts_idx ON document_chunks USING gin (fts);
                CREATE INDEX document_chunks_embedding_hnsw_idx ON document_chunks USING hnsw (embedding vector_cosine_ops);
            ");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION vector_search_document_chunks(
    query_embedding vector,
    filter_document_ids uuid[],
    match_threshold double precision DEFAULT 0.3,
    chunks_per_search integer DEFAULT 20
)
RETURNS TABLE(
    id uuid, document_id uuid, content text, chunk_index integer,
    created_at timestamp with time zone, page_number integer, char_count integer,
    type jsonb, original_content jsonb, embedding vector
)
LANGUAGE sql
AS $function$
SELECT dc.id, dc.document_id, dc.content, dc.chunk_index, dc.created_at,
       dc.page_number, dc.char_count, dc.type, dc.original_content, dc.embedding
FROM document_chunks dc
WHERE dc.document_id = ANY(filter_document_ids)
  AND dc.embedding IS NOT NULL
  AND (1 - (dc.embedding <=> query_embedding)) > match_threshold
ORDER BY dc.embedding <=> query_embedding ASC
LIMIT chunks_per_search;
$function$;
");

            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION keyword_search_document_chunks(
    query_text text,
    filter_document_ids uuid[],
    chunks_per_search integer DEFAULT 20
)
RETURNS TABLE(
    id uuid, document_id uuid, content text, chunk_index integer,
    created_at timestamp with time zone, page_number integer, char_count integer,
    type jsonb, original_content jsonb, embedding vector
)
LANGUAGE sql
AS $function$
SELECT dc.id, dc.document_id, dc.content, dc.chunk_index, dc.created_at,
       dc.page_number, dc.char_count, dc.type, dc.original_content, dc.embedding
FROM document_chunks dc
WHERE dc.fts @@ websearch_to_tsquery('english', query_text)
  AND dc.document_id = ANY(filter_document_ids)
ORDER BY ts_rank_cd(dc.fts, websearch_to_tsquery('english', query_text)) DESC
LIMIT chunks_per_search;
$function$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS vector_search_document_chunks;
                DROP FUNCTION IF EXISTS keyword_search_document_chunks;
            ");

            migrationBuilder.DropTable(
                name: "document_chunks");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "project_settings");

            migrationBuilder.DropTable(
                name: "project_documents");

            migrationBuilder.DropTable(
                name: "chats");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
