using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Auth.IntegrationTests;

[CollectionDefinition("Auth")]
public sealed class AuthCollectionDefinition;
