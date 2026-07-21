# Contexto de build: RAIZ do repositório.
#   docker build -f src/Web/ConexaoSolidaria.Web/Dockerfile -t conexao-web .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Web/ConexaoSolidaria.Web/ConexaoSolidaria.Web.csproj src/Web/ConexaoSolidaria.Web/
RUN dotnet restore src/Web/ConexaoSolidaria.Web/ConexaoSolidaria.Web.csproj

COPY src/Web/ src/Web/
RUN dotnet publish src/Web/ConexaoSolidaria.Web/ConexaoSolidaria.Web.csproj -c Release -o /app --no-restore

# Estáticos servidos por nginx — container pequeno e sem runtime .NET.
FROM nginx:alpine AS final
COPY src/Web/ConexaoSolidaria.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/wwwroot /usr/share/nginx/html
EXPOSE 80
