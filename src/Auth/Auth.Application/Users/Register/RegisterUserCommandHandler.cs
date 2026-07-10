using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Data;
using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Roles;
using Auth.Domain.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Auth.Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IAuthDbContext context, IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHasher.Hash(command.Password)
        };

        Role? standardRole = await context.Roles
            .SingleOrDefaultAsync(r => r.Name == "Standard", cancellationToken);

        if (standardRole is null)
        {
            return Result.Failure<Guid>(UserErrors.RoleNotFound);
        }

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        context.Users.Add(user);
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = standardRole.Id });

        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
