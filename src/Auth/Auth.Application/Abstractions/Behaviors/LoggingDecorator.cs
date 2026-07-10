using Auth.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SharedKernel;

namespace Auth.Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> decorated,
        ILogger<CommandHandler<TCommand, TResponse>> logger) : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Command", command.GetType().Name))
            {
                logger.LogInformation("Executing command {Command}", typeof(TCommand).Name);
                Result<TResponse> result = await decorated.Handle(command, cancellationToken);
                logger.LogInformation("Command {Command} completed with {IsSuccess}", typeof(TCommand).Name, result.IsSuccess);
                return result;
            }
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> decorated,
        ILogger<CommandBaseHandler<TCommand>> logger) : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Command", command.GetType().Name))
            {
                logger.LogInformation("Executing command {Command}", typeof(TCommand).Name);
                Result result = await decorated.Handle(command, cancellationToken);
                logger.LogInformation("Command {Command} completed with {IsSuccess}", typeof(TCommand).Name, result.IsSuccess);
                return result;
            }
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> decorated,
        ILogger<QueryHandler<TQuery, TResponse>> logger) : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("Query", query.GetType().Name))
            {
                logger.LogInformation("Executing query {Query}", typeof(TQuery).Name);
                Result<TResponse> result = await decorated.Handle(query, cancellationToken);
                logger.LogInformation("Query {Query} completed with {IsSuccess}", typeof(TQuery).Name, result.IsSuccess);
                return result;
            }
        }
    }
}
