# Justificativa da escolha dos bancos de dados

> Entregável do hackathon: documento justificando por que os bancos **X** e
> **Y** foram escolhidos. Gere o PDF com `scripts/docs-pdf.ps1`.

## Contexto

A plataforma Conexão Solidária usa **persistência poliglota**: cada
microsserviço é dono do seu armazenamento, escolhido pela natureza dos dados e
pelo padrão de acesso — um requisito arquitetural de microsserviços (evitar o
banco único compartilhado como ponto de acoplamento).

| Serviço | Banco | Dados |
|---|---|---|
| Campanhas.Api | **PostgreSQL** | usuários, campanhas, valor total arrecadado |
| Doacoes.Api / Worker | **MongoDB** | registro de cada doação (documento) |

## Por que PostgreSQL para Usuários e Campanhas?

1. **Dados fortemente relacionais e estruturados** — usuários e campanhas têm
   esquema estável, tipos precisos (`numeric(18,2)` para dinheiro — sem erro de
   ponto flutuante) e relacionamentos (campanha → gestor criador).
2. **Integridade declarativa** — o requisito "e-mail único no banco" é um
   **índice único**; violação vira erro do banco, não convenção de código.
3. **Transações ACID** — o incremento do total arrecadado e o registro de
   idempotência do Worker acontecem **na mesma transação**:
   `INSERT ... ON CONFLICT DO NOTHING` + `UPDATE ... WHERE Status = 'Ativa'`.
   É exatamente o tipo de operação em que um banco relacional brilha: atômica,
   condicional e sem read-modify-write (nenhuma corrida entre réplicas).
4. **Maturidade do ecossistema .NET** — EF Core + Npgsql com migrations
   versionadas no repositório.
5. **Custo zero e onipresença** — open source, imagem oficial leve
   (`postgres:16-alpine`), roda igual no cluster local e em qualquer nuvem.

## Por que MongoDB para Doações?

1. **Carga append-only de alta escrita** — doação é um registro imutável
   criado a cada evento; o padrão de acesso é "insere muito, lê por doador".
   Um document store otimiza exatamente esse perfil.
2. **Esquema flexível para evolução real** — o próximo passo natural é
   integrar meios de pagamento (PIX, cartão, boleto), e cada um retorna
   payloads heterogêneos. Documentos absorvem esses formatos sem migrations.
3. **Consultas simples por índice** — "minhas doações" é um índice em
   `doadorId`; não há JOINs no domínio de doações.
4. **Escala horizontal** — se a ONG crescer (campanhas virais), sharding por
   campanha/doador é caminho conhecido; o serviço de doações é o primeiro a
   receber pico de escrita.
5. **Consistência eventual aceitável e desejada** — o total consolidado vive
   no Postgres e é atualizado pelo Worker; o documento da doação carrega o
   status (`Pendente → Processada/Rejeitada`), tornando o assincronismo
   auditável.

## Por que não um banco só?

- Um único banco relacional atenderia o MVP, mas acoplaria os dois serviços ao
  mesmo esquema e ao mesmo ciclo de vida — exatamente o anti-padrão que a
  arquitetura de microsserviços tenta evitar (o edital pede a arquitetura,
  não só o funcionamento).
- A separação também isola falhas: uma indisponibilidade do Mongo não derruba
  o painel público, e vice-versa.

## Consistência eventual e reconciliação

O total no Postgres é uma **projeção** dos eventos de doação. Se divergir
(falha extrema entre Mongo e a publicação do evento), é possível **re-somar**
as doações `Processada` do Mongo e corrigir a projeção — o desenho mantém a
fonte de verdade auditável. A evolução natural é o padrão **Transactional
Outbox** no serviço de doações.

## Observação sobre o trade-off do Worker

O Worker (contexto de Doações) escreve no banco de Campanhas (incremento do
total). É uma concessão consciente ao escopo do MVP — discutida em
[ARQUITETURA.md](ARQUITETURA.md) — que mantém idempotência e atomicidade com o
mínimo de partes móveis. O `WorkerDbContext` enxerga apenas 3 colunas de uma
tabela, minimizando o acoplamento.
