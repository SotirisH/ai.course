namespace Ai.Api.Application.Features.ApplicationManagement.Queries;

public sealed record GetApplicationByIdQuery
{
    public Guid Id { get; init; }
}

public class GetApplicationByIdQueryHandler(IApplicationRepository repository)
{
    public async Task<ApplicationDto> Handle(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken)
    {
        ApplicationDto? dto = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (dto is null)
        {
            throw new InvalidOperationException($"Application with ID '{query.Id}' was not found.");
        }

        return dto;
    }
}
