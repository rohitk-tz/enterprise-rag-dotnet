using SharedKernel;

namespace Domain.Entities;

public class ProjectDocument : Entity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Filename { get; private set; }
    public string S3Key { get; private set; }
    public int FileSize { get; private set; }
    public string FileType { get; private set; }
    public string ProcessingStatus { get; private set; }
    public string? TaskId { get; private set; }
    public string SourceType { get; private set; }
    public string? SourceUrl { get; private set; }
    public string ProcessingDetailsJson { get; private set; }
    public string ClerkId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ProjectDocument(
        Guid id, Guid projectId, string filename, string s3Key, int fileSize, string fileType,
        string processingStatus, string? taskId, string sourceType, string? sourceUrl,
        string processingDetailsJson, string clerkId, DateTimeOffset createdAt) : base(id)
    {
        ProjectId = projectId;
        Filename = filename;
        S3Key = s3Key;
        FileSize = fileSize;
        FileType = fileType;
        ProcessingStatus = processingStatus;
        TaskId = taskId;
        SourceType = sourceType;
        SourceUrl = sourceUrl;
        ProcessingDetailsJson = processingDetailsJson;
        ClerkId = clerkId;
        CreatedAt = createdAt;
    }

    public void SetTaskId(string taskId) => TaskId = taskId;

    public void UpdateStatus(string status, string processingDetailsJson)
    {
        ProcessingStatus = status;
        ProcessingDetailsJson = processingDetailsJson;
    }
}
