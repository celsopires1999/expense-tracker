using Auth.Application.Abstractions.Data;
using Auth.Domain.Roles;
using MassTransit;
using SharedKernel.Messaging;

namespace Auth.Infrastructure.Roles.Consumers;

internal sealed class RoleUpdatedConsumer(IAuthDbContext authDbContext) : IConsumer<RoleUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RoleUpdatedEvent> context)
    {
        RoleUpdatedEvent message = context.Message;

        Role? existing = await authDbContext.Roles.FindAsync([message.Id]);

        if (existing is not null)
        {
            existing.Name = message.Name;
        }
        else
        {
            authDbContext.Roles.Add(new Role
            {
                Id = message.Id,
                Name = message.Name
            });
        }

        await authDbContext.SaveChangesAsync();
    }
}
