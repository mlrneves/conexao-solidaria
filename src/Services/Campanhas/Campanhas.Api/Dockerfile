# Contexto de build: RAIZ do repositório (por causa dos BuildingBlocks).
#   docker build -f src/Services/Campanhas/Campanhas.Api/Dockerfile -t campanhas-api .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro: o restore vira camada cacheada.
COPY Directory.Build.props ./
COPY src/BuildingBlocks/BuildingBlocks.Auth/BuildingBlocks.Auth.csproj src/BuildingBlocks/BuildingBlocks.Auth/
COPY src/Services/Campanhas/Campanhas.Domain/Campanhas.Domain.csproj src/Services/Campanhas/Campanhas.Domain/
COPY src/Services/Campanhas/Campanhas.Infrastructure/Campanhas.Infrastructure.csproj src/Services/Campanhas/Campanhas.Infrastructure/
COPY src/Services/Campanhas/Campanhas.Api/Campanhas.Api.csproj src/Services/Campanhas/Campanhas.Api/
RUN dotnet restore src/Services/Campanhas/Campanhas.Api/Campanhas.Api.csproj

COPY src/ src/
RUN dotnet publish src/Services/Campanhas/Campanhas.Api/Campanhas.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "Campanhas.Api.dll"]
