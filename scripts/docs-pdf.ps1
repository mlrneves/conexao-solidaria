# Gera os PDFs dos entregáveis (docs/pdf/*.pdf) a partir dos Markdown.
# Usa: npx marked (Node) para MD -> HTML e Microsoft Edge headless para HTML -> PDF.
# Pré-requisitos: Node.js e Edge (padrão no Windows 11).
# Uso: .\scripts\docs-pdf.ps1
$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot
Set-Location $raiz

$edge = @(
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe",
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $edge) { throw "Microsoft Edge não encontrado." }

New-Item -ItemType Directory -Force "docs/pdf" | Out-Null

$css = @"
body { font-family: 'Segoe UI', sans-serif; max-width: 800px; margin: 2rem auto; line-height: 1.6; color: #222; }
h1, h2, h3 { color: #b02a5b; }
table { border-collapse: collapse; width: 100%; margin: 1rem 0; }
th, td { border: 1px solid #ccc; padding: 6px 10px; text-align: left; font-size: 0.92rem; }
th { background: #f6f0f2; }
code { background: #f4f4f4; padding: 1px 5px; border-radius: 4px; font-size: 0.9em; }
pre { background: #f4f4f4; padding: 12px; border-radius: 8px; overflow-x: auto; }
blockquote { border-left: 4px solid #d6336c; margin-left: 0; padding-left: 1rem; color: #555; }
"@

foreach ($md in @("docs/BANCOS.md", "docs/RELATORIO-ENTREGA.md", "docs/ARQUITETURA.md")) {
    $nome = [IO.Path]::GetFileNameWithoutExtension($md)
    Write-Host "==> $md -> docs/pdf/$nome.pdf" -ForegroundColor Cyan

    $corpo = npx -y marked --gfm (Get-Content $md -Raw -Encoding utf8)
    $html = "<!DOCTYPE html><html lang='pt-BR'><head><meta charset='utf-8'><style>$css</style></head><body>$corpo</body></html>"

    $tmp = Join-Path $env:TEMP "$nome.html"
    Set-Content -Path $tmp -Value $html -Encoding utf8

    & $edge --headless --disable-gpu --no-pdf-header-footer `
        --print-to-pdf="$raiz\docs\pdf\$nome.pdf" "file:///$tmp" 2>$null | Out-Null
}

Write-Host "`nPDFs gerados em docs/pdf/" -ForegroundColor Green
Get-ChildItem docs/pdf
