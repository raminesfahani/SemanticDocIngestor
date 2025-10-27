# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/apps/SemanticDocIngestor.AppHost.ApiService/SemanticDocIngestor.AppHost.ApiService.csproj", "src/apps/SemanticDocIngestor.AppHost.ApiService/"]
COPY ["src/apps/SemanticDocIngestor.AppHost.ServiceDefaults/SemanticDocIngestor.AppHost.ServiceDefaults.csproj", "src/apps/SemanticDocIngestor.AppHost.ServiceDefaults/"]
COPY ["src/sdk/SemanticDocIngestor.Core/SemanticDocIngestor.Core.csproj", "src/sdk/SemanticDocIngestor.Core/"]
COPY ["src/sdk/SemanticDocIngestor.Domain/SemanticDocIngestor.Domain.csproj", "src/sdk/SemanticDocIngestor.Domain/"]
COPY ["src/sdk/SemanticDocIngestor.Infrastructure/SemanticDocIngestor.Infrastructure.csproj", "src/sdk/SemanticDocIngestor.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/apps/SemanticDocIngestor.AppHost.ApiService/SemanticDocIngestor.AppHost.ApiService.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/src/apps/SemanticDocIngestor.AppHost.ApiService"
RUN dotnet build "SemanticDocIngestor.AppHost.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SemanticDocIngestor.AppHost.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SemanticDocIngestor.AppHost.ApiService.dll"]
