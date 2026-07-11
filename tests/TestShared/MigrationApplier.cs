using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace TestShared;

public static class MigrationApplier
{
    public static async Task ApplyMigrationsAsync<TContext>(IServiceProvider services)
        where TContext : DbContext
    {
        using IServiceScope scope = services.CreateScope();
        _ = scope.ServiceProvider.GetRequiredService<TContext>();
        IMigrator migrator = scope.ServiceProvider.GetRequiredService<IMigrator>();
        await migrator.MigrateAsync();
    }
}
