# Abre os túneis de acesso às interfaces da aplicação rodando no Kubernetes.
#
# Por que port-forward e não NodePort: as versões recentes do Docker Desktop
# executam o cluster dentro de um container isolado que NÃO publica a faixa de
# NodePorts no host Windows. Os Services continuam declarados como NodePort
# (exigência de entrega), mas o acesso garantido em qualquer máquina é este.
#
# Uso: .\scripts\k8s-portforward.ps1     (Ctrl+C encerra todos os túneis)
$ErrorActionPreference = "Continue"
$ns = "conexao-solidaria"

$tuneis = @(
    @{ Nome = "Campanhas.Api (Swagger)"; Svc = "svc/campanhas-api";       Portas = "8081:8080";  Url = "http://localhost:8081/swagger" },
    @{ Nome = "Doacoes.Api (Swagger)";   Svc = "svc/doacoes-api";         Portas = "8082:8080";  Url = "http://localhost:8082/swagger" },
    @{ Nome = "Frontend Blazor";         Svc = "svc/web";                 Portas = "8080:80";    Url = "http://localhost:8080" },
    @{ Nome = "RabbitMQ Management";     Svc = "svc/rabbitmq-management"; Portas = "8672:15672"; Url = "http://localhost:8672" }
)

$jobs = @()
foreach ($t in $tuneis) {
    $jobs += Start-Job -ScriptBlock {
        param($ns, $svc, $portas)
        kubectl port-forward -n $ns $svc $portas
    } -ArgumentList $ns, $t.Svc, $t.Portas
}

Start-Sleep -Seconds 5

Write-Host "`n=== Conexao Solidaria — acessos disponiveis ===" -ForegroundColor Green
foreach ($t in $tuneis) {
    Write-Host ("  {0,-26} {1}" -f $t.Nome, $t.Url) -ForegroundColor Cyan
}
Write-Host "`n  Grafana (se instalado):    http://localhost:3000  (admin / conexao-dev)" -ForegroundColor Cyan
Write-Host "  RabbitMQ login:            conexao / conexao-dev" -ForegroundColor DarkGray
Write-Host "  Gestor da ONG:             gestor@conexaosolidaria.org / Gestor@123!" -ForegroundColor DarkGray
Write-Host "`nPressione Ctrl+C para encerrar os tuneis.`n" -ForegroundColor Yellow

try {
    while ($true) { Start-Sleep -Seconds 2 }
}
finally {
    $jobs | Stop-Job -PassThru | Remove-Job -Force
    Write-Host "Tuneis encerrados." -ForegroundColor Yellow
}
