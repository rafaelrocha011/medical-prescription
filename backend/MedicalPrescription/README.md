# Backend — MedicalPrescription

API REST em **.NET 9 / ASP.NET Core**, com **Entity Framework Core**, **MySQL 8.4** e endpoint de consulta **OData**. Projeto demonstrativo para avaliação técnica; não se destina à prescrição clínica real.

## Estrutura da solução

```text
backend/MedicalPrescription/
├── MedicalPrescription.Domain/             # Entidades, regras, Value Objects e exceções
├── MedicalPrescription.Application/        # Use Cases, Commands, Queries, interfaces e DTOs
├── MedicalPrescription.Infrastructure/     # DbContext, mapeamentos EF Core, migrations e repositórios
├── MedicalPrescription.Api/                # Controllers, configuração de DI, OData e tratamento de erros
├── MedicalPrescription.UnitTests/          # Testes unitários (xUnit)
├── MedicalPrescription.IntegrationTests/   # Integração com MySQL isolado via Testcontainers
└── MedicalPrescription.slnx
```

### Dependências entre camadas

- **Domain:** não referencia outras camadas da solução.
- **Application:** referencia Domain e declara os contratos de persistência e dos Use Cases.
- **Infrastructure:** implementa os contratos da Application e referencia Domain.
- **API:** compõe as dependências e expõe os casos de uso via HTTP; referencia Application e Infrastructure.

A implementação utiliza CQRS **simplificado**: Commands para escrita e Queries para leitura, com repositórios correspondentes. Não exige MediatR para funcionar. A entidade `Prescricao` é a raiz do agregado e controla seus itens; `Dosagem` é um Value Object.

## Requisitos

- **.NET SDK 9**
- **Docker Desktop** (MySQL local e Testcontainers nos testes de integração)
- Ferramenta `dotnet-ef` declarada em `.config/dotnet-tools.json`, na raiz do repositório

## Executar no Windows

Execute os comandos **na raiz do repositório** (`C:\Projetos\prescricao-medica`).

1. Crie um `.env` a partir de `.env.example` e substitua os valores de exemplo por credenciais locais. Não versione o arquivo `.env`.
2. Inicie o MySQL:

   ```powershell
   docker compose up -d mysql
   docker compose ps
   ```

3. Configure a connection string em **User Secrets** do projeto API (use a mesma senha e porta do `.env`):

   ```powershell
   dotnet user-secrets set "ConnectionStrings:MySql" "Server=localhost;Port=3307;Database=medical_prescription;User=medical_user;Password=SUA_SENHA" --project backend/MedicalPrescription/MedicalPrescription.Api
   ```

4. Restaure a ferramenta EF e aplique as migrations ao MySQL local:

   ```powershell
   dotnet tool restore
   dotnet ef database update --project backend/MedicalPrescription/MedicalPrescription.Infrastructure --startup-project backend/MedicalPrescription/MedicalPrescription.Api
   ```

5. Inicie a API no perfil HTTP:

   ```powershell
   dotnet run --project backend/MedicalPrescription/MedicalPrescription.Api --launch-profile http
   ```

A API ficará disponível em `http://localhost:5080`. O seeder de desenvolvimento cria pacientes e medicamentos de exemplo quando o banco já está migrado e as respectivas tabelas estão vazias.

> **Importante:** no `docker-compose.yml` atual, somente o MySQL está conteinerizado. API e frontend continuam executando separadamente até a etapa de Dockerfiles/Compose completo.

## Rotas da API

| Método | Rota | Operação |
|---|---|---|
| `GET` | `/api/prescricoes?pagina=1&tamanhoPagina=20` | Listagem paginada |
| `GET` | `/api/prescricoes/{id}` | Detalhes |
| `POST` | `/api/prescricoes` | Criação |
| `PUT` | `/api/prescricoes/{id}` | Atualização |
| `DELETE` | `/api/prescricoes/{id}` | Exclusão |
| `GET` | `/api/pacientes?busca=...` | Pesquisa de pacientes |
| `GET` | `/api/medicamentos?busca=...` | Pesquisa de medicamentos |
| `GET` | `/odata/ConsultaPrescricoes` | Consultas OData com limites configurados |

Erros são retornados por `ProblemDetails`, com `traceId`. Exceções de regras de negócio, entradas inválidas, recursos inexistentes e falhas internas são tratadas distintamente.

## Regras de negócio demonstradas

- **ECF.10.01:** prescrição não estruturada (texto livre).
- **ECF.10.03:** prescrição estruturada com itens e campos de forma farmacêutica, dose e unidade, frequência, início e duração/uso contínuo, via de administração, prazo e observações.
- É possível prescrever medicamentos **não cadastrados** no catálogo; `medicamento_id` é opcional.
- `Dosagem` valida quantidade positiva, limite de `decimal(12,3)` e unidade de medida.
- A API persiste e retorna `CriadaEmUtc` como horário UTC, evitando divergência entre REST e OData.

## Build e testes

Execute na raiz do repositório:

```powershell
dotnet restore backend/MedicalPrescription/MedicalPrescription.slnx
dotnet build backend/MedicalPrescription/MedicalPrescription.slnx
dotnet test backend/MedicalPrescription/MedicalPrescription.slnx
```

Os testes de integração usam **Testcontainers** para subir instâncias temporárias de MySQL e aplicar as migrations. Mantenha o Docker Desktop ligado. Esses testes não devem utilizar o banco de desenvolvimento.

## Banco de dados e documentação

- Script SQL de criação: `database/schema.sql` (caminho relativo à raiz do repositório).
- DER: `docs/DER.md`, após integração da documentação à branch principal.
- As migrations versionadas ficam no projeto `MedicalPrescription.Infrastructure`.

## Limitações

Aplicação demonstrativa, sem autenticação clínica, assinatura digital ou homologação SBIS. Ainda dependem de implementação/validação: Dockerfile da API, Compose completo e pipelines de CI/CD exigidos no teste técnico.
