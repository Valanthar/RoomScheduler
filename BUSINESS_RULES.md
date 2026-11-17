# Regras de Negócio e Segurança

## 1. Gestão de Identidade e Segurança de Acesso
- **Regra 1 (Cadastro Administrativo):** Contas de professores e administradores só podem ser criadas por um usuário autenticado com perfil `Admin`.
- **Regra 2 (Hash de Senha Seguro):** Nenhuma senha é armazenada em texto plano. O sistema utiliza hashing criptográfico forte com salt.
- **Regra 3 (Controle de Sessão):** O login gera um token JWT criptografado. Tentativas de acesso sem token válido resultam em **Erro 401 (Unauthorized)**.
- **Regra 4 (Autorização baseada em Roles):** Endpoints administrativos rejeitam requisições de professores comuns com **Erro 403 (Forbidden)**.

## 2. Domínio de Reservas (Proteção de Invariantes via POO)
- **Regra 5 (Indivisibilidade e Conflito de Horário):** A entidade de domínio `Reserva` garante por encapsulamento que uma sala não pode ter sobreposição de horários.
- **Regra 6 (Bloqueio de Passado):** Tentativas de criar reservas para datas ou horários retroativos devem ser rejeitadas diretamente pelo construtor/método de validação da entidade.
- **Regra 7 (Propriedade Exclusiva):** Apenas o autor da reserva (validado pelo ID extraído de forma segura do token JWT atual) pode alterar a matéria ou cancelar o agendamento. Modificações por terceiros retornam **Erro 403**.

## 3. Chamados de Suporte (Bugs)
- **Regra 8:** Abertura de chamados vinculada automaticamente ao ID do professor autenticado, garantindo rastreabilidade e auditoria.