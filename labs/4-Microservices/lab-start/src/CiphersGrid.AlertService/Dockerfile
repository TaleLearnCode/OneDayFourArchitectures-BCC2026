FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY CiphersGrid.sln .
COPY src/CiphersGrid.SharedKernel/CiphersGrid.SharedKernel.csproj src/CiphersGrid.SharedKernel/
COPY src/CiphersGrid.AlertService/CiphersGrid.AlertService.csproj src/CiphersGrid.AlertService/

RUN dotnet restore src/CiphersGrid.AlertService/CiphersGrid.AlertService.csproj
COPY . .
RUN dotnet publish -c Release -o /app/publish src/CiphersGrid.AlertService/CiphersGrid.AlertService.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CiphersGrid.AlertService.dll"]
