using Application.Common;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Projects.Commands.DeleteProject;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;

    public DeleteProjectCommandHandler(IProjectRepository repository, IUnitOfWork unitOfWork, ICurrentUserAccessor currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var project = await _repository.GetByIdForOwnerAsync(request.ProjectId, _currentUser.ClerkId, ct)
            ?? throw new NotFoundException("Project not found.");

        _repository.Remove(project);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
