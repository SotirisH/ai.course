using Ai.Api.Application.Features.CustomerManagement.Commands;
using FluentValidation;

namespace Ai.Api.Application.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(256)
            .WithMessage("First name must not exceed 256 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(256)
            .WithMessage("Last name must not exceed 256 characters.");

        RuleFor(x => x.TaxId)
            .NotEmpty()
            .WithMessage("Tax ID is required.")
            .MaximumLength(16)
            .WithMessage("Tax ID must not exceed 16 characters.");

        RuleFor(x => x.Comments)
            .MaximumLength(1024)
            .WithMessage("Comments must not exceed 1024 characters.");
    }
}
