using MassTransit;
using Permission.Domain.Roles;
using SharedKernel;
using SharedKernel.Messaging;

namespace Permission.Application.Roles.DomainEvents;

internal sealed class RoleDeletedDomainEventHandler(IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<RoleDeletedDomainEvent>
{
    public async Task Handle(RoleDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
            new RoleDeletedEvent(domainEvent.RoleId, DateTime.UtcNow),
            cancellationToken);
    }
}
