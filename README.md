# MedicalPrescription

Projeto demonstrativo full-stack para gestao de prescricoes medicas. **Nao e um produto clinico homologado e nao deve ser utilizado para prescricoes reais.**

**Tecnologias:** .NET 9, ASP.NET Core Web API, Entity Framework Core, MySQL 8.4, OData, Angular 21, TypeScript, RxJS, Docker Compose, Nginx, xUnit, Testcontainers e Vitest.

## Comece aqui: instalar em uma maquina limpa

Este e o caminho recomendado para quem quer **executar a aplicacao completa sem instalar .NET, Node.js, MySQL ou Angular localmente**.

### 1. Pre-requisitos

- **Git**, para clonar o repositorio.
- **Docker Desktop com Docker Compose v2**, iniciado e usando conteineres Linux. No Windows, habilite o backend WSL 2 se solicitado pelo Docker Desktop.
- Conexao com a internet para baixar imagens e dependencias; espaco livre suficiente para construir as imagens.

Verifique no terminal:

```powershell
git --version
docker --version
docker compose version
docker info
```

> O ultimo comando so funciona se o Docker Engine estiver em execucao. Nao e necessario ter Visual Studio, MySQL, .NET SDK ou Node.js instalados para o fluxo Docker abaixo.

### 2. Clonar a branch com a aplicacao completa

A integracao da solucao foi realizada em **`develop`**. Ate que `main` seja atualizada, use explicitamente essa branch:

```powershell
git clone --branch develop https://github.com/rafaelrocha011/medical-prescription.git
cd medical-prescription
```

### 3. Configurar credenciais locais

Crie o arquivo **`.env` na raiz**, a partir de **`.env.example`**:

**Windows PowerShell:**

```powershell
Copy-Item .env.example .env
notepad .env
```

**Linux/macOS:**

```bash
cp .env.example .env
```

Altere as senhas dos campos `MYSQL_ROOT_PASSWORD` e `MYSQL_PASSWORD` para valores locais fortes. Exemplo de estrutura (nao reutilize estas senhas):

```dotenv
MYSQL_ROOT_PASSWORD=DEFINA_UMA_SENHA_ROOT_FORTE
MYSQL_DATABASE=medical_prescription
MYSQL_USER=medical_user
MYSQL_PASSWORD=DEFINA_UMA_SENHA_DO_USUARIO
MYSQL_PORT=3307
```

O arquivo `.env` esta ignorado pelo Git. Nao envie credenciais ao repositorio. **Se alterar senhas depois da primeira inicializacao do MySQL, elas nao serao aplicadas automaticamente ao banco ja existente no volume persistente.**

### 4. Construir e iniciar a solucao

Execute **na raiz do repositorio**:

```powershell
docker compose config --quiet
docker compose up -d --build
docker compose ps -a
```

O Docker Compose disponibiliza:

| Servico | O que faz | Acesso na maquina |
|---|---|---|
| `mysql` | Banco MySQL 8.4, com volume persistente | `127.0.0.1:3307` |
| `migrate` | Executa as migrations EF Core antes da API | Finaliza com codigo `0` |
| `api` | API ASP.NET Core 9; conecta-se a `mysql:3306` na rede Docker | `http://localhost:5080` |
| `frontend` | Angular compilado, servido pelo Nginx | `http://localhost:4200` |

**Resultado esperado:** `mysql`, `api` e `frontend` com status **healthy**, e `migrate` com **Exited (0)**. O contêiner de migrations nao precisa permanecer executando.

A construcao inicial pode levar mais tempo por causa do download das imagens e dos pacotes.

### 5. Abrir e testar

- **Aplicacao:** http://localhost:4200/prescricoes
- **Consulta REST:** http://localhost:5080/api/prescricoes
- **Consulta OData:** http://localhost:5080/odata/ConsultaPrescricoes
- **API via Nginx:** http://localhost:4200/api/prescricoes

No PowerShell, teste a resposta da API:

```powershell
Invoke-RestMethod "http://localhost:5080/api/prescricoes?pagina=1&tamanhoPagina=10"
```

O Angular acessa a API pelo mesmo host usando o proxy reverso do **Nginx** (`/api/` e `/odata/`); nao precisa configurar CORS nem `host.docker.internal` nesse modo. O projeto roda em HTTP local, apropriado apenas para avaliacao e desenvolvimento.

**Primeiro acesso:** quando a API roda com `ASPNETCORE_ENVIRONMENT=Development`, o inicializador de desenvolvimento insere pacientes e medicamentos de exemplo caso os respectivos catalogos estejam vazios. As prescricoes sao criadas pelo usuario; o banco nao precisa conter prescricoes inicialmente.

## Operacao diaria

Execute os comandos na raiz do repositorio:

```powershell
# Consultar estado dos servicos
docker compose ps -a

# Visualizar logs
docker compose logs --tail=100 api
docker compose logs --tail=100 frontend
docker compose logs --tail=100 migrate
docker compose logs --tail=100 mysql

# Parar mantendo os dados
docker compose down

# Iniciar novamente
docker compose up -d
```

O volume nomeado `mysql_data` preserva as informacoes entre reinicializacoes. **NAO execute `docker compose down -v` se quiser conservar o banco.** `docker compose down` sem `-v` preserva os volumes nomeados.

Para reconstruir apos atualizacoes do codigo:

```powershell
git pull --ff-only origin develop
docker compose up -d --build
```

### Portas ocupadas

O Compose vincula as portas a `127.0.0.1`. Se `4200`, `5080` ou `3307` estiverem em uso, altere/adicione no `.env`:

```dotenv
FRONTEND_PORT=4201
API_PORT=5081
MYSQL_PORT=3308
```

