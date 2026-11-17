# MedicalPrescription

Projeto demonstrativo de gestão de prescrições médicas, desenvolvido para avaliação técnica full-stack. **Não é um produto clínico homologado nem deve ser utilizado para prescrições reais.**

## Tecnologias

- **Backend:** C#, .NET 9, ASP.NET Core Web API, Entity Framework Core, MySQL 8.4 e OData.
- **Frontend:** Angular 21, TypeScript, componentes standalone, Reactive Forms e RxJS.
- **Testes:** xUnit, testes de integração com Testcontainers/MySQL e testes Angular.
- **Ambiente:** Docker Desktop, Docker Compose (MySQL) e Visual Studio ou .NET CLI.

## Regras funcionais

O sistema permite cadastrar, listar, consultar, editar e excluir prescrições. Existem dois tipos:

- **Não estruturada:** texto livre (ECF.10.01).
- **Estruturada:** itens de medicamentos, inclusive não cadastrados no catálogo, com forma farmacêutica, dosagem, frequência, início, duração/uso contínuo, via de administração, prazo e observações (ECF.10.03).

O frontend disponibiliza pesquisa por nome no catálogo de medicamentos e consulta os registros pela API. Dosagens são validadas com até três casas decimais; o armazenamento utiliza `decimal(12,3)`.

## Arquitetura

```text
backend/MedicalPrescription/
  MedicalPrescription.Domain/           Entidades, regras e Value Objects
  MedicalPrescription.Application/      Use Cases, Commands, Queries, contratos e DTOs
  MedicalPrescription.Infrastructure/   EF Core, migrations e repositórios
  MedicalPrescription.Api/              Controllers, DI, OData e ProblemDetails
  MedicalPrescription.UnitTests/        Testes unitários
  MedicalPrescription.IntegrationTests/ Testes com MySQL isolado
frontend/src/app/
  features/                              Módulos por funcionalidade
  app.config.ts                          Provedores e injeção de dependências
  app.routes.ts                          Rotas e lazy loading
```

Dependências do backend apontam para o interior da arquitetura: Domain não depende de outras camadas; Application depende de Domain; Infrastructure implementa contratos; API é o ponto de composição. Há separação lógica entre comandos e consultas, sem necessidade de MediatR.

## Pré-requisitos

- .NET SDK 9
- Docker Desktop em execução
- Git
- Para executar Angular fora do Docker: Node.js 22 e npm compatível

## Executar localmente no Windows

1. Crie o arquivo `.env` na raiz usando `.env.example` como referência. Não versione senhas.
2. Suba somente o MySQL:

   ```powershell
   docker compose up -d mysql
   docker compose ps
   ```

3. Configure a connection string em **User Secrets** (substitua os dados de exemplo pelas credenciais usadas no `.env`):

   ```powershell
   dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3307;Database=medical_prescription;User=medical_user;Password=SUA_SENHA" --project backend/MedicalPrescription/MedicalPrescription.Api
   ```

4. Instale a ferramenta EF Core local e aplique as migrations:

   ```powershell
   dotnet tool restore
   dotnet ef database update --project backend/MedicalPrescription/MedicalPrescription.Infrastructure --startup-project backend/MedicalPrescription/MedicalPrescription.Api
   ```

5. Execute a API com o perfil HTTP (porta `5080`):

   ```powershell
   dotnet run --project backend/MedicalPrescription/MedicalPrescription.Api --launch-profile http
   ```

6. Em outro PowerShell, na raiz do repositório, execute o Angular em Docker:

   ```powershell
   docker run --rm -it -p 4200:4200 -v "${PWD}\frontend:/workspace/frontend" -w /workspace/frontend node:22 sh -c "npm ci && npx ng serve --host 0.0.0.0 --poll 1000 --proxy-config src/proxy.conf.json"
   ```

Abra `http://localhost:4200/prescricoes`. O proxy do frontend foi configurado para usar `http://host.docker.internal:5080`, adequado ao cenário Docker Desktop + API executando no Windows.

> **Nota:** o `docker-compose.yml` atual provisiona apenas o MySQL. A conteinerização completa (API e Angular) e CI/CD são etapas posteriores; não estão representadas como concluídas neste README.

## Endpoints principais

| Método | Caminho | Finalidade |
|---|---|---|
| GET | `/api/prescricoes` | Listar com paginação |
| GET | `/api/prescricoes/{id}` | Obter detalhes |
| POST | `/api/prescricoes` | Criar |
| PUT | `/api/prescricoes/{id}` | Atualizar |
| DELETE | `/api/prescricoes/{id}` | Excluir |
| GET | `/api/medicamentos?busca=...` | Pesquisar catálogo |
| GET | `/api/pacientes?busca=...` | Pesquisar pacientes |
| GET | `/odata/ConsultaPrescricoes` | Consultar com OData |

As falhas HTTP usam `ProblemDetails` com `traceId`. As datas `CriadaEmUtc` são tratadas como UTC na API e na persistência.

## Testes e build

```powershell
dotnet build backend/MedicalPrescription/MedicalPrescription.slnx
dotnet test backend/MedicalPrescription/MedicalPrescription.slnx
```

Para build/testes Angular em Docker:

```powershell
docker run --rm -v "${PWD}\frontend:/workspace/frontend" -w /workspace/frontend node:22 sh -c "npm ci && npm run build && npm test -- --watch=false"
```

Os testes de integração iniciam contêineres MySQL temporários com Testcontainers e não devem modificar o banco de desenvolvimento.

## Modelo relacional

O script de criação está em `database/schema.sql`. O diagrama ER está documentado em `docs/DER.md` quando integrado à branch principal.

## Limitações e próximos passos

Este é um projeto demonstrativo: não inclui autenticação/autorização clínica, assinatura digital, certificação SBIS ou integração com sistemas de saúde. Ainda são previstos Dockerfiles, Compose completo, pipeline CI/CD e validação integral em ambiente limpo.
