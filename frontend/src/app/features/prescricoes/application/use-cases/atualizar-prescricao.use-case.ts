import { Injectable, inject } from '@angular/core';
import { AtualizarPrescricaoDto } from '../dtos/atualizar-prescricao.dto';
import { PRESCRICAO_ATUALIZACAO_PORT } from '../ports/prescricao-atualizacao.port';

@Injectable({ providedIn: 'root' })
export class AtualizarPrescricaoUseCase {
  private readonly repository = inject(PRESCRICAO_ATUALIZACAO_PORT);

  executar(id: string, request: AtualizarPrescricaoDto) {
    if (!id?.trim()) {
      throw new Error('Identificador obrigatorio.');
    }

    return this.repository.atualizar(id, request);
  }
}