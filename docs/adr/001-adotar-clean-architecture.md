# 1. Adotar Clean Architecture

Data: 2026-02-06
Status: Aceito

## Contexto
O projeto Conciliação precisa evoluir de forma sustentável, permitindo a troca de banco de dados, interfaces de usuário ou frameworks externos sem impactar as regras de negócio. O domínio (regras de conciliação) é complexo e deve ser isolado.

## Decisão
Adotamos a **Clean Architecture** (Arquitetura Limpa), dividindo o projeto em 4 camadas com dependência unidirecional (de fora para dentro):

1. **Conciliacao.Domain** (Núcleo): Entidades, Value Objects, Interfaces de Repositório, Policies e Exceptions. Não depende de ninguém.
2. **Conciliacao.Application**: Casos de uso e orquestração. Depende apenas do Domain.
3. **Conciliacao.Infra**: Implementação concreta (EF Core, SQL Server). Depende do Domain (implementa interfaces).
4. **Conciliacao.Api**: Entry point. Depende de Application e Infra (para DI).

## Consequências
- **Positivo**: O `Domain` é puro C# e fácil de testar unitariamente.
- **Positivo**: O `Application` não conhece Entity Framework, evitando vazamento de abstração.
- **Negativo**: Aumenta o número de classes e arquivos (DTOs, Mappers, Interfaces).
- **Negativo**: Exige mapeamento constante (Entity <-> DTO).
