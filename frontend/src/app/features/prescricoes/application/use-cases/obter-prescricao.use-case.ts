import { Injectable, inject } from '@angular/core';
import { PRESCRICAO_DETALHE_PORT } from '../ports/prescricao-detalhe.port';

@Injectable({ providedIn: 'root' })
export class ObterPrescricaoUseCase {
  private readonly repository = inject(PRESCRICAO_DETALHE_PORT);

  executar(id: string) {
    if (!id) {
      throw new Error('Identificador da prescrição é obrigatório.');
    }

    return this.repository.obterPorId(id);
  }
}