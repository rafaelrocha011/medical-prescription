# CI/CD - MedicalPrescription

## Fluxo das branches e entrega

- `develop`: integracao de funcionalidades, com CI automatico.
- `release/1.0.0`: branch de preparacao da entrega, derivada de `develop`.
- `main`: versao de entrega para o avaliador. O README principal ensina a instalar a partir desta branch.

O fluxo recomendado e abrir um Pull Request de `release/1.0.0` para `main`. Antes do merge, conferir os tres jobs obrigatorios no GitHub Actions. Depois do merge, verificar os builds das tres imagens Docker e a publicacao no GHCR.

> O repositorio ainda pode ter a branch `develop` configurada como padrao no GitHub. Para obter a versao final sem depender dessa configuracao, clone com `git clone --branch main ...`. Caso queira mudar a branch padrao no GitHub, use Settings > General > Default branch (exige permissao administrativa).

## Integracao continua (CI)

Arquivo: `.github/workflows/ci.yml`.

Triggers:
- Pull Requests para `develop` **ou** `main`: build e testes, sem publicar imagens.
- Pushes para `develop` **ou** `main`: repetem testes e, se aprovados, publicam imagens.
- `workflow_dispatch`: permite execucao manual de CI.

Jobs de validacao:

1. **Build e testes .NET 9:** restore, build Release, xUnit unitario e Testcontainers/MySQL de integracao; publica binarios e relatorios TRX.
2. **Build e testes Angular 21:** `npm ci`, build de producao, Vitest; publica os arquivos estaticos gerados.
3. **Primeiro boot - seed em MySQL vazio:** utiliza `scripts/validar-primeira-instalacao.ps1` para subir a stack em projeto Docker Compose isolado, testar API, verificar dois pacientes e dois medicamentos, reiniciar API e confirmar ausencia de duplicatas. Remove apenas os volumes temporarios desse projeto.

Apenas se **os tres jobs** aprovarem, o job matricial `images` e executado em push na `develop` ou `main`. Ele gera e envia para GHCR:

| Componente | Imagem/tag |
|---|---|
| API | `ghcr.io/rafaelrocha011/medical-prescription-api:sha-<SHA>` |
| Migrations | `ghcr.io/rafaelrocha011/medical-prescription-migrate:sha-<SHA>` |
| Angular/Nginx | `ghcr.io/rafaelrocha011/medical-prescription-frontend:sha-<SHA>` |

`<SHA>` e o SHA completo do commit executado, inclusive o commit de merge na `main`. **Build e push para GHCR nao sao deploy.**

## Instalar em maquina limpa

Consulte o [README principal](../README.md). O caminho recomendado requer Git, Docker Desktop/Engine e Compose v2, mas nao requer .NET, Node, MySQL ou Angular instalados fora dos contêineres.

```powershell
git clone --branch main https://github.com/rafaelrocha011/medical-prescription.git
cd medical-prescription
Copy-Item .env.example .env
notepad .env
# Defina MYSQL_ROOT_PASSWORD e MYSQL_PASSWORD antes de continuar.
docker compose up -d --build
docker compose ps -a
```

`migrate` deve sair com codigo `0`; `mysql`, `api` e `frontend` devem ficar saudaveis. Confira `http://localhost:4200/prescricoes`. A API inicia em `Development` no Compose local, gerando dados ficticios de pacientes e medicamentos quando as tabelas estao vazias.

Para testar uma instalacao isolada sem tocar no MySQL existente:

```powershell
./scripts/validar-primeira-instalacao.ps1
```

**Nunca execute `docker compose down -v` no projeto de desenvolvimento se quiser preservar o banco.**

## Deploy manual de homologacao (CD)

Arquivo: `.github/workflows/deploy.yml`. A execucao e **manual**, e nao acontece automaticamente na entrega.

- Use Actions > CD - Deploy manual em homologacao > Run workflow.
- Selecione `main` ou `develop` e informe o **SHA completo** de um commit ancestral da branch selecionada, cujas tres imagens ja tenham sido publicadas pelo CI.
- O job so pode executar em runner proprio Linux com labels `self-hosted`, `linux`, `x64` e `medicalprescription-deploy`.
- Configure o GitHub Environment `staging` com os Secrets `STAGING_MYSQL_ROOT_PASSWORD` e `STAGING_MYSQL_PASSWORD`, e Variables `STAGING_MYSQL_USER` e `STAGING_MYSQL_DATABASE`; as portas sao opcionais.
- O runner precisa ter Docker Engine e Compose, permissao para ler as imagens no GHCR e acesso a rede necessaria.

O job usa `docker-compose.yml` + `docker-compose.deploy.yml` para baixar e iniciar imagens com tag imutavel por commit. O projeto Compose de homologacao usa nome fixo `medicalprescription-staging`, mantendo o volume MySQL entre execucoes. Nao execute `down -v`.

**Limitacoes:** esse CD demonstra homologacao, nao implantacao clinica ou producao. Nao inclui HTTPS publico, autenticacao clinica, assinatura digital, backups ou certificacao SBIS.