Depois, reinicie os servicos com `docker compose up -d` e use as novas URLs. **Nao execute simultaneamente a API pelo Visual Studio ou o Angular por `ng serve` usando as mesmas portas.**

### Solucao de problemas

| Sintoma | O que verificar |
|---|---|
| `docker compose` nao funciona | Docker Desktop iniciado; Compose v2 disponivel; conteineres Linux |
| Erro de variavel obrigatoria | Confirme a existencia de `.env` na raiz e os campos `MYSQL_...` |
| Porta ja utilizada | Pare a aplicacao concorrente ou altere as portas no `.env` |
| `migrate` finalizou com erro | Consulte `docker compose logs migrate` e `docker compose logs mysql` |
| API ou frontend nao ficam saudaveis | Consulte `docker compose ps -a` e os logs do servico |
| Lista vazia | Verifique se a API responde; em um banco novo pode nao haver prescricoes |
| Problemas apos alterar credenciais | As senhas nao sao redefinidas em um volume MySQL ja inicializado |

**Atencao ao banco:** nunca use `down -v` ou remova volumes como primeira tentativa de correcao. Isso apaga os dados persistidos.

## Desenvolvimento sem rodar toda a stack no Docker (opcional)

Se desejar executar a API no Visual Studio e o Angular com hot reload, instale .NET SDK 9 e Node.js 22. O modo abaixo e **alternativo** ao Compose completo: nao rode as duas formas da API/frontend nas mesmas portas.

1. Suba apenas o MySQL: `docker compose up -d mysql`.
2. Configure `ConnectionStrings:MySql` via User Secrets no projeto `backend/MedicalPrescription/MedicalPrescription.Api`, com `Server=localhost;Port=3307` e as credenciais do `.env`.
3. Aplique migrations com `dotnet tool restore` e `dotnet ef database update --project backend/MedicalPrescription/MedicalPrescription.Infrastructure --startup-project backend/MedicalPrescription/MedicalPrescription.Api`.
4. Inicie a API: `dotnet run --project backend/MedicalPrescription/MedicalPrescription.Api --launch-profile http`.
5. Em `frontend/`, execute `npm ci` e `npx ng serve --proxy-config src/proxy.conf.json`.

O proxy de desenvolvimento Angular usa `host.docker.internal:5080` para acesso a uma API executando no host Windows quando o Angular estiver em contêiner. Se rodar `ng serve` nativamente, ajuste o destino do proxy para `http://localhost:5080`, caso necessario.

## Arquitetura

```text
backend/MedicalPrescription/
  MedicalPrescription.Domain/           Entidades, regras e Value Objects
  MedicalPrescription.Application/      Use Cases, Commands, Queries, contratos e DTOs
  MedicalPrescription.Infrastructure/   EF Core, migrations e repositorios
  MedicalPrescription.Api/              Endpoints REST/OData, DI e ProblemDetails
  MedicalPrescription.UnitTests/        Testes unitarios xUnit
  MedicalPrescription.IntegrationTests/ Testcontainers/MySQL
frontend/
  src/app/features/                     Funcionalidades, adaptadores, paginas e componentes
  Dockerfile                             Build Angular + Nginx
  nginx.conf                             SPA + proxy da API
docker-compose.yml                       MySQL, migrations, API e frontend
docker-compose.deploy.yml                Override para imagens versionadas de homologacao
.github/workflows/                       CI e deploy manual
```

O backend utiliza Clean Architecture, SOLID e separacao logica de comandos e consultas (CQRS). O frontend usa Angular standalone, Reactive Forms e contratos desacoplados de adaptadores HTTP.

### Funcionalidades

- Criacao, consulta, listagem, edicao e exclusao de prescricoes.
- Prescricoes nao estruturadas (texto livre) e estruturadas (itens de medicamentos).
- Medicamentos do catalogo ou preenchimento manual, dosagem com ate tres casas decimais, frequencia, inicio, duracao/uso continuo, via, prazo e observacoes.
- Listagem paginada com nome do paciente e detalhes expansiveis.
- OData para consultas; erros HTTP no formato ProblemDetails.

## Testes e CI/CD

**Execucao local opcional (com SDK .NET 9, Node 22 e Docker instalado):**

```powershell
dotnet test backend/MedicalPrescription/MedicalPrescription.slnx
cd frontend
npm ci
npm run build
npm test -- --watch=false
```

Os testes de integracao utilizam MySQL temporario via Testcontainers; requerem Docker Engine acessivel. Eles nao utilizam o volume `mysql_data` do Compose.

**GitHub Actions:** o workflow `.github/workflows/ci.yml` executa build e testes em Pull Requests e em pushes na `develop`. Apos sucesso na `develop`, publica as imagens `api`, `frontend` e `migrate` no GitHub Container Registry (GHCR), com tag `sha-<commit>`. O workflow `deploy.yml` permite **deploy manual de homologacao**, mas exige runner e credenciais configurados; publicar imagem nao equivale a executar deploy. Mais detalhes em [docs/CI-CD.md](docs/CI-CD.md).

## Banco, modelo e limitacoes

- O esquema e gerenciado pelas migrations EF Core; o contêiner `migrate` as aplica na inicializacao.
- Modelo relacional em `database/schema.sql`; diagrama ER em `docs/DER-MedicalPrescription.md`.
- Este projeto demonstrativo **nao oferece** autenticacao clinica, assinatura digital, homologacao/certificacao SBIS ou infraestrutura para prontuario real. Use somente dados ficticios.
- Credenciais locais ficam em `.env` (nao versionado); para producao seria necessario gerenciar segredos, HTTPS, autorizacao, auditoria e backups.
