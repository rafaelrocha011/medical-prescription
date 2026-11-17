import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';

import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { ObterPrescricaoUseCase } from '../../application/use-cases/obter-prescricao.use-case';
import { ExcluirPrescricaoUseCase } from '../../application/use-cases/excluir-prescricao.use-case';
import { PrescricaoDetalhe } from '../../domain/models/prescricao-detalhe.model';
import { TipoPrescricao } from '../../domain/models/prescricao.model';

@Component({
  selector: 'app-detalhe-prescricao',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './detalhe-prescricao.html',
  styleUrl: './detalhe-prescricao.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DetalhePrescricao {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly obterUseCase = inject(ObterPrescricaoUseCase);
  private readonly excluirUseCase = inject(ExcluirPrescricaoUseCase);

  readonly prescricao = signal<PrescricaoDetalhe | null>(null);
  readonly carregando = signal(true);
  readonly excluindo = signal(false);
  readonly confirmandoExclusao = signal(false);
  readonly erro = signal<string | null>(null);

  readonly tipoEstruturada = TipoPrescricao.Estruturada;
  readonly id = this.route.snapshot.paramMap.get('id');

  constructor() {
    if (!this.id) {
      this.erro.set('Identificador inválido.');
      this.carregando.set(false);
      return;
    }

    this.obterUseCase.executar(this.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: resultado => {
          this.prescricao.set(resultado);
          this.carregando.set(false);
        },
        error: (erro: HttpErrorResponse) => {
          this.erro.set(
            erro.status === 404
              ? 'Prescrição não encontrada.'
              : 'Não foi possível consultar a prescrição.'
          );
          this.carregando.set(false);
        }
      });
  }

  solicitarExclusao(): void {
    this.confirmandoExclusao.set(true);
  }

  cancelarExclusao(): void {
    this.confirmandoExclusao.set(false);
  }

  excluir(): void {
    if (!this.id || this.excluindo() || !this.confirmandoExclusao()) {
      return;
    }

    this.excluindo.set(true);
    this.erro.set(null);

    this.excluirUseCase.executar(this.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.excluindo.set(false))
      )
      .subscribe({
        next: () => {
          void this.router.navigate(['/prescricoes']);
        },
        error: (erro: HttpErrorResponse) => {
          this.erro.set(
            typeof erro.error?.detail === 'string'
              ? erro.error.detail
              : 'Não foi possível excluir a prescrição.'
          );
        }
      });
  }
}