# Frontend — MedicalPrescription

Interface Angular 21 para gestão demonstrativa de prescrições médicas.

## Organização

Os componentes standalone ficam em `src/app`. Cada funcionalidade pode conter:

- `domain/`: modelos e tipos TypeScript.
- `application/`: DTOs, Use Cases e contratos (ports).
- `infrastructure/`: adaptadores HTTP que implementam os contratos.
- `presentation/`: páginas, componentes, validadores e utilitários de interface.

Os repositórios HTTP de prescrições são organizados em **leitura** e **escrita**. A injeção por `InjectionToken` permite desacoplar Use Cases do `HttpClient`.

## Executar no Docker Desktop (Windows)

Na raiz do repositório:

```powershell
docker run --rm -it -p 4200:4200 -v "${PWD}\frontend:/workspace/frontend" -w /workspace/frontend node:22 sh -c "npm ci && npx ng serve --host 0.0.0.0 --poll 1000 --proxy-config src/proxy.conf.json"
```

Acesse `http://localhost:4200/prescricoes`. A API .NET deve estar disponível em `http://localhost:5080`. O proxy usa `host.docker.internal` para alcançá-la a partir do contêiner.

## Build e testes

Na pasta `frontend/`, se Node.js/npm estiverem instalados localmente:

```powershell
npm ci
npm run build
npm test -- --watch=false
```

Ou execute tudo em Docker, a partir da raiz:

```powershell
docker run --rm -v "${PWD}\frontend:/workspace/frontend" -w /workspace/frontend node:22 sh -c "npm ci && npm run build && npm test -- --watch=false"
```

## Comportamento da interface

- Menu lateral com acesso a **Prescrições**.
- Listagem com paginação e detalhes expansíveis.
- Edição por registro; exclusão mediante modal de confirmação.
- Cadastro estruturado ou não estruturado.
- Pesquisa por nome no catálogo de medicamentos e opção de inclusão manual.
- Validação de dosagem, campos obrigatórios e datas.

## Observações

Não incluir `node_modules/`, `.angular/`, `dist/` ou resultados de testes no Git. O `.gitignore` é centralizado na raiz do repositório.
