# Contexto de build: RAIZ do repositório.
#   docker build -f src/Services/Doacoes/Doacoes.Worker/Dockerfile -t doacoes-worker .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj src/BuildingBlocks/BuildingBlocks.Contracts/
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/Services/Doacoes/Doacoes.Worker/Doacoes.Worker.csproj src/Services/Doacoes/Doacoes.Worker/
RUN dotnet restore src/Services/Doacoes/Doacoes.Worker/Doacoes.Worker.csproj

COPY src/ src/
RUN dotnet publish src/Services/Doacoes/Doacoes.Worker/Doacoes.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Doacoes.Worker.dll"]
