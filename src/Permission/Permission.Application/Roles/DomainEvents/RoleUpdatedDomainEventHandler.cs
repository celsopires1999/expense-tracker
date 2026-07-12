using MassTransit;
using Permission.Application.Abstractions.Data;
using Permission.Domain.Roles;
using SharedKernel;
using SharedKernel.Messaging;

namespace Permission.Application.Roles.DomainEvents;

internal sealed class RoleUpdatedDomainEventHandler(IPermissionDbContext context, IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<RoleUpdatedDomainEvent>
{
    public async Task Handle(RoleUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Role? role = await context.Roles.FindAsync([domainEvent.RoleId], cancellationToken);

        if (role is null)
        {
            return;
        }

        await publishEndpoint.Publish(
            new RoleUpdatedEvent(role.Id, role.Name, DateTime.UtcNow),
            cancellationToken);
    }
}
