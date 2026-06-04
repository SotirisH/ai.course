using Ai.Api.Domain.Exceptions;

namespace Ai.Api.Domain.Entities;

public class Application
{
    private Application()
    {
    }

    public Application(Guid id,
        string name,
        string? comments = null)
    {
        Id = id;
        Name = name;
        Comments = comments;
    }

    public Guid Id { get; private set; } = Guid.CreateVersion7();

    public string Name
    {
        get;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("Application name is required.");
            }

            if (value.Length > 256)
            {
                throw new DomainException("Application name must not exceed 256 characters.");
            }

            field = value;
        }
    } = null!;

    public string? Comments
    {
        get;
        private set
        {
            if (value is not null && value.Length > 1024)
            {
                throw new DomainException("Comments must not exceed 1024 characters.");
            }

            field = value;
        }
    }

    public void Update(string name,
        string? comments)
    {
        Name = name;
        Comments = comments;
    }
}
