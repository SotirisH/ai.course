using FluentValidation;
using ConfigService.Application.DTOs;

namespace ConfigService.Application.Validators;

public class UpdateApplicationRequestValidator : AbstractValidator<UpdateApplicationRequest>
{
    public UpdateApplicationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters");
        
        RuleFor(x => x.Comments)
            .MaximumLength(1024).WithMessage("Comments must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Comments));
    }
}

