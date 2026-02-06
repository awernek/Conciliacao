# Guia de Contribuição

Obrigado pelo interesse em contribuir com o Projeto Conciliação! Este documento define o processo para contribuir com código e documentação.

## 🚀 Como executar o projeto

Você pode rodar o projeto de duas formas:

### Opção 1: Docker (Recomendado)
Necessário Docker e Docker Compose instalados.

```bash
docker compose up --build
```
A API estará disponível em `http://localhost:5000/swagger`.

### Opção 2: Visual Studio / .NET CLI
Necessário .NET 10 SDK e SQL Server (ou LocalDB).

1. Configure a connection string `DefaultConnection` no `appsettings.json`.
2. Rode as migrações (se necessário):
   ```bash
   dotnet ef database update --project Conciliacao.Infra --startup-project Conciliacao.Api
   ```
3. Execute:
   ```bash
   dotnet run --project Conciliacao.Api
   ```

## 🧪 Testes

Antes de enviar seu PR, garanta que todos os testes estão passando:

```bash
dotnet test
```

## 📐 Padrões de Código

### Clean Architecture
Respeite a separação de camadas:
- **Domain**: Entidades puras, regras de negócio. Zero dependência externa.
- **Application**: Casos de uso. Use DTOs para entrada/saída.
- **Infra**: Implementações de banco de dados e frameworks.
- **API**: Apenas controllers e configuração.

### Estilo
- Use `PascalCase` para métodos e classes.
- Use `camelCase` para variáveis locais e parâmetros.
- Use `_camelCase` para campos privados.
- Evite commits gigantes. Quebre em mudanças lógicas menores.

## 🤝 Processo de Pull Request

1. Faça um Fork do repositório.
2. Crie uma branch para sua feature (`git checkout -b feature/minha-feature`).
3. Comite suas mudanças seguindo o padrão [Conventional Commits](https://www.conventionalcommits.org/) (ex: `feat: adiciona nova regra de conciliação`).
4. Empurre para o seu fork (`git push origin feature/minha-feature`).
5. Abra um Pull Request para a branch `master`.

## 📝 Documentação

- Se você mudar uma decisão arquitetural, crie ou atualize um **ADR** em `docs/adr/`.
- Se mudar a API, atualize o Swagger (automático) e verifique se a documentação em `docs/` precisa de ajustes.
