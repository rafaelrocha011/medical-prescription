import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { PrescricoesConsultaPort } from '../../application/ports/prescricoes-consulta.port';
import { PrescricaoDetalhePort } from '../../application/ports/prescricao-detalhe.port';
import { PrescricaoDetalhe } from '../../domain/models/prescricao-detalhe.model';
import { PrescricaoResumo, ResultadoPaginado } from '../../domain/models/prescricao.model';

@Injectable()
export class PrescricaoReadHttpRepository
  implements PrescricoesConsultaPort, PrescricaoDetalhePort {
  private readonly http = inject(HttpClient);
  private readonly endpoint = '/api/prescricoes';

  listar(pagina: number, tamanhoPagina: number) {
    const params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina);

    return this.http.get<ResultadoPaginado<PrescricaoResumo>>(
      this.endpoint,
      { params }
    );
  }

  obterPorId(id: string) {
    return this.http.get<PrescricaoDetalhe>(
      `${this.endpoint}/${encodeURIComponent(id)}`
    );
  }
}
