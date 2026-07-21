# Instala o stack de observabilidade (Prometheus + Grafana) via Helm e
# aplica os ServiceMonitors + dashboard da plataforma.
# Pré-requisito: helm (winget install Helm.Helm)
# Uso: .\scripts\obs-up.ps1
$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz

Write-Host "==> Adicionando repositório de charts" -ForegroundColor Cyan
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

Write-Host "`n==> Instalando kube-prometheus-stack (primeira vez demora ~3 min)" -ForegroundColor Cyan
helm upgrade --install kps prometheus-community/kube-prometheus-stack `
    --namespace monitoring --create-namespace `
    -f k8s/observability/values-kps.yaml `
    --wait --timeout 10m

Write-Host "`n==> ServiceMonitors + dashboard da plataforma" -ForegroundColor Cyan
kubectl apply -f k8s/observability/servicemonitors.yaml
kubectl apply -f k8s/observability/grafana-dashboard.yaml

Write-Host @"

Grafana: http://localhost:30300  (admin / grafana-dev)
  - Dashboard custom: 'Conexão Solidária — Plataforma'
  - Prontos do stack: Kubernetes / Compute Resources / Namespace (Pods)
Targets do Prometheus: aguarde ~1 min e confira em
  kubectl port-forward svc/kps-kube-prometheus-stack-prometheus 9090:9090 -n monitoring
  -> http://localhost:9090/targets (3 ServiceMonitors do namespace conexao-solidaria)
"@ -ForegroundColor Green
