# Build environment for .NET services
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy solution and projects
COPY CiphersGrid.sln .
COPY src/CiphersGrid.SharedKernel/CiphersGrid.SharedKernel.csproj src/CiphersGrid.SharedKernel/
COPY src/CiphersGrid.Api/CiphersGrid.Api.csproj src/CiphersGrid.Api/
COPY src/CiphersGrid.CrewService/CiphersGrid.CrewService.csproj src/CiphersGrid.CrewService/
COPY src/CiphersGrid.RaceService/CiphersGrid.RaceService.csproj src/CiphersGrid.RaceService/
COPY src/CiphersGrid.TelemetryService/CiphersGrid.TelemetryService.csproj src/CiphersGrid.TelemetryService/
COPY src/CiphersGrid.AlertService/CiphersGrid.AlertService.csproj src/CiphersGrid.AlertService/

# Restore
RUN dotnet restore CiphersGrid.sln

# Copy source
COPY . .

# Build
RUN dotnet build -c Release --no-restore

# Publish
RUN dotnet publish -c Release -o /app/publish --no-build

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/publish .

ENTRYPOINT ["dotnet", "CiphersGrid.Api.dll"]
