import { Injectable, inject } from '@angular/core';
import { PRESCRICAO_EXCLUSAO_PORT } from '../ports/prescricao-exclusao.port';

@Injectable({ providedIn: 'root' })
export class ExcluirPrescricaoUseCase {
  private readonly repository = inject(PRESCRICAO_EXCLUSAO_PORT);

  executar(id: string) {
    if (!id) {
      throw new Error('Identificador da prescrição é obrigatório.');
    }

    return this.repository.excluir(id);
  }
}