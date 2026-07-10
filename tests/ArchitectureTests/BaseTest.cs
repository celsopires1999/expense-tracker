using System.Reflection;
using Expense.Api;
using Expense.Application.Abstractions.Messaging;
using Expense.Domain.Expenses;
using Expense.Infrastructure.Database;

namespace ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(Expense.Domain.Expenses.Expense).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(ICommand).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
