using System.Text.Json;
using Application.Chats.Services;
using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Chats;
using Domain.Entities;
using MediatR;

namespace Application.Chats.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IProjectSettingsRepository _settingsRepository;
    private readonly IProjectDocumentRepository _documentRepository;
    private readonly IRagRetrievalService _ragRetrievalService;
    private readonly IChatAnswerService _answerService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    private static readonly JsonSerializerOptions CitationSerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public SendMessageCommandHandler(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IProjectSettingsRepository settingsRepository,
        IProjectDocumentRepository documentRepository,
        IRagRetrievalService ragRetrievalService,
        IChatAnswerService answerService,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _settingsRepository = settingsRepository;
        _documentRepository = documentRepository;
        _ragRetrievalService = ragRetrievalService;
        _answerService = answerService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SendMessageResponse> Handle(SendMessageCommand request, CancellationToken ct)
    {
        // server/routes/chats.py:send_message performs no ownership check on chat_id/project_id
        // beyond the bearer token's identity (see docs/migration/03-API-Inventory.md). This
        // handler closes that gap - consistent with how MIG-004 fixed the HTTPException bug and
        // MIG-034 fixes the CORS wildcard, rather than reproducing a known security gap.
        var chat = await _chatRepository.GetByIdForOwnerAsync(request.ChatId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Chat not found.");
        if (chat.ProjectId != request.ProjectId)
        {
            throw new NotFoundException("Chat not found.");
        }

        var userMessage = new Message(
            Guid.NewGuid(), request.Content, "user", request.ChatId, _currentUser.ClerkId, "[]", null, DateTimeOffset.UtcNow);
        await _messageRepository.AddAsync(userMessage, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var settings = await _settingsRepository.GetByProjectIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException("Project settings not found");

        var documents = await _documentRepository.GetByProjectIdAsync(request.ProjectId, ct);
        var documentIds = documents.Select(d => d.Id).ToList();

        var chunks = await _ragRetrievalService.RetrieveAsync(request.Content, documentIds, settings, ct);

        var uniqueDocumentIds = chunks.Select(c => c.DocumentId).Distinct().ToList();
        var filenames = await _documentRepository.GetFilenamesByIdsAsync(uniqueDocumentIds, ct);
        var context = RagContextBuilder.Build(chunks, filenames);

        var aiResponseText = await _answerService.AnswerAsync(request.Content, context.Texts, context.Images, context.Tables, ct);

        var aiCitationsJson = JsonSerializer.Serialize(context.Citations, CitationSerializeOptions);
        var aiMessage = new Message(
            Guid.NewGuid(), aiResponseText, "assistant", request.ChatId, _currentUser.ClerkId,
            aiCitationsJson, null, DateTimeOffset.UtcNow);
        await _messageRepository.AddAsync(aiMessage, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var aiCitations = JsonSerializer.Deserialize<List<CitationDto>>(aiCitationsJson, CitationSerializeOptions)
            ?? new List<CitationDto>();

        return new SendMessageResponse(
            new MessageResponse(userMessage.Id, userMessage.Content, userMessage.Role, userMessage.ChatId, userMessage.CreatedAt, new List<CitationDto>()),
            new MessageResponse(aiMessage.Id, aiMessage.Content, aiMessage.Role, aiMessage.ChatId, aiMessage.CreatedAt, aiCitations));
    }
}
