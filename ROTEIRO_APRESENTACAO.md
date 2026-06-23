# 🎤 Roteiro de Apresentação — BankPriorityQueueApi

> Guia para apresentar em sala: o que falar, em que ordem, e ONDE está cada coisa
> no código. Tempo sugerido: **10–12 min**. Autores: Sávio, Estevão, Matheus.

---

## 0. Antes de começar (deixe rodando)

```bash
docker compose up --build
```
Abra o **Swagger**: http://localhost:8080/swagger — é a tela da demonstração.

---

## 1. Abertura — o problema (≈1 min)

> "Em um banco, atender só por ordem de chegada é injusto: um cliente com urgência
> alta (ex.: golpe/bloqueio) não pode esperar atrás de 20 pessoas. Nosso sistema
> ordena a fila por **urgência** e, no empate, por **ordem de chegada** — usando a
> estrutura de dados ideal para isso: o **Heap**."

Conecte com os exemplos do enunciado: pronto-socorro, entregas, pagamentos — todos
são **filas de prioridade**.

---

## 2. A estrutura de dados: Heap (≈3 min) — CORAÇÃO DO TRABALHO

**Arquivo:** `src/Domain/DataStructures/PriorityHeap.cs`

Pontos a falar:
- É um **min-heap binário** sobre um array (`List<T>`).
- Invariante: **o pai é sempre mais prioritário que os filhos** → o topo (índice 0)
  é sempre o próximo a atender.
- Índices no array: filhos de `i` em `2i+1` e `2i+2`; pai de `i` em `(i-1)/2`.
- Operações e custos:
  - `Peek` → **O(1)** (só olha o topo).
  - `Enqueue` → **O(log n)** (`SiftUp`: o novo elemento "sobe" trocando com o pai).
  - `Dequeue` → **O(log n)** (`SiftDown`: o topo sai, o último sobe e "desce").
- **Por que não usar lista ordenada?** Inserir em lista ordenada é **O(n)**; no heap é
  **O(log n)**. Mostre a tabela do README.

> Mostre na tela os métodos `SiftUp` e `SiftDown` — são o segredo do heap.

---

## 3. A regra de negócio (≈2 min)

**Arquivo:** `src/Application/Services/PriorityComparer.cs`

> "A regra é explícita e automática. O comparador define a ordem:"
1. Menor `Urgencia` vence (1 = máxima).
2. Empate → menor `DataChegada` (chegou primeiro).
3. Desempate final → `Id` (determinístico, sem ambiguidade).

> "Esse comparador é injetado no Heap. Trocar a regra = trocar o comparador, sem
> mexer no Heap (princípio Aberto/Fechado do SOLID)."

**Entidade:** `src/Domain/Entities/Atendimento.cs` — mostre `Criar()` (valida urgência
1–5 e CPF) e `Cancelar()` (exclusão lógica).

---

## 4. A arquitetura (≈2 min)

**Mostre a árvore de pastas `src/`.** Fale a regra de dependência:

> "Clean Architecture: as dependências apontam para o **Domain**, que não conhece
> banco nem web. A **Application** define interfaces; a **Infrastructure** (EF Core +
> Postgres) as implementa. A **Api** só recebe HTTP e chama o caso de uso."

- `Domain` → entidade, enum de status, value object `Cpf`, e o `PriorityHeap`.
- `Application` → `AtendimentoService` (orquestra repositório + fila), DTOs, interfaces.
- `Infrastructure` → `AtendimentoRepository` (SQL via EF Core), `AppDbContext`.
- `Api` → `AtendimentosBancariosController`, Swagger, DI em `Program.cs`.

> "Tudo é injetado por **interface** (`IAtendimentoRepository`, `IPriorityQueueService`)
> — isso é o **D** do SOLID e deixa o código testável."

Cite o `Program.cs`: a fila é **Singleton** (uma só para o app); o repositório é
**Scoped** (por requisição). No startup, o Heap é **reconstruído** do banco.

---

## 5. Demonstração ao vivo (≈3 min) — no Swagger

Roteiro de cliques que comprova a prioridade:

1. **POST** 3 atendimentos, de propósito fora de ordem:
   - João — urgência **5** (normal)
   - Ana — urgência **1** (máxima)
   - Beto — urgência **3** (média)
2. **GET (listar)** → mostre que a ordem retornada é **Ana (1) → Beto (3) → João (5)**,
   não a ordem de cadastro. ✅ prova a fila de prioridade.
3. **PUT** no João mudando urgência para **1** → liste de novo: João sobe na fila
   (recalculo automático). Se João chegou depois da Ana, Ana continua na frente
   (empate resolvido por chegada). ✅ prova o desempate.
4. **GET /proximo** → retorna a Ana (topo do Heap) e muda o status dela para
   `EmAtendimento`. ✅ prova o `Dequeue`.
5. **DELETE** em um id → **204**. Depois **GET por id**: o registro ainda existe, mas
   com status `Cancelado`, e some da listagem de ativos. ✅ prova a **exclusão lógica**.

---

## 6. Fechamento (≈1 min)

> "Resumindo: usamos **Heap** porque é a estrutura ótima para fila de prioridade
> (O(log n) por operação). Aplicamos **Clean Architecture/DDD e SOLID** para separar
> domínio, casos de uso e infraestrutura. Tudo roda com **um comando no Docker**, com
> **Swagger** para documentação e **exclusão lógica** preservando o histórico."

---

## 📌 Mapa rápido "pergunta → arquivo"

| Se perguntarem sobre... | Mostre |
|--------------------------|--------|
| O Heap / complexidade | `src/Domain/DataStructures/PriorityHeap.cs` |
| Regra de prioridade / empate | `src/Application/Services/PriorityComparer.cs` |
| Entidade / exclusão lógica | `src/Domain/Entities/Atendimento.cs` |
| Validação de CPF | `src/Domain/ValueObjects/Cpf.cs` |
| Orquestração dos casos de uso | `src/Application/Services/AtendimentoService.cs` |
| Banco / SQL / EF Core | `src/Infrastructure/Repositories/AtendimentoRepository.cs` |
| Endpoints HTTP | `src/Api/Controllers/AtendimentosBancariosController.cs` |
| Injeção de dependência / startup | `src/Api/Program.cs` |
| Docker | `Dockerfile`, `docker-compose.yml` |
| Testes | `tests/PriorityHeapTests.cs`, `tests/PriorityComparerTests.cs` |

---

## ❓ Possíveis perguntas do professor

- **"Por que o Heap e não uma fila normal (`Queue`)?"** → `Queue` é FIFO puro, ignora
  prioridade. O Heap mantém o mais prioritário no topo em O(log n).
- **"E se dois chegarem no mesmo instante?"** → desempate final pelo `Id` no
  `PriorityComparer` → ordem sempre determinística.
- **"Onde está o SOLID?"** → seção 4 do README; interfaces + injeção de dependência.
- **"O Heap perde os dados se a API reiniciar?"** → não: o banco é a fonte da verdade;
  no startup o Heap é reconstruído (`Program.cs` → `InicializarBancoEFilaAsync`).
- **"Por que exclusão lógica?"** → auditoria/histórico; nunca se perde o registro.
