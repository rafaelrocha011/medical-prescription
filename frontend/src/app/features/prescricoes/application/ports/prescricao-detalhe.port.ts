import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { PrescricaoDetalhe } from '../../domain/models/prescricao-detalhe.model';

export interface PrescricaoDetalhePort {
  obterPorId(id: string): Observable<PrescricaoDetalhe>;
}

export const PRESCRICAO_DETALHE_PORT =
  new InjectionToken<PrescricaoDetalhePort>('PRESCRICAO_DETALHE_PORT');