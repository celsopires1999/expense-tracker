using System.Globalization;
using System.Security.Cryptography;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TestShared;

public sealed class PostgreSqlFixture
{
    private static readonly Lazy<PostgreSqlFixture> InstanceLazy = new(() => new PostgreSqlFixture());

    public static PostgreSqlFixture Instance => InstanceLazy.Value;

    private readonly Lazy<PostgreSqlContainer> _containerLazy = new(() =>
        new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase("integration_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build());

    private readonly Lazy<RSA> _rsaLazy = new(RSA.Create);

    private readonly Lazy<string> _privateKeyPathLazy;

    private readonly Lazy<string> _publicKeyPathLazy;

    private PostgreSqlFixture()
    {
        _privateKeyPathLazy = new Lazy<string>(() =>
        {
            string path = Path.Combine(Path.GetTempPath(), $"auth-test-{Guid.NewGuid():N}-priv.pem");
            File.WriteAllText(path, _rsaLazy.Value.ExportPkcs8PrivateKeyPem());
            return path;
        });

        _publicKeyPathLazy = new Lazy<string>(() =>
        {
            string path = Path.Combine(Path.GetTempPath(), $"auth-test-{Guid.NewGuid():N}-pub.pem");
            File.WriteAllText(path, _rsaLazy.Value.ExportSubjectPublicKeyInfoPem());
            return path;
        });
    }

    public string DefaultConnectionString => _containerLazy.Value.GetConnectionString();

    public string PrivateKey => _privateKeyPathLazy.Value;

    public string PublicKey => _publicKeyPathLazy.Value;

    public async Task InitializeAsync()
    {
        await _containerLazy.Value.StartAsync();
    }

    public string GetConnectionString(string dbName)
    {
        var builder = new NpgsqlConnectionStringBuilder(DefaultConnectionString)
        {
            Database = dbName
        };
        return builder.ConnectionString;
    }

    public async Task CreateDatabaseAsync(string dbName)
    {
        var baseBuilder = new NpgsqlConnectionStringBuilder(DefaultConnectionString)
        {
            Database = "postgres"
        };

        await using NpgsqlConnection connection = new(baseBuilder.ConnectionString);
        await connection.OpenAsync();

        const string sql = "CREATE DATABASE \"{0}\"";
        string commandText = string.Format(CultureInfo.InvariantCulture, sql, dbName);

        //dbName is a controlled constant, not user input
        #pragma warning disable CA2100
        await using NpgsqlCommand command = new(commandText, connection);
        #pragma warning restore CA2100
        await command.ExecuteNonQueryAsync();
    }
}
