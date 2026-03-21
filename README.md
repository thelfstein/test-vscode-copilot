# BarberBooking - Backend de Agendamento de Cortes de Cabelo

Sistema completo de backend para agendamento de serviços de barbearia em ASP.NET Core com autorização por roles, controle de disponibilidade de horários e validação de regras de negócio.

## Tecnologias

- **.NET 10** - Framework principal
- **EF Core 10** - ORM para acesso a dados
- **SQLite** - Banco de dados local para desenvolvimento
- **ASP.NET Core Identity** - Autenticação e gerenciamento de usuários
- **JWT Bearer** - Tokens para autenticação sem estado
- **Swagger/OpenAPI** - Documentação e teste de APIs

## Arquitetura

### Estrutura de Projetos

```
BarberBooking/
├── BarberBooking.Api/      # REST API com Controllers
├── BarberBooking.Core/      # Models, DTOs, Serviços
├── BarberBooking.Data/      # EF Core, DbContext
└── BarberBooking.sln        # Solution
```

### Camadas

- **API Layer**: Controllers que expõem endpoints HTTP
- **Service Layer**: Lógica de negócio (AuthService, AppointmentService, DbInitializer)
- **Data Layer**: Entidades EF Core, DbContext, acesso a dados
- **DTO Layer**: Transferência de dados entre API e cliente

### Entidades Principais

#### ApplicationUser
```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
    public string UserType { get; set; }     // Admin, Employee, Client
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

#### Service
```csharp
public class Service
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DurationMinutes { get; set; }    // 30 ou 60 min
    public decimal Price { get; set; }
    public string CreatedByUserId { get; set; }
    public bool Active { get; set; }
}
```

#### Appointment
```csharp
public class Appointment
{
    public int Id { get; set; }
    public string CustomerId { get; set; }      // Cliente
    public string BarberId { get; set; }        // Funcionário
    public int ServiceId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }   // Automático
    public string Status { get; set; }          // Booked, Completed, Canceled
    public bool IsPaid { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
}
```

## Regras de Negócio

### 1. Roles e Permissões

| Role | Pode | Não Pode |
|------|------|----------|
| **Admin** | Tudo | - |
| **Employee** | Ver agendamentos, marcar pagamento | Deletar, gerenciar serviços |
| **Client** | Criar agendamento, cancelar 24h antes | Ver agendamentos de outros |

### 2. Agendamento

- **Blocos fixos**: Apenas 30/60min (duração do serviço)
- **Sem overlap**: Não permite horários conflitantes
- **EndDateTime automático**: Calculado pela duração do serviço

### 3. Cancelamento

- **Clientes**: Até **24 horas antes**
- **Funcionários/Admin**: A qualquer momento
- **Status**: Muda para "Canceled", registra `CanceledAt`

### 4. Pagamento

- Campo `IsPaid` booleano
- Apenas **funcionário/admin** pode marcar como pago
- Sem integração de gateway (controle interno)

### 5. Registro

- **Clientes**: Auto-registro via `POST /api/auth/register`
- **Funcionários/Admin**: Apenas admin cria

## Como Executar

### Pré-requisitos
- .NET 10 SDK
- VS Code com Copilot (opcional)

### Passos

1. **Clone**
   ```bash
   git clone <repo-url>
   cd test-vscode-copilot
   ```

2. **Restore**
   ```bash
   dotnet restore
   ```

3. **Build**
   ```bash
   dotnet build
   ```

4. **Execute**
   ```bash
   cd BarberBooking.Api
   dotnet run
   ```

5. **Acesse Swagger**
   ```
   https://localhost:5001/swagger
   ```

## Dados Seed Automáticos

### Admin
- Email: `admin@barberbooking.com`
- Senha: `Admin@123456`

### Barbeiros
1. **João**: `barber@barberbooking.com` / `Barber@123456`
2. **Carlos**: `barber2@barberbooking.com` / `Barber@123456`

### Serviços
1. Corte Simples - 30min - R$ 50
2. Corte + Barba - 60min - R$ 80
3. Barba Técnica - 30min - R$ 40
4. Corte Degradê - 45min - R$ 65

## Endpoints da API

### Autenticação

#### POST /api/auth/login
```json
{
  "email": "admin@barberbooking.com",
  "password": "Admin@123456"
}
```

**Response**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": "uuid",
  "email": "admin@barberbooking.com",
  "fullName": "Admin Barbearia",
  "userType": "Admin",
  "expiresAt": "2026-03-21T01:58:00Z"
}
```

**Usar em requisições**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

#### POST /api/auth/register
```json
{
  "email": "cliente@example.com",
  "password": "Senha@123",
  "fullName": "João Cliente",
  "phoneNumber": "(11) 98765-4321",
  "userType": "Client"
}
```

---

### Serviços

