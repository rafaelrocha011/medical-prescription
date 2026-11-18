import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

import {
  PrescricaoResumo,
  ResultadoPaginado
} from '../../domain/models/prescricao.model';

export interface PrescricoesConsultaPort {
  listar(
    pagina: number,
    tamanhoPagina: number
  ): Observable<ResultadoPaginado<PrescricaoResumo>>;
}

export const PRESCRICOES_CONSULTA =
  new InjectionToken<PrescricoesConsultaPort>(
    'PRESCRICOES_CONSULTA'
  );
