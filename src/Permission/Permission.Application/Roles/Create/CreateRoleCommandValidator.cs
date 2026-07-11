using FluentValidation;

namespace Permission.Application.Roles.Create;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Permissions).NotNull();
        RuleForEach(c => c.Permissions).NotEmpty();
    }
}
