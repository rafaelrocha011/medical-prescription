# MedicalPrescription

Projeto demonstrativo full-stack para gestão de prescrições médicas. **Não é um produto clínico homologado e não deve ser utilizado para prescrições reais.**

**Tecnologias:** .NET 9, ASP.NET Core Web API, Entity Framework Core, MySQL 8.4, OData, Angular 21, TypeScript, RxJS, Docker Compose, Nginx, xUnit, Testcontainers e Vitest.

## Comece aqui: instalar em uma máquina limpa

Este é o caminho recomendado para quem quer **executar a aplicação completa sem instalar .NET, Node.js, MySQL ou Angular localmente**.

### 1. Pré-requisitos

- **Git**, para clonar o repositório.
- **Docker Desktop com Docker Compose v2**, iniciado e usando contêineres Linux. No Windows, habilite o backend WSL 2 se solicitado pelo Docker Desktop.
- Conexão com a internet para baixar imagens e dependências; espaço livre suficiente para construir as imagens.

Verifique no terminal:

```powershell
git --version
docker --version
docker compose version
docker info
```

> O último comando só funciona se o Docker Engine estiver em execução. Não é necessário ter Visual Studio, MySQL, .NET SDK ou Node.js instalados para o fluxo Docker abaixo.

### 2. Clonar a versão de entrega

A versão de entrega validada está na branch **`main`**. Use essa branch explicitamente para obter o código destinado a avaliação:

```powershell
git clone --branch main https://github.com/rafaelrocha011/medical-prescription.git
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

Altere as senhas dos campos `MYSQL_ROOT_PASSWORD` e `MYSQL_PASSWORD` para valores locais fortes. Exemplo de estrutura (não reutilize estas senhas):

```dotenv
MYSQL_ROOT_PASSWORD=DEFINA_UMA_SENHA_ROOT_FORTE
MYSQL_DATABASE=medical_prescription
MYSQL_USER=medical_user
MYSQL_PASSWORD=DEFINA_UMA_SENHA_DO_USUARIO
MYSQL_PORT=3307
```

O arquivo `.env` está ignorado pelo Git. Não envie credenciais ao repositório. **Se alterar senhas depois da primeira inicialização do MySQL, elas não serão aplicadas automaticamente ao banco já existente no volume persistente.**

### 4. Construir e iniciar a solução

Execute **na raiz do repositório**:

```powershell
docker compose config --quiet
docker compose up -d --build
docker compose ps -a
```

O Docker Compose disponibiliza:

| Serviço | O que faz | Acesso na máquina |
|---|---|---|
| `mysql` | Banco MySQL 8.4, com volume persistente | `127.0.0.1:3307` |
| `migrate` | Executa as migrations EF Core antes da API | Finaliza com código `0` |
| `api` | API ASP.NET Core 9; conecta-se a `mysql:3306` na rede Docker | `http://localhost:5080` |
| `frontend` | Angular compilado, servido pelo Nginx | `http://localhost:4200` |

**Resultado esperado:** `mysql`, `api` e `frontend` com status **healthy**, e `migrate` com **Exited (0)**. O contêiner de migrations não precisa permanecer executando.

A construção inicial pode levar mais tempo por causa do download das imagens e dos pacotes.

### 5. Abrir e testar

- **Aplicação:** http://localhost:4200/prescricoes
- **Swagger UI (documentação interativa):** http://localhost:5080/swagger
- **Especificação OpenAPI JSON:** http://localhost:5080/openapi/v1.json
- **Consulta REST:** http://localhost:5080/api/prescricoes
- **Consulta OData:** http://localhost:5080/odata/ConsultaPrescricoes
- **API via Nginx:** http://localhost:4200/api/prescricoes

No PowerShell, teste a resposta da API:

```powershell
Invoke-RestMethod "http://localhost:5080/api/prescricoes?pagina=1&tamanhoPagina=10"
```

O Angular acessa a API pelo mesmo host usando o proxy reverso do **Nginx** (`/api/` e `/odata/`); não precisa configurar CORS nem `host.docker.internal` nesse modo. O projeto roda em HTTP local, apropriado apenas para avaliação e desenvolvimento.

**Swagger UI:** disponível somente no ambiente `Development` definido no `docker-compose.yml` local. Ele usa a especificação OpenAPI nativa do .NET 9 e permite visualizar e testar as rotas REST pelo navegador, sem Postman. Não e habilitado no ambiente `Production` do deploy manual.

