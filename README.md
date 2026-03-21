# BarberBooking - Backend de Agendamento de Cortes de Cabelo

Backend em ASP.NET Core para sistema de agendamento de barbearia, com CRUD, controle de acesso e APIs REST.

## Tecnologias
- .NET 10
- EF Core + LocalDB (SQL Server)
- ASP.NET Core Identity + JWT
- Roles: Admin, Funcionário, Cliente

## Como executar
1. Clone o repositório.
2. Execute `dotnet restore`.
3. Configure connection string em `appsettings.Development.json`.
4. Execute `dotnet run` (porta padrão 5000/5001).
5. Acesse `/swagger` para APIs.

## Funcionalidades
- CRUD de serviços, usuários e agendamentos.
- Controle de disponibilidade (blocos 30/60min).
- Cancelamento até 24h antes.
- Pagamento interno (IsPaid).

## Desenvolvimento
- Use VS Code com extensão GitHub Copilot.
- Testes: `dotnet test`.
- Produção: Configure SQL Server.

## Arquitetura
- Clean Architecture: Api, Core, Data.
- Seed de admin e roles na inicialização.
