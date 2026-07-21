# Remove a plataforma do cluster (mantém o stack de observabilidade;
# use -TudoInclusiveObservabilidade para remover o monitoring também).
param([switch]$TudoInclusiveObservabilidade)
$ErrorActionPreference = "Continue"

kubectl delete namespace conexao-solidaria --ignore-not-found

if ($TudoInclusiveObservabilidade) {
    helm uninstall kps -n monitoring 2>$null
    kubectl delete namespace monitoring --ignore-not-found
}

Write-Host "Pronto." -ForegroundColor Green
