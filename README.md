# Harmonia API - Gestão de Estoque para Acessórios de Pets

API backend para controle de estoque e gerenciamento de catálogo de produtos para **acessórios para pets** (coleiras, brinquedos, camas, etc.).

## 🚀 Tecnologias

- **ASP.NET Core 8** - Web API
- **PostgreSQL** - Banco de dados
- **JWT** - Autenticação
- **FluentValidation** - Validação
- **Docker** - Container
- **xUnit** - Testes unitários
- **Clean Architecture** - Organização do código

## 📋 Funcionalidades

| Funcionalidade | Descrição |
|----------------|-----------|
| **Cadastro de Usuários** | Registro de Administradores e Vendedores |
| **Login** | Autenticação via JWT |
| **Gerenciamento de Produtos** | CRUD completo (Admin) |
| **Controle de Estoque** | Entrada de estoque com nota fiscal |
| **Emissão de Pedidos** | Criação de pedidos e validação de estoque |

## 🐳 Executando com Docker

```bash
# Clonar o repositório
git clone https://github.com/felipeshurrab/Harmonia
cd Harmonia

# Subir os containers
docker-compose up -d --build

# Aguardar os containers iniciarem (~30s)
# API estará disponível em: http://localhost:5000
# Swagger UI: http://localhost:5000 (raiz)
# PgAdmin: http://localhost:8080
```

### Aplicar Migrations

As migrations são aplicadas automaticamente na primeira execução. Se precisar aplicar manualmente:

```bash
# Via Visual Studio (Package Manager Console)
Update-Database

# Ou via CLI
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

## 🧪 Executando Testes

```bash
# Todos os testes
dotnet test

# Com detalhes
dotnet test --verbosity normal
```

### Cobertura de Testes

| Arquivo | Cenários Cobertos |
|---------|-------------------|
| `AuthServiceTests.cs` | Registro OK, email duplicado, login válido/inválido |
| `ProductServiceTests.cs` | CRUD completo, produto não encontrado |
| `StockServiceTests.cs` | Entrada de estoque, produto inexistente |
| `OrderServiceTests.cs` | Pedido OK (CPF/CNPJ), estoque insuficiente, múltiplos itens |
| `CreateOrderRequestValidatorTests.cs` | DocumentType obrigatório/inválido, CPF 11 dígitos, CNPJ 14 dígitos, apenas números |

---

## 📖 Fluxo Completo de Teste (Passo a Passo)

> **Pré-requisito:** Execute `docker-compose up -d --build` e aguarde os containers iniciarem.

### Passo 1: Cadastrar Administrador

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name": "Admin", "email": "admin@harmonia.com", "password": "123456", "role": "Administrator"}'
```

**Resposta esperada (201):**
```json
{"id": "...", "name": "Admin", "email": "admin@harmonia.com", "role": "Administrator"}
```

---

### Passo 2: Cadastrar Vendedor

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name": "João Vendedor", "email": "joao@harmonia.com", "password": "123456", "role": "Seller"}'
```

**Resposta esperada (201):**
```json
{"id": "...", "name": "João Vendedor", "email": "joao@harmonia.com", "role": "Seller"}
```

---

### Passo 3: Login como Administrador

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@harmonia.com", "password": "123456"}'
```

**Resposta esperada (200):**
```json
{"token": "eyJhbGc...", "expiresAt": "2026-02-02T...", "user": {"id": "...", "name": "Admin", "role": "Administrator"}}
```

> ⚠️ **Guarde o valor de `token`** - será usado como `{TOKEN_ADMIN}` nos próximos passos.

---

### Passo 4: Cadastrar Produto (requer Admin)

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -d '{"name": "Coleira Premium para Cães", "description": "Coleira ajustável para cães de médio porte", "price": 89.90}'
```

**Resposta esperada (201):**
```json
{"id": "abc123...", "name": "Coleira Premium para Cães", "description": "...", "price": 89.90, "stockQuantity": 0}
```

> ⚠️ **Guarde o valor de `id`** - será usado como `{PRODUCT_ID}` nos próximos passos.

---

### Passo 5: Adicionar Estoque (requer Admin)

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/stock \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN_ADMIN}" \
  -d '{"productId": "{PRODUCT_ID}", "quantity": 50, "invoiceNumber": "NF-2026-001"}'
```

**Resposta esperada (201):**
```json
{"id": "...", "productId": "{PRODUCT_ID}", "quantity": 50, "invoiceNumber": "NF-2026-001", "entryDate": "..."}
```

