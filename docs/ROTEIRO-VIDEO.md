# Roteiro do vídeo de demonstração (máx. 15 min — alvo 14:00)

Mapeado aos critérios **a–d** do edital. Regra de ouro: **não ler código linha
a linha** — comprovar arquitetura e funcionamento.

## Preparação (antes de gravar)

- [ ] Cluster no ar: `scripts/k8s-up.ps1` e `scripts/obs-up.ps1` (Grafana com ~30 min de métricas acumuladas fica mais bonito).
- [ ] Push recente na `main` com o pipeline **verde** aberto numa aba.
- [ ] Abas prontas: GitHub Actions, GHCR (packages), Swagger 30081 e 30082, RabbitMQ UI 30672, Grafana 30300 (dashboard "Conexão Solidária — Plataforma"), jwt.io, Blazor 30080.
- [ ] Terminal com fonte grande: `kubectl get pods -n conexao-solidaria`.
- [ ] **Fazer login no início da gravação** (token dura 120 min — cobre a sessão).
- [ ] Ensaiar a cena da fila: `kubectl scale deployment doacoes-worker --replicas=0 -n conexao-solidaria` (sem isso o worker consome em milissegundos e a mensagem não aparece na fila).

## Cena a cena

| Tempo | Cena | Critério |
|---|---|---|
| 0:00–1:00 | **Abertura**: o problema da ONG Esperança Solidária e o que é o MVP Conexão Solidária. | — |
| 1:00–3:00 | **Diagrama** (docs/ARQUITETURA.md): 3 microsserviços, por que 2 bancos (PG relacional/ACID; Mongo append-only), RabbitMQ com DLQ, fluxo da doação 1→7, Prometheus→Grafana (15s: "Prometheus é o coletor padrão do K8s e alimenta o Grafana exigido"). | **a** |
| 3:00–4:30 | **CI**: mostrar o workflow verde no Actions (build → testes xUnit → 4 imagens Docker) e os packages no GHCR. Mencionar: "deploy automatizado é opcional no edital; o cluster é local". | **b** |
| 4:30–6:30 | **Cluster + Grafana**: `kubectl get pods -n conexao-solidaria` (tudo Running/Ready); dashboard com CPU/memória dos pods mexendo ao vivo e painel de requisições HTTP. | **c** |
| 6:30–12:30 | **Funcionamento** (ver detalhamento abaixo). | **d** |
| 12:30–13:30 | **Conteúdo do curso**: SOLID.md (tabela princípio→classe), CPF mascarado no `/me` (LGPD), board Kanban, UI Blazor (painel + doação pela interface). | — |
| 13:30–14:00 | **Encerramento**: entregáveis no repositório e evoluções (outbox, BFF, Ocelot). | — |

## Detalhamento da cena de funcionamento (6:30–12:30)

1. **Login gestor** no Swagger do Campanhas.Api (30081): `POST /api/auth/login`
   com `gestor@conexaosolidaria.org` / `Gestor@123!` → copiar o token →
   colar no **jwt.io** mostrando a claim `role: GestorONG`. **(d.i)**
2. **Authorize** no Swagger com o token. Criar campanha **inválida** primeiro
   (DataFim no passado) → **400** com a mensagem da regra; criar a válida →
   **201**. **(d.ii)**
3. **Cadastro de doador**: `POST /api/auth/registrar` com CPF inválido → 400;
   com CPF válido e `consentimentoLgpd: true` → 201 (mostrar que a resposta
   NÃO devolve o CPF). Login do doador → Authorize no Swagger do Doacoes.Api (30082).
4. **Painel público** (`GET /api/publico/campanhas`): campanha com arrecadado **R$ 0**.
5. **Cena da fila**: `kubectl scale deployment doacoes-worker --replicas=0` →
   `POST /api/doacoes` (mostrar o payload) → **202 Accepted** → **RabbitMQ UI**:
   fila `doacoes.processamento` com **1 mensagem** — abrir e mostrar o payload
   do `DoacaoRecebidaEvent`. **(d.iii — parte 1)**
6. `kubectl scale deployment doacoes-worker --replicas=1` → na UI, a mensagem
   é consumida ao vivo → `GET /api/publico/campanhas` de novo: **valor
   atualizado pelo Worker**. **(d.iii — parte 2)**
7. `GET /api/doacoes/minhas`: status **Processada** (assincronismo visível).
8. **Regra de negócio**: cancelar a campanha (PUT do gestor) e tentar doar →
   **422** "Não é possível doar para campanhas encerradas ou canceladas".
9. **Grafana**: painel "Doações processadas" com o counter subindo.

## Dicas de gravação

- OBS/Teams com 1080p; fonte do terminal ≥ 16pt; zoom do browser 110–125%.
- Ensaiar uma vez completa cronometrando — a cena d é a mais longa.
- Se algo falhar ao vivo, `kubectl delete pod <nome>` resolve 90% (o
  Deployment recria); os retries dos serviços seguram a ordem de subida.
