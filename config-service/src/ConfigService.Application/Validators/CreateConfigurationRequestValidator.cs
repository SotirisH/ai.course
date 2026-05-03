using FluentValidation;
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Validators;

public class CreateConfigurationRequestValidator : AbstractValidator<CreateConfigurationRequest>
{
    public CreateConfigurationRequestValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("ApplicationId is required")
            .Length(26).WithMessage("ApplicationId must be a valid ULID (26 characters)");
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1024).WithMessage("Comments must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
        
        RuleFor(x => x.Config)
            .NotNull().WithMessage("Config is required");
    }
}

