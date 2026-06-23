# BankPriorityQueueApi 🏦

API REST que implementa uma **fila de prioridade** de atendimento bancário usando
**Heap (min-heap binário)** como estrutura de dados. O atendimento **não segue
ordem de chegada pura**: segue critérios de **urgência** e, em caso de empate,
**ordem de chegada**.

> Trabalho de Estrutura de Dados — conecta teoria (Heap / fila de prioridade) com
> prática (API REST + banco + Docker).

**Autores:** Sávio Cardoso · Estevão · Matheus Ribeiro

---

## 🎯 Regra de prioridade (explícita e automatizada)

A posição de cada atendimento na fila é decidida automaticamente por:

1. **Menor `Urgencia` é atendida primeiro** — `1 = máxima` … `5 = mínima`.
2. **Empate de urgência → quem chegou primeiro** (menor `DataChegada`).
3. **Desempate final absoluto → `Id`** (garante ordenação 100% determinística).

Implementação: [`PriorityComparer`](src/Application/Services/PriorityComparer.cs).
No **PUT**, se a urgência muda, a posição na fila é **recalculada** automaticamente.

---

## 🧠 Por que Heap? (justificativa da estrutura de dados)

Uma fila de prioridade precisa devolver **sempre o mais prioritário** sem reordenar
toda a coleção a cada operação. O **min-heap binário** entrega exatamente isso:

| Operação | Heap | Lista ordenada | Busca linear |
|----------|------|----------------|--------------|
| Ver próximo (`Peek`) | **O(1)** | O(1) | O(n) |
| Inserir (`Enqueue`) | **O(log n)** | O(n) | O(1) |
| Remover próximo (`Dequeue`) | **O(log n)** | O(n) | O(n) |

O heap mantém a invariante: **o pai é sempre mais prioritário que os filhos**, então o
topo (índice 0) é o próximo a ser atendido. Implementação comentada em
[`PriorityHeap.cs`](src/Domain/DataStructures/PriorityHeap.cs).

> O **banco de dados é a fonte da verdade** (persistência); o **Heap em memória** é a
> ordenação rápida da fila. No startup, o Heap é reconstruído a partir dos registros
> que estão `Aguardando`.

---

## 🏛️ Arquitetura (Clean Architecture / DDD + SOLID)

```
src/
├── Domain/          # núcleo: entidades, enums, value objects e o Heap. Sem dependências.
├── Application/     # casos de uso, DTOs, interfaces e a regra de prioridade.
├── Infrastructure/  # EF Core + PostgreSQL (implementa as interfaces da Application).
└── Api/             # controllers, Swagger e injeção de dependência (Program.cs).
tests/               # testes xUnit do Heap e da regra de prioridade.
```

Regra de dependência: `Api → Application → Domain` e `Infrastructure → Application/Domain`.
O **Domain não depende de nada**. As interfaces ficam na Application e são implementadas
na Infrastructure (**Inversão de Dependência** — o "D" de SOLID).

**SOLID aplicado:**
- **S** (Single Responsibility): controller fino, serviço orquestra, repositório persiste.
- **O/L**: `PriorityHeap<T>` é genérico; a regra entra por `IComparer<T>`.
- **I**: interfaces pequenas e específicas (`IAtendimentoRepository`, `IPriorityQueueService`).
- **D**: tudo injetado por interface.

---

## 🐳 Como rodar (Docker Compose)

Pré-requisito: **Docker** instalado.

```bash
docker compose up --build
```

- API: http://localhost:8080
- **Swagger (documentação interativa):** http://localhost:8080/swagger
- PostgreSQL: `localhost:5432` (db `bankqueue`, user/senha `postgres`/`postgres`)

Para parar: `Ctrl+C` e depois `docker compose down` (ou `down -v` para apagar os dados).

---

## 🔌 Endpoints (`/atendimentos-bancarios`)

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/atendimentos-bancarios` | Cadastra item e calcula a prioridade. |
| `GET` | `/atendimentos-bancarios/{id}` | Busca um item específico. |
| `GET` | `/atendimentos-bancarios?page=1&size=10` | Lista ativos paginados (ordem de prioridade). |
| `GET` | `/atendimentos-bancarios/buscar?cpf=&descricao=` | Busca por CPF ou descrição. |
| `PUT` | `/atendimentos-bancarios/{id}` | Atualiza dados e **recalcula a prioridade**. |
| `DELETE` | `/atendimentos-bancarios/{id}` | **Exclusão lógica** (status → `Cancelado`). |
| `GET` | `/atendimentos-bancarios/proximo` | (Bônus) chama o próximo da fila (`Dequeue`). |

### Exemplos (curl)

```bash
# Cadastrar (urgência 1 = máxima)
curl -X POST http://localhost:8080/atendimentos-bancarios \
  -H "Content-Type: application/json" \
  -d '{"nomeCliente":"Maria","cpf":"529.982.247-25","tipoServico":"Empréstimo","descricao":"renegociação","urgencia":1}'

# Listar ativos (página 1, 10 por página)
curl "http://localhost:8080/atendimentos-bancarios?page=1&size=10"

# Buscar por CPF
curl "http://localhost:8080/atendimentos-bancarios/buscar?cpf=52998224725"

# Atender o próximo da fila
curl "http://localhost:8080/atendimentos-bancarios/proximo"

# Exclusão lógica
curl -X DELETE http://localhost:8080/atendimentos-bancarios/{id}
```

---

## 🗑️ Exclusão lógica

O `DELETE` **não remove** a linha do banco: apenas muda `Status` para `Cancelado`
(ver [`Atendimento.Cancelar()`](src/Domain/Entities/Atendimento.cs)). O registro
continua para histórico, mas some das listagens de ativos e da fila.

---

## ✅ Testes

```bash
dotnet test
```

Cobrem a mecânica do Heap ([`PriorityHeapTests`](tests/PriorityHeapTests.cs)) e a
regra de prioridade + empate ([`PriorityComparerTests`](tests/PriorityComparerTests.cs)).

---

## 🛠️ Stack

.NET 8 · ASP.NET Core Web API · Entity Framework Core 8 (Npgsql) · PostgreSQL 16 ·
Swagger/OpenAPI · xUnit · Docker / Docker Compose.
