# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for better caching
COPY ["Conciliacao.slnx", "./"]
COPY ["Conciliacao.Api/Conciliacao.Api.csproj", "Conciliacao.Api/"]
COPY ["Conciliacao.Application/Conciliacao.Application.csproj", "Conciliacao.Application/"]
COPY ["Conciliacao.Domain/Conciliacao.Domain.csproj", "Conciliacao.Domain/"]
COPY ["Conciliacao.Infra/Conciliacao.Infra.csproj", "Conciliacao.Infra/"]
COPY ["Conciliacao.Api.Tests/Conciliacao.Api.Tests.csproj", "Conciliacao.Api.Tests/"]
COPY ["Conciliacao.Domain.Tests/Conciliacao.Domain.Tests.csproj", "Conciliacao.Domain.Tests/"]

# Restore dependencies
RUN dotnet restore "Conciliacao.Api/Conciliacao.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Conciliacao.Api"
RUN dotnet build "Conciliacao.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Conciliacao.Api.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conciliacao.Api.dll"]
