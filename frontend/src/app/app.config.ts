import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { CATALOGOS_PORT } from './features/catalogos/application/ports/catalogos.port';
import { CatalogosHttpRepository } from './features/catalogos/infrastructure/http/catalogos-http.repository';
import { PRESCRICOES_CONSULTA } from './features/prescricoes/application/ports/prescricoes-consulta.port';
import { PRESCRICAO_DETALHE_PORT } from './features/prescricoes/application/ports/prescricao-detalhe.port';
import { PRESCRICOES_ESCRITA_PORT } from './features/prescricoes/application/ports/prescricoes-escrita.port';
import { PRESCRICAO_ATUALIZACAO_PORT } from './features/prescricoes/application/ports/prescricao-atualizacao.port';
import { PRESCRICAO_EXCLUSAO_PORT } from './features/prescricoes/application/ports/prescricao-exclusao.port';
import { PrescricaoReadHttpRepository } from './features/prescricoes/infrastructure/http/prescricao-read-http.repository';
import { PrescricaoWriteHttpRepository } from './features/prescricoes/infrastructure/http/prescricao-write-http.repository';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    { provide: CATALOGOS_PORT, useClass: CatalogosHttpRepository },
    PrescricaoReadHttpRepository,
    PrescricaoWriteHttpRepository,
    { provide: PRESCRICOES_CONSULTA, useExisting: PrescricaoReadHttpRepository },
    { provide: PRESCRICAO_DETALHE_PORT, useExisting: PrescricaoReadHttpRepository },
    { provide: PRESCRICOES_ESCRITA_PORT, useExisting: PrescricaoWriteHttpRepository },
    { provide: PRESCRICAO_ATUALIZACAO_PORT, useExisting: PrescricaoWriteHttpRepository },
    { provide: PRESCRICAO_EXCLUSAO_PORT, useExisting: PrescricaoWriteHttpRepository }
  ]
};
