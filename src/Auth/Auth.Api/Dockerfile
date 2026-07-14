FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/SharedKernel/SharedKernel.csproj src/SharedKernel/
COPY src/Auth/Auth.Domain/Auth.Domain.csproj src/Auth/Auth.Domain/
COPY src/Auth/Auth.Application/Auth.Application.csproj src/Auth/Auth.Application/
COPY src/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj src/Auth/Auth.Infrastructure/
COPY src/Auth/Auth.Api/Auth.Api.csproj src/Auth/Auth.Api/

RUN dotnet restore src/Auth/Auth.Api/Auth.Api.csproj

COPY . .
RUN dotnet publish src/Auth/Auth.Api/Auth.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER 1000

COPY --from=build --chown=1000:1000 /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
