# Spec Atual: API Segura de Reservas e Autenticação (DDD)

## Objetivo
Implementar a API Web estruturada em DDD com encapsulamento de POO, garantindo autenticação JWT, proteção contra sobreposição de horários e preparação para containerização (Docker) visando o deploy.

## Contrato da API (Endpoints Protegidos)
- `POST /api/v1/auth/login` (Público) - Autentica credenciais e retorna Token JWT.
- `POST /api/v1/professores` (Restrito a Admin) - Cadastra novo professor com senha segura.
- `GET /api/v1/salas/grid?data=YYYY-MM-DD` (Autenticado) - Retorna o estado das salas.
- `POST /api/v1/reservas` (Restrito a Professor) - Cria reserva validando conflitos via regras de domínio.
- `PUT /api/v1/reservas/{id}` (Restrito ao Autor) - Edita dados da reserva.
- `DELETE /api/v1/reservas/{id}` (Restrito ao Autor) - Remove a reserva.

## Checklist de Entrega
- [ ] Configurar a arquitetura DDD dividida em Domain, Application, Infrastructure e Api.
- [ ] Implementar a Entidade Rica `Reserva` no Domain com encapsulamento estrito (setters privados) e validação de conflito de horários.
- [ ] Configurar Autenticação JWT e PasswordHasher na Infrastructure.
- [ ] Implementar tratamento global de exceções na API para segurança de respostas.
- [ ] Criar o `Dockerfile` multi-stage na raiz (base image `.NET 10`) para o bônus de deploy.
- [ ] Subir o repositório completo para o GitHub com a pasta `/docs` contendo a apresentação.