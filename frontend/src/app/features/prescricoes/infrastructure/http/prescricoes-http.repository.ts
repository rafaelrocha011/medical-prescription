import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { PrescricoesConsultaPort } from '../../application/ports/prescricoes-consulta.port';
import {
  PrescricaoResumo,
  ResultadoPaginado
} from '../../domain/models/prescricao.model';

@Injectable()
export class PrescricoesHttpRepository implements PrescricoesConsultaPort {
  private readonly http = inject(HttpClient);

  listar(pagina: number, tamanhoPagina: number) {
    const params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanhoPagina', tamanhoPagina);

    return this.http.get<ResultadoPaginado<PrescricaoResumo>>(
      '/api/prescricoes',
      { params }
    );
  }
}
