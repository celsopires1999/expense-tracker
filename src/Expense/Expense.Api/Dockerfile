FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/SharedKernel/SharedKernel.csproj src/SharedKernel/
COPY src/Expense/Expense.Domain/Expense.Domain.csproj src/Expense/Expense.Domain/
COPY src/Expense/Expense.Application/Expense.Application.csproj src/Expense/Expense.Application/
COPY src/Expense/Expense.Infrastructure/Expense.Infrastructure.csproj src/Expense/Expense.Infrastructure/
COPY src/Expense/Expense.Api/Expense.Api.csproj src/Expense/Expense.Api/

RUN dotnet restore src/Expense/Expense.Api/Expense.Api.csproj

COPY . .
RUN dotnet publish src/Expense/Expense.Api/Expense.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER 1000

COPY --from=build --chown=1000:1000 /app/publish .
ENTRYPOINT ["dotnet", "Expense.Api.dll"]
