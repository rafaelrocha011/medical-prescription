import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { AtualizarPrescricaoDto } from '../dtos/atualizar-prescricao.dto';

export interface PrescricaoAtualizacaoPort {
  atualizar(
    id: string,
    request: AtualizarPrescricaoDto
  ): Observable<void>;
}

export const PRESCRICAO_ATUALIZACAO_PORT =
  new InjectionToken<PrescricaoAtualizacaoPort>(
    'PRESCRICAO_ATUALIZACAO_PORT'
  );