> ✅ O produto agora tem `stockQuantity: 50`.

---

### Passo 6: Login como Vendedor

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "joao@harmonia.com", "password": "123456"}'
```

**Resposta esperada (200):**
```json
{"token": "eyJhbGc...", "expiresAt": "...", "user": {"id": "...", "name": "João Vendedor", "role": "Seller"}}
```

> ⚠️ **Guarde o valor de `token`** - será usado como `{TOKEN_SELLER}`.

---

### Passo 7: Criar Pedido (requer Seller)

**Requisição:**
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN_SELLER}" \
  -d '{"documentType": "CPF", "customerDocument": "12345678900", "items": [{"productId": "{PRODUCT_ID}", "quantity": 2}]}'
```

**Resposta esperada (201):**
```json
{
  "id": "...",
  "documentType": "CPF",
  "customerDocument": "12345678900",
  "sellerName": "João Vendedor",
  "totalAmount": 179.80,
  "items": [{"productId": "...", "productName": "Coleira Premium...", "quantity": 2, "unitPrice": 89.90}]
}
```

> ✅ O estoque foi automaticamente reduzido de 50 para 48.

---

### Passo 8: Testar Validação de Estoque Insuficiente

**Requisição (tentando pedir mais do que disponível):**
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN_SELLER}" \
  -d '{"documentType": "CNPJ", "customerDocument": "12345678000190", "items": [{"productId": "{PRODUCT_ID}", "quantity": 1000}]}'
```

**Resposta esperada (400):**
```json
{
  "errorCode": "INSUFFICIENT_STOCK",
  "message": "Estoque insuficiente para o produto 'Coleira Premium...'. Solicitado: 1000, Disponível: 48",
  "details": {"productId": "...", "requestedQuantity": 1000, "availableQuantity": 48}
}
```

> ✅ O pedido foi **rejeitado** e o estoque permanece inalterado.

---

## ⚠️ Tratamento de Erros

A API retorna erros padronizados com códigos para facilitar o tratamento no frontend:

| Código | HTTP | Descrição |
|--------|------|-----------|
| `VALIDATION_ERROR` | 400 | Campos inválidos ou faltando |
| `NOT_FOUND` | 404 | Recurso não encontrado |
| `INSUFFICIENT_STOCK` | 400 | Estoque insuficiente para o pedido |
| `UNAUTHORIZED` | 401 | Credenciais inválidas |
| `INTERNAL_ERROR` | 500 | Erro interno do servidor |

## 🔐 Autorização

| Endpoint | Admin | Seller | Sem Auth |
|----------|:-----:|:------:|:--------:|
| POST /api/auth/register | ✅ | ✅ | ✅ |
| POST /api/auth/login | ✅ | ✅ | ✅ |
| GET /api/products | ✅ | ✅ | ✅ |
| POST/PUT/DELETE /api/products | ✅ | ❌ | ❌ |
| POST /api/stock | ✅ | ❌ | ❌ |
| POST /api/orders | ❌ | ✅ | ❌ |
| GET /api/orders | ✅ (todos) | ✅ (seus) | ❌ |

## 🏥 Health Checks

A API expõe um endpoint de health check para monitoramento:

```bash
curl http://localhost:5000/health
# Healthy
```

## 📁 Estrutura do Projeto

```
src/
├── Api/                        # Camada de Apresentação
│   ├── Controllers/            # Endpoints REST
│   ├── Middleware/             # Tratamento global de erros
│   └── Program.cs              # Configuração e DI
├── Application/                # Camada de Aplicação
│   ├── Dtos/                   # Data Transfer Objects
│   ├── Exceptions/             # Exceções customizadas
│   ├── Service/                # Lógica de negócio
│   └── Validators/             # FluentValidation rules
├── Domain/                     # Camada de Domínio
│   ├── Entities/               # Entidades do domínio
│   ├── Enums/                  # Enumerações
│   └── Interfaces/             # Contratos dos repositórios
└── Infrastructure/             # Camada de Infraestrutura
    ├── Configurations/         # EF Core configurations
    ├── DbContexts/             # DbContext
    ├── Migrations/             # Migrations do EF Core
    └── Repositories/           # Implementações dos repositórios

tests/
├── Api.Tests/                  # Testes de Controllers
└── Application.Tests/          # Testes de Services e Validators
```
