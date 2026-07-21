# LGPD — Privacidade de dados no projeto

Como o MVP aplica os princípios da **Lei 13.709/2018 (LGPD)** vistos no módulo
de Privacidade de Dados do curso.

## Princípios aplicados

| Princípio (LGPD, art. 6º) | Aplicação concreta |
|---|---|
| **Minimização / necessidade** | O cadastro de doador coleta apenas 4 dados: nome, e-mail, CPF e senha — o mínimo para gerir doações. Nada de telefone, endereço ou data de nascimento. |
| **Finalidade** | O texto de consentimento declara a finalidade: "exclusivamente para gestão das minhas doações". |
| **Transparência** | Painel público expõe **somente dados de campanhas** (título, meta, arrecadado) — nunca dados de pessoas. |
| **Segurança** | Senha com **hash BCrypt** (nunca em claro); JWT assinado; secrets fora do código-fonte (env/Secrets K8s). |
| **Prevenção** | Erro de login genérico ("Credenciais inválidas") — não revela se um e-mail está cadastrado (evita enumeração de titulares). |
| **Responsabilização (accountability)** | O consentimento é persistido com **timestamp** (`ConsentimentoLgpd` + `DataConsentimentoUtc` na tabela `usuarios`). |

## Consentimento

- O cadastro **exige** o aceite explícito (checkbox no Blazor + validação no
  domínio: `Usuario.CriarDoador` lança exceção sem consentimento).
- Base legal do tratamento: **consentimento do titular** (art. 7º, I).

## CPF — dado pessoal sensível ao contexto

- Validado no cadastro (dígitos verificadores — value object `Cpf`).
- **Nunca** aparece em endpoints públicos nem em respostas de cadastro.
- No `GET /api/auth/me` (o próprio titular), retorna **mascarado**:
  `529.***.***-25` (`Cpf.Mascarado()`).
- Armazenado normalizado (só dígitos) no Postgres, acessível apenas pelo
  serviço de Campanhas.

## Logs e observabilidade

- Logs estruturados registram **IDs** (GUIDs de usuário/doação), não CPF,
  e-mail ou nome.
- Métricas Prometheus são agregadas (contadores) — sem dados pessoais.

## Token no navegador (Blazor)

- JWT guardado em **sessionStorage** (morre ao fechar a aba) em vez de
  localStorage — menor janela de exposição.
- Trade-off documentado: em produção, o padrão recomendado é um **BFF com
  cookie HttpOnly + SameSite**, eliminando o acesso do JavaScript ao token.

## Direitos do titular (arts. 18+) — estado atual e evolução

| Direito | Estado no MVP |
|---|---|
| Acesso aos dados | `GET /api/auth/me` (com CPF mascarado) |
| Correção | Evolução: endpoint de atualização cadastral |
| Eliminação/anonimização | Evolução: exclusão da conta com **anonimização** das doações (mantém a integridade contábil das campanhas trocando `doadorId` por pseudônimo) |
| Revogação do consentimento | Evolução: revogar = inativar a conta para novas doações |

## Transporte e produção

No cluster local o tráfego é HTTP; em produção, **TLS obrigatório** de ponta a
ponta (Ingress com certificado) e Secrets via cofre (Key Vault / Sealed
Secrets), não versionados.
