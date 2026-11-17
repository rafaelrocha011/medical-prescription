import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { AtualizarPrescricaoDto } from '../../application/dtos/atualizar-prescricao.dto';
import { PrescricaoAtualizacaoPort } from '../../application/ports/prescricao-atualizacao.port';

@Injectable()
export class PrescricaoAtualizacaoHttpRepository
  implements PrescricaoAtualizacaoPort {

  private readonly http = inject(HttpClient);

  atualizar(id: string, request: AtualizarPrescricaoDto) {
    return this.http.put<void>(
      `/api/prescricoes/${encodeURIComponent(id)}`,
      request
    );
  }
}