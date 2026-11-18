import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

import {
  CriarPrescricaoDto,
  PrescricaoCriadaDto
} from '../dtos/criar-prescricao.dto';

export interface PrescricoesEscritaPort {
  criar(request: CriarPrescricaoDto): Observable<PrescricaoCriadaDto>;
}

export const PRESCRICOES_ESCRITA_PORT =
  new InjectionToken<PrescricoesEscritaPort>(
    'PRESCRICOES_ESCRITA_PORT'
  );