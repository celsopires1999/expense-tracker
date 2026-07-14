FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/SharedKernel/SharedKernel.csproj src/SharedKernel/
COPY src/Permission/Permission.Domain/Permission.Domain.csproj src/Permission/Permission.Domain/
COPY src/Permission/Permission.Application/Permission.Application.csproj src/Permission/Permission.Application/
COPY src/Permission/Permission.Infrastructure/Permission.Infrastructure.csproj src/Permission/Permission.Infrastructure/
COPY src/Permission/Permission.Api/Permission.Api.csproj src/Permission/Permission.Api/

RUN dotnet restore src/Permission/Permission.Api/Permission.Api.csproj

COPY . .
RUN dotnet publish src/Permission/Permission.Api/Permission.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER 1000

COPY --from=build --chown=1000:1000 /app/publish .
ENTRYPOINT ["dotnet", "Permission.Api.dll"]
