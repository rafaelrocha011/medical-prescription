import { Injectable, inject } from '@angular/core';

import { CriarPrescricaoDto } from '../dtos/criar-prescricao.dto';
import { PRESCRICOES_ESCRITA_PORT } from '../ports/prescricoes-escrita.port';

@Injectable({ providedIn: 'root' })
export class CriarPrescricaoUseCase {
  private readonly repository = inject(PRESCRICOES_ESCRITA_PORT);

  executar(request: CriarPrescricaoDto) {
    if (!request.pacienteId) {
      throw new Error('Selecione um paciente.');
    }

    if (request.tipo === 1 && !request.textoLivre?.trim()) {
      throw new Error('Informe o texto da prescrição.');
    }

    if (request.tipo === 2 && request.itens.length === 0) {
      throw new Error('Adicione ao menos um medicamento.');
    }

    return this.repository.criar(request);
  }
}