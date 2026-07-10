using Auth.Application.Abstractions.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Auth.Application.Abstractions.Behaviors;

internal static class ValidationDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> decorated,
        IServiceProvider serviceProvider) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            IValidator<TCommand>? validator = serviceProvider.GetService<IValidator<TCommand>>();

            if (validator is null)
            {
                return await decorated.Handle(command, cancellationToken);
            }

            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (validationResult.IsValid)
            {
                return await decorated.Handle(command, cancellationToken);
            }

            Error[] errors = validationResult.Errors
                .ConvertAll(e => Error.Failure(e.PropertyName, e.ErrorMessage))
                .ToArray();

            return Result<TResponse>.ValidationFailure(new ValidationError(errors));
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> decorated,
        IServiceProvider serviceProvider) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            IValidator<TCommand>? validator = serviceProvider.GetService<IValidator<TCommand>>();

            if (validator is null)
            {
                return await decorated.Handle(command, cancellationToken);
            }

            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);

            if (validationResult.IsValid)
            {
                return await decorated.Handle(command, cancellationToken);
            }

            Error[] errors = validationResult.Errors
                .ConvertAll(e => Error.Failure(e.PropertyName, e.ErrorMessage))
                .ToArray();

            return Result.Failure(new ValidationError(errors));
        }
    }
}
