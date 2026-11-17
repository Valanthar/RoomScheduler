# Arquitetura do Sistema (DDD e POO)

## Camadas e Princípios de DDD
1. **Domain (Domínio):** O núcleo da aplicação. Contém as Entidades ricas, Objetos de Valor (Value Objects), Agregados e Interfaces de Repositório. 
   - *Regra de POO:* **Encapsulamento estrito**. Propriedades possuem construtores protegidos e setters privados (`private set`). Nenhuma alteração de estado é feita sem passar por métodos de negócio que validam as invariantes.
2. **Application (Aplicação):** Casos de Uso (Services/Handlers). Orquestram as transações, recebem DTOs, convertem para objetos de domínio e chamam os repositórios. Não contêm regras de negócio puras.
3. **Infrastructure (Infraestrutura):** Implementa o acesso ao banco de dados via EF Core, configurações de persistência, mapeamentos e geração de tokens JWT.
4. **Presentation / API (Apresentação):** Controladores HTTP (`Controllers`), tratamento global de exceções, filtros de autorização e injeção de dependência.

## Diretrizes de Segurança na Arquitetura
- **Princípio da Responsabilidade Única (SOLID):** Cada camada tem uma atribuição clara, impedindo vazamento de lógica de banco para a API.
- **Validação Antecipada:** Dados de entrada são validados na camada de API/Application antes de tocar no domínio.
- **Tratamento Global de Erros:** Erros internos do servidor nunca expovem detalhes de pilha (*stack traces*) ao cliente; apenas códigos de status padronizados (RFC 7807).