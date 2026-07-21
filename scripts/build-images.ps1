# Builda as 4 imagens locais usadas pelos manifests do k8s/ (tag :local).
# O Kubernetes do Docker Desktop compartilha o daemon — sem push necessário.
# Uso: .\scripts\build-images.ps1
# O docker escreve o progresso do build em stderr; sem este ajuste o
# PowerShell trata essas linhas como erro e aborta o script.
$ErrorActionPreference = "Continue"
$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz

$imagens = @(
    @{ Nome = "conexao-solidaria/campanhas-api:local";  Dockerfile = "src/Services/Campanhas/Campanhas.Api/Dockerfile" },
    @{ Nome = "conexao-solidaria/doacoes-api:local";    Dockerfile = "src/Services/Doacoes/Doacoes.Api/Dockerfile" },
    @{ Nome = "conexao-solidaria/doacoes-worker:local"; Dockerfile = "src/Services/Doacoes/Doacoes.Worker/Dockerfile" },
    @{ Nome = "conexao-solidaria/web:local";            Dockerfile = "src/Web/ConexaoSolidaria.Web/Dockerfile" }
)

foreach ($img in $imagens) {
    Write-Host "==> docker build $($img.Nome)" -ForegroundColor Cyan
    docker build -f $img.Dockerfile -t $img.Nome .
    if ($LASTEXITCODE -ne 0) { throw "Falha no build de $($img.Nome)" }
}

Write-Host "`nImagens prontas:" -ForegroundColor Green
docker images "conexao-solidaria/*"
