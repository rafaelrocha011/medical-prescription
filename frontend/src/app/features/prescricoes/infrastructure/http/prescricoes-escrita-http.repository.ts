import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import {
  CriarPrescricaoDto,
  PrescricaoCriadaDto
} from '../../application/dtos/criar-prescricao.dto';

import {
  PrescricoesEscritaPort
} from '../../application/ports/prescricoes-escrita.port';

@Injectable()
export class PrescricoesEscritaHttpRepository
  implements PrescricoesEscritaPort {

  private readonly http = inject(HttpClient);

  criar(request: CriarPrescricaoDto) {
    return this.http.post<PrescricaoCriadaDto>(
      '/api/prescricoes',
      request
    );
  }
}