using Permission.Application.Abstractions.Messaging;

namespace Permission.Application.Roles.Resolve;

public sealed record ResolvePermissionsQuery(string[] Roles) : IQuery<HashSet<string>>;
