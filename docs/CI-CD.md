# CI/CD - MedicalPrescription

## 1. Integracao continua

Arquivo `.github/workflows/ci.yml`:

- Pull requests para `develop`: compila e testa .NET 9 + Angular 21; executa testes de integracao com Testcontainers MySQL no runner Linux do GitHub.
- Push em `develop`: repete as validacoes; publica artefatos `api-net9`, `angular21-dist` e resultados de testes; so entao publica as imagens no GHCR.
- Imagens geradas: `ghcr.io/OWNER/medical-prescription-{api,migrate,frontend}:sha-COMMIT` (substituir OWNER e COMMIT).
- O build Docker so executa se ambos os jobs de verificacao forem aprovados.

As imagens sao construidas com as versoes de Dockerfile que ja funcionaram localmente. Os workflows nao usam senhas do MySQL para testar: os testes de integracao usam MySQL efemero via Testcontainers.

## 2. Entrega continua (deploy controlado)

Arquivo `.github/workflows/deploy.yml`:

- Disparo manual no menu **Actions > CD - Deploy manual em homologacao > Run workflow**, selecionando `develop`.
- Informar o **SHA completo** de um commit da `develop` cujo CI publicou as tres imagens; use o SHA exibido na execucao do CI.
- O job fica reservado para um runner proprio Linux que possua os rotulos `self-hosted`, `linux`, `x64`, `medicalprescription-deploy`.
- Necessario instalar Docker Engine e Compose nesse runner, e ter acesso ao GHCR.
- Configure o ambiente `staging` em Settings > Environments. Recomenda-se aprovacoes obrigatorias (required reviewers), se disponiveis no seu plano/repositorio.
- No ambiente `staging`, configure **Secrets**: `STAGING_MYSQL_ROOT_PASSWORD` e `STAGING_MYSQL_PASSWORD`.
- No ambiente `staging`, configure **Variables**: `STAGING_MYSQL_USER`, `STAGING_MYSQL_DATABASE`; opcionalmente `STAGING_MYSQL_PORT`, `STAGING_API_PORT` e `STAGING_FRONTEND_PORT`.
- O `GITHUB_TOKEN` precisa ter permissao `packages: read` para baixar imagens privadas. Caso a organizacao use outras restricoes de pacote, permita acesso do repositorio ao pacote no GHCR.

O projeto Compose de homologacao usa o nome fixo `medicalprescription-staging`. Isso estabiliza o nome do volume do MySQL entre execucoes. Nao execute `docker compose down -v`; o comando remove o volume persistente.

O `docker-compose.deploy.yml` apenas substitui a origem das imagens de `migrate`, `api` e `frontend`; as dependencias e healthchecks continuam definidos no Compose base.

**Advertencia:** este deploy e uma demonstracao de homologacao, nao um ambiente clinico de producao. Nao inclui HTTPS publico, autenticacao clinica, politicas de backup, monitoramento e plano de recuperacao de desastres.

## 3. Validacao antes de abrir PR

Com o Docker Compose local da Etapa 52 instalado e `.env` configurado:

```powershell
cd C:\Projetos\prescricao-medica
$env:REGISTRY_NAMESPACE = 'ghcr.io/rafaelrocha011/medical-prescription'
$env:IMAGE_TAG = 'sha-0000000000000000000000000000000000000000'
docker compose -f docker-compose.yml -f docker-compose.deploy.yml config --quiet
Remove-Item Env:REGISTRY_NAMESPACE
Remove-Item Env:IMAGE_TAG
```

Esse teste valida o YAML de Compose; **nao faz deploy nem exige que as imagens existam**. O GitHub Actions devera validar os arquivos de workflow apos o PR. O `README.md` principal deve ser atualizado depois da publicacao do pipeline, indicando a existencia dos fluxos CI/CD.

## 4. Como verificar

1. Abra Pull Request `feature/docker-ci-cd` para `develop`.
2. Em Actions, confira `Build e testes .NET 9` e `Build e testes Angular 21`.
3. Apos merge, confira os tres builds/push para GHCR e os artefatos no workflow.
4. Configure runner e ambiente `staging` antes de usar o workflow de deploy; ate la, ele permanecera apenas preparado.
