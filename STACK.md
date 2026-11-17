# Stack Tecnológica e Segurança

## Backend e Arquitetura
- **Linguagem:** C# 14 / **.NET 10** (STS — Standard-Term Support, lançado em Novembro de 2025)
- **Framework Web:** ASP.NET Core Web API (.NET 10)
- **Arquitetura:** Domain-Driven Design (DDD) com separação estrita por Domain, Application, Infrastructure e Presentation.

## Segurança e Criptografia
- **Autenticação:** JWT (JSON Web Token) com expiração controlada e refresh tokens seguros.
- **Proteção de Senhas:** ASP.NET Core PasswordHasher (Algoritmo PBKDF2 com Salt aleatório e hashing iterativo).
- **Validação de Entrada:** FluentValidation 12.x (para barrar dados maliciosos ou inválidos antes de atingirem o domínio).
- **Proteção Web:** HTTPS obrigatório, CORS restrito e cabeçalhos de segurança (HSTS, X-Content-Type-Options).

## Banco de Dados e Testes
- **Banco de Dados:** SQLite (desenvolvimento) / PostgreSQL (produção/deploy).
- **ORM:** Entity Framework Core **10.0** (com proteção contra SQL Injection via consultas parametrizadas).
- **Testes:** xUnit 2.x e FluentAssertions 7.x para testes unitários das regras de domínio.

## Ferramentas de Deploy (Bônus)
- **Containerização:** Docker — Dockerfile multi-stage otimizado para **.NET 10** (`mcr.microsoft.com/dotnet/aspnet:10.0` / `mcr.microsoft.com/dotnet/sdk:10.0`).
- **Plataforma de Deploy:** Render, Railway ou Fly.io (hospedagem gratuita ou de baixo custo para APIs Web).