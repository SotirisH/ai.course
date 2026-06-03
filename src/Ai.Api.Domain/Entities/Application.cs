using Ai.Api.Domain.Exceptions;

namespace Ai.Api.Domain.Entities;

public class Application
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = null!;
    public string? Comments { get; private set; }

    private Application() { }

    public Application(string name, string? comments = null)
    {
        Validate(name, comments);
        Name = name;
        Comments = comments;
    }

    public Application(Guid id, string name, string? comments = null)
    {
        Id = id;
        Name = name;
        Comments = comments;
    }

    public void Update(string name, string? comments)
    {
        Validate(name, comments);
        Name = name;
        Comments = comments;
    }

    private static void Validate(string name, string? comments)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Application name is required.");
        }

        if (name.Length > 256)
        {
            throw new DomainException("Application name must not exceed 256 characters.");
        }

        if (comments?.Length > 1024)
        {
            throw new DomainException("Comments must not exceed 1024 characters.");
        }
    }
}
