import { PRESCRICAO_ATUALIZACAO_PORT } from './features/prescricoes/application/ports/prescricao-atualizacao.port';
import { PrescricaoAtualizacaoHttpRepository } from './features/prescricoes/infrastructure/http/prescricao-atualizacao-http.repository';
import { PRESCRICAO_DETALHE_PORT } from './features/prescricoes/application/ports/prescricao-detalhe.port';
import { PRESCRICAO_EXCLUSAO_PORT } from './features/prescricoes/application/ports/prescricao-exclusao.port';
import { PrescricaoDetalheHttpRepository } from './features/prescricoes/infrastructure/http/prescricao-detalhe-http.repository';
import { PrescricaoExclusaoHttpRepository } from './features/prescricoes/infrastructure/http/prescricao-exclusao-http.repository';
import { CATALOGOS_PORT } from './features/catalogos/application/ports/catalogos.port';
import { CatalogosHttpRepository } from './features/catalogos/infrastructure/http/catalogos-http.repository';
import { PRESCRICOES_ESCRITA_PORT } from './features/prescricoes/application/ports/prescricoes-escrita.port';
import { PrescricoesEscritaHttpRepository } from './features/prescricoes/infrastructure/http/prescricoes-escrita-http.repository';
import { provideHttpClient } from '@angular/common/http';
import { PRESCRICOES_CONSULTA } from './features/prescricoes/application/ports/prescricoes-consulta.port';
import { PrescricoesHttpRepository } from './features/prescricoes/infrastructure/http/prescricoes-http.repository';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    { provide: PRESCRICAO_ATUALIZACAO_PORT, useClass: PrescricaoAtualizacaoHttpRepository },
    { provide: PRESCRICAO_DETALHE_PORT, useClass: PrescricaoDetalheHttpRepository },
    { provide: PRESCRICAO_EXCLUSAO_PORT, useClass: PrescricaoExclusaoHttpRepository },
    { provide: CATALOGOS_PORT, useClass: CatalogosHttpRepository },
    { provide: PRESCRICOES_ESCRITA_PORT, useClass: PrescricoesEscritaHttpRepository },provideHttpClient(), { provide: PRESCRICOES_CONSULTA, useClass: PrescricoesHttpRepository }, 
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes)
  ]
};

