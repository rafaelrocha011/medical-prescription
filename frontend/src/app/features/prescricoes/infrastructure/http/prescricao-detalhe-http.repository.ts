import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { PrescricaoDetalhePort } from '../../application/ports/prescricao-detalhe.port';
import { PrescricaoDetalhe } from '../../domain/models/prescricao-detalhe.model';

@Injectable()
export class PrescricaoDetalheHttpRepository
  implements PrescricaoDetalhePort {

  private readonly http = inject(HttpClient);

  obterPorId(id: string) {
    return this.http.get<PrescricaoDetalhe>(
      `/api/prescricoes/${encodeURIComponent(id)}`
    );
  }
}