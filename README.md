# Conexão Solidária 💗

![CI](https://github.com/SEU_USUARIO/SEU_REPO/actions/workflows/ci.yml/badge.svg)

MVP da plataforma de doações da **ONG Esperança Solidária** — hackathon da
pós-graduação (Fase 5: Agilidade, Segurança e IA).

A ONG gerencia doadores e campanhas manualmente; este MVP digitaliza o
processo com foco em **escalabilidade, observabilidade e automação**:
microsserviços .NET 9, mensageria assíncrona (RabbitMQ), persistência
poliglota (PostgreSQL + MongoDB), Kubernetes, Prometheus + Grafana, CI/CD e
front-end Blazor WebAssembly.

## Arquitetura (resumo)

```
Blazor WASM ──► Campanhas.Api (JWT/RBAC, campanhas, painel público) ──► PostgreSQL
     │                 ▲ valida status da campanha (HTTP)
     └─────────► Doacoes.Api ── grava Pendente ──► MongoDB
                       │ publica DoacaoRecebidaEvent (202 Accepted)
                       ▼
                RabbitMQ (exchange conexao.eventos → fila + DLQ)
                       │ consome (ack manual)
                Doacoes.Worker ──► incrementa total (PostgreSQL, idempotente)
                               ──► marca Processada/Rejeitada (MongoDB)
Prometheus raspa /metrics de todos → Grafana (dashboards)
```

**A API de doações nunca atualiza o total arrecadado** — publica o evento e o
Worker consome a fila e faz o incremento (requisito central do hackathon).
Diagrama completo e decisões: [docs/ARQUITETURA.md](docs/ARQUITETURA.md).

| Documento | Conteúdo |
|---|---|
| [docs/ARQUITETURA.md](docs/ARQUITETURA.md) | Diagrama, fluxo da doação 1→7, decisões e trade-offs |
| [docs/BANCOS.md](docs/BANCOS.md) | Justificativa PostgreSQL + MongoDB (entregável em PDF) |
| [docs/SOLID.md](docs/SOLID.md) | Mapeamento princípio → classe do projeto |
| [docs/LGPD.md](docs/LGPD.md) | Privacidade: minimização, consentimento, CPF mascarado |
| [docs/AGIL.md](docs/AGIL.md) | Kanban, XP, Lean aplicados ao trabalho |
| [docs/ROTEIRO-VIDEO.md](docs/ROTEIRO-VIDEO.md) | Roteiro do vídeo de demonstração |

## Pré-requisitos (Windows)

1. **WSL2**: `wsl --install` (PowerShell como admin; pode pedir reboot).
2. **Docker Desktop**: <https://www.docker.com/products/docker-desktop/> —
   após instalar, abra **Settings → Kubernetes → Enable Kubernetes** (a opção
   de cluster usada neste projeto).
3. **kubectl + Helm**: `winget install Kubernetes.kubectl Helm.Helm`
   (o Docker Desktop já instala um kubectl; o Helm é necessário para a
   observabilidade).
4. **.NET SDK 9** (apenas para rodar testes/dev): <https://dotnet.microsoft.com/download/dotnet/9.0>

Verifique: `docker version` e `kubectl get nodes` devem responder.

## Opção A — Subir tudo com Docker Compose (caminho rápido)

```powershell
docker compose -f docker-compose.full.yml up --build -d
```

| Serviço | URL |
|---|---|
| Web (Blazor) | http://localhost:8080 |
| Campanhas.Api (Swagger) | http://localhost:8081/swagger |
| Doacoes.Api (Swagger) | http://localhost:8082/swagger |
| Worker (métricas) | http://localhost:8083/metrics |
| RabbitMQ Management | http://localhost:15672 (`conexao` / `conexao-dev`) |

Para derrubar: `docker compose -f docker-compose.full.yml down -v`

## Opção B — Kubernetes (requisito do hackathon)

```powershell
# 1. Builda as imagens e aplica todos os manifests (namespace, config, infra, apps)
.\scripts\k8s-up.ps1

# 2. Observabilidade (Prometheus + Grafana via Helm — primeira vez ~3 min)
.\scripts\obs-up.ps1
```

Ou manualmente, na ordem:

```powershell
.\scripts\build-images.ps1
kubectl apply -f k8s/00-namespace.yaml
kubectl apply -f k8s/config/
kubectl apply -f k8s/infra/
kubectl wait --for=condition=ready pod --all -n conexao-solidaria --timeout=300s
kubectl apply -f k8s/apps/
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm upgrade --install kps prometheus-community/kube-prometheus-stack -n monitoring --create-namespace -f k8s/observability/values-kps.yaml
kubectl apply -f k8s/observability/servicemonitors.yaml
kubectl apply -f k8s/observability/grafana-dashboard.yaml
```

Confirme: `kubectl get pods -n conexao-solidaria` — tudo `Running/READY`.

| Serviço | URL |
|---|---|
| Web (Blazor) | http://localhost:30080 |
| Campanhas.Api (Swagger) | http://localhost:30081/swagger |
| Doacoes.Api (Swagger) | http://localhost:30082/swagger |
| RabbitMQ Management | http://localhost:30672 (`conexao` / `conexao-dev`) |
| Grafana | http://localhost:30300 (`admin` / `grafana-dev`) |

Para derrubar: `.\scripts\k8s-down.ps1`

## Credenciais seed

| Perfil | E-mail | Senha |
|---|---|---|
| **GestorONG** | `gestor@conexaosolidaria.org` | `Gestor@123!` |

Doadores são criados pelo cadastro público (UI ou `POST /api/auth/registrar`).

## Roteiro de teste manual (10 passos)

Coleção pronta: [docs/api/requests.http](docs/api/requests.http) (VS Code REST
Client) ou [docs/api/ConexaoSolidaria.postman_collection.json](docs/api/ConexaoSolidaria.postman_collection.json)
(Postman — o login preenche os tokens sozinho).

1. `POST /api/auth/login` (gestor seed) → copie o `accessToken` (role `GestorONG` — confira no jwt.io).
2. No Swagger 30081, clique **Authorize** e cole o token.
3. `POST /api/campanhas` com `dataFim` no passado → **400** (regra de negócio).
4. `POST /api/campanhas` válida → **201**. Guarde o `id`.
5. `POST /api/auth/registrar` com CPF `111.111.111-11` → **400**; com CPF
   válido (ex.: `529.982.247-25`) e `consentimentoLgpd: true` → **201**.
6. Login do doador → **Authorize** no Swagger 30082.
7. `GET /api/publico/campanhas` (sem token) → campanha com arrecadado `0`.
8. *(cena da fila)* `kubectl scale deployment doacoes-worker --replicas=0 -n conexao-solidaria`
   → `POST /api/doacoes` → **202** → veja a mensagem parada na fila
   `doacoes.processamento` (RabbitMQ UI) → `--replicas=1` → mensagem consumida.
9. `GET /api/publico/campanhas` → **valor atualizado pelo Worker**;
   `GET /api/doacoes/minhas` → status `Processada`.
10. Cancele a campanha (`PUT` do gestor) e tente doar → **422**.

## Testes de unidade (bônus — rodam também no CI)

```powershell
dotnet test ConexaoSolidaria.sln
```

35 testes xUnit: regras de campanha e CPF (domínio), fluxo de doação e
idempotência do worker (com mocks NSubstitute).

## CI/CD

Pipeline em [.github/workflows/ci.yml](.github/workflows/ci.yml), acionado a
cada push na `main`: build .NET → testes xUnit → build das 4 imagens Docker →
push no **GHCR** (`ghcr.io/<owner>/conexao-solidaria/*`, tags `latest` + SHA).

> Deploy automatizado no cluster é **opcional** no edital e não se aplica:
> o cluster é local (inalcançável pelo runner). O critério obrigatório —
> imagem gerada no CI — está coberto. Localmente os manifests usam as imagens
> `:local` (`scripts/build-images.ps1`); para usar as do GHCR, troque o campo
> `image:` nos YAMLs de `k8s/apps/`.

## Estrutura do repositório

```
src/BuildingBlocks/    Contracts (eventos), Messaging (IEventBus/RabbitMQ), Auth (JWT compartilhado)
src/Services/Campanhas Domain / Infrastructure (EF+Postgres) / Api
src/Services/Doacoes   Api (Mongo + publisher) / Worker (consumer + incremento)
src/Web/               Blazor WASM (nginx)
tests/                 xUnit (domínio + serviços com mocks)
k8s/                   Manifests: config, infra, apps, observability
scripts/               build-images, k8s-up/down, obs-up, docs-pdf
docs/                  Arquitetura, bancos, SOLID, LGPD, ágil, roteiro do vídeo, collections
```

## Troubleshooting

| Sintoma | Solução |
|---|---|
| `ImagePullBackOff` | Rode `.\scripts\build-images.ps1` (manifests usam imagens locais `:local`). |
| Pod da API reiniciando no começo | Normal: espera o Postgres/RabbitMQ ficarem prontos (retry embutido). Aguarde os probes. |
| Grafana sem os targets | Aguarde ~1 min; confira que `obs-up.ps1` aplicou os ServiceMonitors. O values já traz `serviceMonitorSelectorNilUsesHelmValues=false`. |
| Porta 30080-30672 ocupada | Ajuste o `nodePort` no YAML correspondente. |
| Cluster K8s corrompido | Docker Desktop → Settings → Kubernetes → **Reset Kubernetes Cluster** e rode `k8s-up.ps1` de novo. |
| CORS no browser | A origem do front precisa estar em `Cors__AllowedOrigins__0` (ConfigMap `app-config`). |