**Primeiro acesso:** quando a API roda com `ASPNETCORE_ENVIRONMENT=Development`, o inicializador de desenvolvimento insere pacientes e medicamentos de exemplo caso os respectivos catálogos estejam vazios. As prescrições sao criadas pelo usuário; o banco não precisa conter prescrições inicialmente.

### Garantir o seed na primeira instalacao

O seed não depende de importar `database/schema.sql`: o serviço `migrate` aplica as migrations, e a API, iniciada em **Development** pelo `docker-compose.yml`, executa `DevelopmentDataSeeder.SeedAsync`. O método insere **Paciente Exemplo Um**, **Paciente Exemplo Dois**, **Medicamento Exemplo A** e **Medicamento Exemplo B** quando cada catálogo estiver completamente vazio.

Confira o banco iniciado normalmente:

```powershell
(Invoke-RestMethod http://localhost:5080/api/pacientes) | Select-Object nomeCompleto
(Invoke-RestMethod http://localhost:5080/api/medicamentos) | Select-Object nome
```

Para simular **uma máquina limpa sem apagar o banco de desenvolvimento**, rode o teste automatizado abaixo a partir da raiz, com Docker Desktop ativo:

```powershell
./scripts/validar-primeira-instalacao.ps1
```

O teste cria um **projeto Compose com nome único e volume MySQL próprio**, usa as portas de avaliação `15080` (API), `14200` (frontend) e `13307` (MySQL), faz build, sobe a stack, verifica que os quatro registros esperados existem exatamente uma vez e reinicia a API para confirmar que o seed não duplica dados. Ao final remove **somente os contêineres e volumes do projeto temporário**. Para manter o ambiente de teste aberto, use `-KeepEnvironment`; nesse caso, remova-o manualmente depois.

O workflow de CI também executa esse teste de primeiro boot em ambiente isolado, antes de permitir a publicação de imagens Docker.

**Limite atual:** como o seed verifica se cada tabela está vazia, ele não recria um exemplo individual que tenha sido removido de uma tabela que ainda contém outros registros. Isso é intencional para não sobrescrever dados alterados durante o desenvolvimento. O seed é **apenas demonstrativo** e só roda quando a API usa `ASPNETCORE_ENVIRONMENT=Development`. Não use nomes fictícios ou seed de desenvolvimento em produção.

## Operação diária

Execute os comandos na raiz do repositório:

```powershell
# Consultar estado dos serviços
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

O volume nomeado `mysql_data` preserva as informações entre reinicializações. **NÃO execute `docker compose down -v` se quiser conservar o banco.** `docker compose down` sem `-v` preserva os volumes nomeados.

Para reconstruir após atualizações do código:

```powershell
git pull --ff-only origin main
docker compose up -d --build
```

### Portas ocupadas

O Compose vincula as portas a `127.0.0.1`. Se `4200`, `5080` ou `3307` estiverem em uso, altere/adicione no `.env`:

```dotenv
FRONTEND_PORT=4201
API_PORT=5081
MYSQL_PORT=3308
```

Depois, reinicie os serviços com `docker compose up -d` e use as novas URLs. **Não execute simultaneamente a API pelo Visual Studio ou o Angular por `ng serve` usando as mesmas portas.**

### Solução de problemas

| Sintoma | O que verificar |
|---|---|
| `docker compose` não funciona | Docker Desktop iniciado; Compose v2 disponível; contêineres Linux |
| Erro de variável obrigatória | Confirme a existência de `.env` na raiz e os campos `MYSQL_...` |
| Porta já utilizada | Pare a aplicação concorrente ou altere as portas no `.env` |
| `migrate` finalizou com erro | Consulte `docker compose logs migrate` e `docker compose logs mysql` |
| API ou frontend não ficam saudáveis | Consulte `docker compose ps -a` e os logs do serviço |
| Lista vazia | Verifique se a API responde; em um banco novo pode não haver prescrições |
| Problemas após alterar credenciais | As senhas não sao redefinidas em um volume MySQL já inicializado |

**Atenção ao banco:** nunca use `down -v` ou remova volumes como primeira tentativa de correção. Isso apaga os dados persistidos.

## Desenvolvimento sem rodar toda a stack no Docker (opcional)

Se desejar executar a API no Visual Studio e o Angular com hot reload, instale .NET SDK 9 e Node.js 22. O modo abaixo é **alternativo** ao Compose completo: não rode as duas formas da API/frontend nas mesmas portas.

1. Suba apenas o MySQL: `docker compose up -d mysql`.
2. Configure `ConnectionStrings:MySql` via User Secrets no projeto `backend/MedicalPrescription/MedicalPrescription.Api`, com `Server=localhost;Port=3307` e as credenciais do `.env`.
3. Aplique migrations com `dotnet tool restore` e `dotnet ef database update --project backend/MedicalPrescription/MedicalPrescription.Infrastructure --startup-project backend/MedicalPrescription/MedicalPrescription.Api`.
4. Inicie a API: `dotnet run --project backend/MedicalPrescription/MedicalPrescription.Api --launch-profile http`.
5. Em `frontend/`, execute `npm ci` e `npx ng serve --proxy-config src/proxy.conf.json`.

O proxy de desenvolvimento Angular usa `host.docker.internal:5080` para acesso a uma API executando no host Windows quando o Angular estiver em contêiner. Se rodar `ng serve` nativamente, ajuste o destino do proxy para `http://localhost:5080`, caso necessário.

