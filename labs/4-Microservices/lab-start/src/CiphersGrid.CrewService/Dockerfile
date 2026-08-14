FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY CiphersGrid.sln .
COPY src/CiphersGrid.SharedKernel/CiphersGrid.SharedKernel.csproj src/CiphersGrid.SharedKernel/
COPY src/CiphersGrid.CrewService/CiphersGrid.CrewService.csproj src/CiphersGrid.CrewService/

RUN dotnet restore src/CiphersGrid.CrewService/CiphersGrid.CrewService.csproj
COPY . .
RUN dotnet publish -c Release -o /app/publish src/CiphersGrid.CrewService/CiphersGrid.CrewService.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CiphersGrid.CrewService.dll"]
