import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { PrescricaoExclusaoPort } from '../../application/ports/prescricao-exclusao.port';

@Injectable()
export class PrescricaoExclusaoHttpRepository
  implements PrescricaoExclusaoPort {

  private readonly http = inject(HttpClient);

  excluir(id: string) {
    return this.http.delete<void>(
      `/api/prescricoes/${encodeURIComponent(id)}`
    );
  }
}