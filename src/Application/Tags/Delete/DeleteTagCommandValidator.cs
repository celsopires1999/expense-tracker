using FluentValidation;

namespace Application.Tags.Delete;

internal sealed class DeleteTagCommandValidator : AbstractValidator<DeleteTagCommand>
{
    public DeleteTagCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
