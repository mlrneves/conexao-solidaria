# SOLID aplicado no projeto

Mapeamento princípio → código real (procure também os comentários `// SOLID:`
nos arquivos citados).

## S — Single Responsibility Principle

| Classe | Arquivo | Responsabilidade única |
|---|---|---|
| `TokenService` | `src/Services/Campanhas/Campanhas.Api/Services/TokenService.cs` | Só emite JWT. Login, senha e persistência ficam no `AuthService`/repositórios. |
| `Cpf` (value object) | `src/Services/Campanhas/Campanhas.Domain/ValueObjects/Cpf.cs` | Toda a regra de CPF (normalização, dígitos verificadores, máscara LGPD) em um único lugar. |
| `RabbitMqEventBus` | `src/BuildingBlocks/BuildingBlocks.Messaging/RabbitMqEventBus.cs` | Só publica eventos. O consumo é do `DoacaoRecebidaConsumer`. |
| `ProcessadorDeDoacao` | `src/Services/Doacoes/Doacoes.Worker/Processing/ProcessadorDeDoacao.cs` | Orquestra o processamento de UM evento; não conhece RabbitMQ (consumer) nem SQL (repositório). |

## O — Open/Closed Principle

- **Novas regras de campanha** entram em `Campanha.Criar()`/`ValidarDadosBasicos()`
  (`Campanhas.Domain/Entities/Campanha.cs`) sem alterar controllers ou serviços
  — o domínio é o ponto de extensão.
- **Trocar RabbitMQ por Kafka** = escrever `KafkaEventBus : IEventBus` e trocar
  1 linha na raiz de composição. Nenhum consumidor de `IEventBus` muda
  (`DoacaoService` continua intacto).

## L — Liskov Substitution Principle

- Nos testes, `IDoacaoRepository`, `ICampanhasClient` e `IEventBus` são
  substituídos por dublês (NSubstitute) e o `DoacaoService` funciona idêntico —
  qualquer implementação que honre o contrato é substituível
  (`tests/Doacoes.Tests/DoacaoServiceTests.cs`).
- `CampanhaRepository` (EF/Postgres) pode ser trocado por um fake em memória
  sem quebrar o `CampanhaService`.

## I — Interface Segregation Principle

- `ICampanhasClient` (`Doacoes.Api/Infrastructure/ICampanhasClient.cs`) tem
  **um único método** — exatamente o que o fluxo de doação precisa do outro
  serviço, nada além.
- `IDoacaoRepository` expõe só gravar e listar por doador; atualizar status é
  contrato separado (`IStatusDoacaoAtualizador`), usado só pelo Worker.
- **ISP aplicado a dados**: o `WorkerDbContext`
  (`Doacoes.Worker/Persistence/WorkerDbContext.cs`) mapeia apenas
  `campanhas{Id, Status, ValorTotalArrecadado}` — o Worker não enxerga título,
  descrição, datas.

## D — Dependency Inversion Principle

- O domínio define os contratos (`ICampanhaRepository`, `IUsuarioRepository` em
  `Campanhas.Domain/Repositories/`); a infraestrutura implementa. A seta de
  dependência aponta para dentro.
- `DoacaoService` recebe **só abstrações** no construtor
  (`IDoacaoRepository`, `ICampanhasClient`, `IEventBus`).
- As implementações concretas aparecem em **um único lugar** por serviço: a
  raiz de composição (`Program.cs`), via injeção de dependência.
