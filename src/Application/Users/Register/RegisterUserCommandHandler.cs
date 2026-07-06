using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
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
