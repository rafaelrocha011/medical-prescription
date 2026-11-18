import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { PrescricoesEscritaPort } from '../../application/ports/prescricoes-escrita.port';
import { PrescricaoAtualizacaoPort } from '../../application/ports/prescricao-atualizacao.port';
import { PrescricaoExclusaoPort } from '../../application/ports/prescricao-exclusao.port';
import { AtualizarPrescricaoDto } from '../../application/dtos/atualizar-prescricao.dto';
import { CriarPrescricaoDto, PrescricaoCriadaDto } from '../../application/dtos/criar-prescricao.dto';

@Injectable()
export class PrescricaoWriteHttpRepository
  implements PrescricoesEscritaPort, PrescricaoAtualizacaoPort, PrescricaoExclusaoPort {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/prescricoes';

  criar(request: CriarPrescricaoDto) {
    return this.http.post<PrescricaoCriadaDto>(this.endpoint, request);
  }

  atualizar(id: string, request: AtualizarPrescricaoDto) {
    return this.http.put<void>(
      `${this.endpoint}/${encodeURIComponent(id)}`,
      request
    );
  }

  excluir(id: string) {
    return this.http.delete<void>(
      `${this.endpoint}/${encodeURIComponent(id)}`
    );
  }
}
