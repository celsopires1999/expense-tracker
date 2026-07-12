using Auth.Application.Abstractions.Data;
using Auth.Domain.Roles;
using MassTransit;
using SharedKernel.Messaging;

namespace Auth.Infrastructure.Roles.Consumers;

internal sealed class RoleCreatedConsumer(IAuthDbContext authDbContext) : IConsumer<RoleCreatedEvent>
{
    public async Task Consume(ConsumeContext<RoleCreatedEvent> context)
    {
        RoleCreatedEvent message = context.Message;

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
