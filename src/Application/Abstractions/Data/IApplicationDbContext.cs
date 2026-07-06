using Domain.Categories;
using Domain.Expenses;
using Domain.PaymentMethods;
using Domain.Roles;
using Domain.Tags;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<PaymentMethod> PaymentMethods { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<ExpenseTag> ExpenseTags { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
