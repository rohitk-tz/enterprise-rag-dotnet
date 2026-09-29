using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMessageRepository
{
    Task AddAsync(Message message, CancellationToken ct);
}
