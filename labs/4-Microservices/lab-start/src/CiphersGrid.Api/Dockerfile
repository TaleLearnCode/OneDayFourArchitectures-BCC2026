FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY CiphersGrid.sln .
COPY src/CiphersGrid.SharedKernel/CiphersGrid.SharedKernel.csproj src/CiphersGrid.SharedKernel/
COPY src/CiphersGrid.Api/CiphersGrid.Api.csproj src/CiphersGrid.Api/

RUN dotnet restore src/CiphersGrid.Api/CiphersGrid.Api.csproj
COPY . .
RUN dotnet publish -c Release -o /app/publish src/CiphersGrid.Api/CiphersGrid.Api.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CiphersGrid.Api.dll"]