| Método | Endpoint | Auth | Permissão |
|--------|----------|------|-----------|
| GET | `/api/services` | Não | Público |
| GET | `/api/services/{id}` | Não | Público |
| POST | `/api/services` | Sim | Admin |
| PUT | `/api/services/{id}` | Sim | Admin |
| DELETE | `/api/services/{id}` | Sim | Admin |

**POST /api/services (Admin)**
```json
{
  "name": "Novo Serviço",
  "durationMinutes": 45,
  "price": 70.00
}
```

---

### Agendamentos

| Método | Endpoint | Permissão |
|--------|----------|-----------|
| GET | `/api/appointments/{id}` | Autenticado |
| GET | `/api/appointments/my-appointments` | Client |
| GET | `/api/appointments/barber/{id}` | Employee/Admin |
| GET | `/api/appointments/disponibilidade/{barberId}` | Público |
| POST | `/api/appointments` | Client |
| PUT | `/api/appointments/{id}` | Client (24h) / Employee/Admin |
| POST | `/api/appointments/{id}/cancel` | Client (24h) / Employee/Admin |
| POST | `/api/appointments/{id}/pay` | Employee/Admin |

**POST /api/appointments (Cliente)**
```json
{
  "barberId": "uuid-barber",
  "serviceId": 1,
  "startDateTime": "2026-03-21T10:00:00Z"
}
```

**PUT /api/appointments/{id} (Atualizar)**
```json
{
  "barberId": "novo-uuid",
  "serviceId": 2,
  "startDateTime": "2026-03-21T14:00:00Z"
}
```

**POST /api/appointments/{id}/cancel (Cancelar)**
```
Status: 204 No Content
```

**POST /api/appointments/{id}/pay (Marcar Pago)**
```json
{
  "isPaid": true
}
```

---

### Usuários

| Método | Endpoint | Permissão |
|--------|----------|-----------|
| GET | `/api/users/{id}` | Self / Admin |
| GET | `/api/users` | Admin |
| PUT | `/api/users/{id}` | Self / Admin |
| POST | `/api/users` | Admin |
| DELETE | `/api/users/{id}` | Admin |

**POST /api/users (Criar por Admin)**
```json
{
  "email": "novo-barber@barberbooking.com",
  "password": "Barber@123456",
  "fullName": "Novo Barbeiro",
  "userType": "Employee"
}
```

---

## Debug e Teste

### Via Swagger (Recomendado)

1. Inicie: `dotnet run`
2. Abra: `https://localhost:5001/swagger`
3. Clique "Authorize"
4. Cole o token JWT completo (com prefixo "Bearer ")
5. Teste os endpoints

### Via cURL

```bash
# Login
TOKEN=$(curl -s -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d '{"email":"admin@barberbooking.com","password":"Admin@123456"}' \
  | jq -r '.token')

echo "Token: $TOKEN"

# Usar token
curl -X GET https://localhost:5001/api/services \
  -H "Authorization: Bearer $TOKEN"
```

### Via VS Code REST Client

Crie `requests.http`:

```http
@baseUrl = https://localhost:5001

### Login
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "admin@barberbooking.com",
  "password": "Admin@123456"
}

###
@token = TOKEN_RECEBIDO_AQUI

### Listar Serviços
GET {{baseUrl}}/api/services
Authorization: Bearer {{token}}

### Criar Agendamento
POST {{baseUrl}}/api/appointments
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "barberId": "barber-uuid",
  "serviceId": 1,
  "startDateTime": "2026-03-21T10:00:00Z"
}
```

---

## Testes

### Executar Testes Unitários
```bash
dotnet test
```

### Executar Testes de um Projeto
```bash
dotnet test BarberBooking.Api.Tests
```

---

## Estrutura JWT

```
Header: { "alg": "HS256", "typ": "JWT" }

Payload: {
  "nameid": "user-uuid",
  "email": "user@example.com",
  "UserType": "Admin",
  "FullName": "Nome",
  "exp": 1742564280,
  "iss": "BarberBooking",
  "aud": "BarberBookingClient"
}
```

---

## Variáveis de Ambiente

```bash
# .env (não commitado)
ASPNETCORE_ENVIRONMENT=Development
JWT_SECRET=supersecretkeysupersecretkey123456789!@#$%^&*()
DATABASE_URL=Data Source=barber_booking.db
```

---

## SQLite

### Inspecionar Banco
```bash
sqlite3 barber_booking.db
.tables
SELECT * FROM AspNetUsers;
SELECT * FROM Services;
SELECT * FROM Appointments;
```

---

## Status

- ✅ Passo 1: Repositório
- ✅ Passo 2: Banco de dados + Entidades
- ✅ Passo 3: Autenticação + JWT
- ✅ Passo 4: Controllers CRUD
- ✅ Passo 5: Regras de negócio
- ✅ Passo 6: Seed de dados
- ✅ Passo 7: Swagger + Autorização
- ⏳ Passo 8: Testes
- ⏳ Passo 9: GitHub Actions

---

## Licença

MIT
