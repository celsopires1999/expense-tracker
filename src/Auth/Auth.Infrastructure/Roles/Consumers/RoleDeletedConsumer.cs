using Auth.Application.Abstractions.Data;
using Auth.Domain.Roles;
using MassTransit;
using SharedKernel.Messaging;

namespace Auth.Infrastructure.Roles.Consumers;

internal sealed class RoleDeletedConsumer(IAuthDbContext authDbContext) : IConsumer<RoleDeletedEvent>
{
    public async Task Consume(ConsumeContext<RoleDeletedEvent> context)
    {
        RoleDeletedEvent message = context.Message;

        Role? existing = await authDbContext.Roles.FindAsync([message.Id]);

        if (existing is not null)
        {
            authDbContext.Roles.Remove(existing);
            await authDbContext.SaveChangesAsync();
        }
    }
}