## Arquitetura

```text
backend/MedicalPrescription/
  MedicalPrescription.Domain/           Entidades, regras e Value Objects
  MedicalPrescription.Application/      Use Cases, Commands, Queries, contratos e DTOs
  MedicalPrescription.Infrastructure/   EF Core, migrations e repositórios
  MedicalPrescription.Api/              Endpoints REST/OData, DI e ProblemDetails
  MedicalPrescription.UnitTests/        Testes unitarios xUnit
  MedicalPrescription.IntegrationTests/ Testcontainers/MySQL
frontend/
  src/app/features/                     Funcionalidades, adaptadores, paginas e componentes
  Dockerfile                             Build Angular + Nginx
  nginx.conf                             SPA + proxy da API
docker-compose.yml                       MySQL, migrations, API e frontend
docker-compose.deploy.yml                Override para imagens versionadas de homologação
.github/workflows/                       CI e deploy manual
```

O backend utiliza Clean Architecture, SOLID e separação lógica de comandos e consultas (CQRS). O frontend usa Angular standalone, Reactive Forms e contratos desacoplados de adaptadores HTTP.

### Funcionalidades

- Criação, consulta, listagem, edição e exclusão de prescrições.
- Prescrições não estruturadas (texto livre) e estruturadas (itens de medicamentos).
- Medicamentos do catálogo ou preenchimento manual, dosagem com até três casas decimais, frequência, início, duração/uso continuo, via, prazo e observações.
- Listagem paginada com nome do paciente e detalhes expansíveis.
- OData para consultas; erros HTTP no formato ProblemDetails.

## Testes e CI/CD

**Execução local opcional (com SDK .NET 9, Node 22 e Docker instalado):**

```powershell
dotnet test backend/MedicalPrescription/MedicalPrescription.slnx
cd frontend
npm ci
npm run build
npm test -- --watch=false
```

Os testes de integração utilizam MySQL temporário via Testcontainers; requerem Docker Engine acessível. Eles não utilizam o volume `mysql_data` do Compose.

**GitHub Actions:** o workflow `.github/workflows/ci.yml` executa build, testes .NET/Angular e o teste de primeiro boot em MySQL vazio nos Pull Requests e pushes para `develop` e `main`. Em pushes aprovados nessas branches, publica as imagens `api`, `frontend` e `migrate` no GitHub Container Registry (GHCR), identificadas por `sha-<commit>`. O workflow `deploy.yml` permite **deploy manual de homologação**, mas exige runner e credenciais configurados; publicar imagem não equivale a executar deploy. Mais detalhes em [docs/CI-CD.md](docs/CI-CD.md).

## Banco, modelo e limitações

- O esquema é gerenciado pelas migrations EF Core; o contêiner `migrate` as aplica na inicialização.
- Modelo relacional em `database/schema.sql`; diagrama ER em `docs/DER-MedicalPrescription.md`.
- Este projeto demonstrativo **não oferece** autenticação clínica, assinatura digital, homologação/certificação SBIS ou infraestrutura para prontuario real. Use somente dados fictícios.
- Credenciais locais ficam em `.env` (não versionado); para produção seria necessário gerenciar segredos, HTTPS, autorização, auditoria e backups.
