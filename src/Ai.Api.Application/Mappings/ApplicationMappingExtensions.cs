namespace Ai.Api.Application.Mappings;

public static class ApplicationMappingExtensions
{
    public static CreateApplicationDto ToDto(this CreateApplicationCommand command)
    {
        return new CreateApplicationDto
        {
            Name = command.Name,
            Comments = command.Comments
        };
    }

    public static ApplicationDto ApplyTo(this UpdateApplicationCommand command, ApplicationDto existing)
    {
        return existing with
        {
            Name = command.Name,
            Comments = command.Comments
        };
    }
}
