# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/apps/SemanticDocIngestor.AppHost/SemanticDocIngestor.AppHost.csproj", "src/apps/SemanticDocIngestor.AppHost/"]
COPY ["src/apps/SemanticDocIngestor.AppHost.ApiService/SemanticDocIngestor.AppHost.ApiService.csproj", "src/apps/SemanticDocIngestor.AppHost.ApiService/"]
COPY ["src/apps/SemanticDocIngestor.AppHost.ServiceDefaults/SemanticDocIngestor.AppHost.ServiceDefaults.csproj", "src/apps/SemanticDocIngestor.AppHost.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "src/apps/SemanticDocIngestor.AppHost/SemanticDocIngestor.AppHost.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/src/apps/SemanticDocIngestor.AppHost"
RUN dotnet build "SemanticDocIngestor.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SemanticDocIngestor.AppHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SemanticDocIngestor.AppHost.dll"]
