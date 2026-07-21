# Metodologia ágil aplicada ao trabalho

Como o desenvolvimento do MVP aplicou o conteúdo do módulo de Metodologias
Ágeis (Scrum, XP, Lean, Kanban).

## Kanban — gestão do fluxo

- Board no **GitHub Projects** com colunas `Backlog → Em andamento → Concluído`
  e **WIP limit 2** (terminar antes de começar — fluxo, não acúmulo).
- Cada fase do plano virou um cartão/issue (F0 esqueleto, F1 infra local,
  F2 Campanhas, F3 mensageria, F5 containers, F6 K8s, F7 observabilidade,
  F8 CI, F9 Blazor, F10 docs), cada uma com **critério de pronto (DoD)**
  verificável — ex.: F3 pronta quando "doação via Swagger atualiza o painel e a
  mensagem é visível na fila com o worker parado".
- *Sugestão para o relatório: tirar screenshot do board e anexar.*

## XP — práticas de engenharia

- **Testes primeiro no domínio**: as regras do edital (data de término no
  passado, meta > 0, CPF válido, consentimento LGPD, idempotência do worker)
  têm testes xUnit — 35 testes no total.
- **Integração contínua**: todo push na `main` compila, testa e gera as
  imagens Docker (GitHub Actions). Testes rodando **na esteira** (bônus do
  edital).
- **Design simples + refatoração**: MVP sem over-engineering (3 projetos por
  serviço no máximo), com os pontos de evolução documentados
  (outbox, StatefulSets, BFF) em vez de implementados prematuramente.

## Lean — eliminar desperdício

- **MVP enxuto**: requisitos obrigatórios antes de qualquer bônus; o bônus
  Ocelot foi **conscientemente cortado** do escopo (decisão registrada) por não
  impactar a nota.
- **Decidir o mais tarde possível**: broker, bancos e stack de observabilidade
  foram decididos com base nos critérios de aceite (ex.: RabbitMQ pela
  Management UI exigida no vídeo).

## Scrum — cadência

- Trabalho organizado em incrementos curtos (as fases), cada um terminando em
  software funcionando e commitado — o equivalente a sprints com review
  (verificação do critério de pronto) e retro embutidas.
