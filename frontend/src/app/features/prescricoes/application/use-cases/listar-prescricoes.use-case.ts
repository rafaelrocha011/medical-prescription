import { Injectable, inject } from '@angular/core';

import {
  PRESCRICOES_CONSULTA
} from '../ports/prescricoes-consulta.port';

@Injectable({ providedIn: 'root' })
export class ListarPrescricoesUseCase {
  private readonly repository = inject(PRESCRICOES_CONSULTA);

  executar(pagina = 1, tamanhoPagina = 10) {
    if (pagina < 1) {
      throw new Error('A pagina deve ser maior que zero.');
    }

    if (tamanhoPagina < 1 || tamanhoPagina > 100) {
      throw new Error('Tamanho de pagina invalido.');
    }

    return this.repository.listar(pagina, tamanhoPagina);
  }
}
