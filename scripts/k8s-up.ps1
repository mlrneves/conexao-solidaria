# Sobe a plataforma completa no Kubernetes local (Docker Desktop).
# Idempotente: pode rodar quantas vezes quiser.
# Uso: .\scripts\k8s-up.ps1            (aplica tudo)
#      .\scripts\k8s-up.ps1 -SkipBuild (não rebuilda as imagens)
param([switch]$SkipBuild)
$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz

if (-not $SkipBuild) {
    & "$PSScriptRoot\build-images.ps1"
}

Write-Host "`n==> Namespace + config" -ForegroundColor Cyan
kubectl apply -f k8s/00-namespace.yaml
kubectl apply -f k8s/config/

Write-Host "`n==> Infraestrutura (Postgres, MongoDB, RabbitMQ)" -ForegroundColor Cyan
kubectl apply -f k8s/infra/

Write-Host "`n==> Aguardando infraestrutura ficar pronta (até 5 min)..." -ForegroundColor Cyan
kubectl wait --for=condition=ready pod -l app=postgres -n conexao-solidaria --timeout=300s
kubectl wait --for=condition=ready pod -l app=mongodb -n conexao-solidaria --timeout=300s
kubectl wait --for=condition=ready pod -l app=rabbitmq -n conexao-solidaria --timeout=300s

Write-Host "`n==> Aplicações (APIs, Worker, Web)" -ForegroundColor Cyan
kubectl apply -f k8s/apps/

Write-Host "`n==> Status:" -ForegroundColor Cyan
kubectl get pods -n conexao-solidaria

Write-Host @"

URLs (Docker Desktop expõe NodePort em localhost):
  Web (Blazor)....: http://localhost:30080
  Campanhas.Api...: http://localhost:30081/swagger
  Doacoes.Api.....: http://localhost:30082/swagger
  RabbitMQ UI.....: http://localhost:30672  (conexao / conexao-dev)
  Grafana.........: http://localhost:30300  (apos scripts/obs-up.ps1)

Login seed do gestor: gestor@conexaosolidaria.org / Gestor@123!
"@ -ForegroundColor Green
