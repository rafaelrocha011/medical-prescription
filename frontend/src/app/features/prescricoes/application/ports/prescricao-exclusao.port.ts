import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

export interface PrescricaoExclusaoPort {
  excluir(id: string): Observable<void>;
}

export const PRESCRICAO_EXCLUSAO_PORT =
  new InjectionToken<PrescricaoExclusaoPort>('PRESCRICAO_EXCLUSAO_PORT');