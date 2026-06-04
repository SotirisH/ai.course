using Ai.Api.Application.Features.ApplicationManagement.Commands;
using FluentValidation;

namespace Ai.Api.Application.Validators;

public class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Application name is required.")
            .MaximumLength(256)
            .WithMessage("Application name must not exceed 256 characters.");

        RuleFor(x => x.Comments)
            .MaximumLength(1024)
            .WithMessage("Comments must not exceed 1024 characters.");
    }
}
