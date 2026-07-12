using Testcontainers.RabbitMq;

namespace TestShared;

public sealed class RabbitMQFixture
{
    private static readonly Lazy<RabbitMQFixture> InstanceLazy = new(() => new RabbitMQFixture());

    public static RabbitMQFixture Instance => InstanceLazy.Value;

    private readonly Lazy<RabbitMqContainer> _containerLazy = new(() =>
        new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .Build());

    public string Host => _containerLazy.Value.Hostname;

    public int Port => _containerLazy.Value.GetMappedPublicPort(5672);

    public string AmqpUri => _containerLazy.Value.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _containerLazy.Value.StartAsync();
    }
}
