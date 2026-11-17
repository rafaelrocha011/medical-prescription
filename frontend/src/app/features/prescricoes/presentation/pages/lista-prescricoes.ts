import { RouterLink } from '@angular/router';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';

import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { ListarPrescricoesUseCase } from '../../application/use-cases/listar-prescricoes.use-case';
import {
  PrescricaoResumo,
  TipoPrescricao
} from '../../domain/models/prescricao.model';

@Component({
  selector: 'app-lista-prescricoes',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './lista-prescricoes.html',
  styleUrl: './lista-prescricoes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListaPrescricoes {
  private readonly useCase = inject(ListarPrescricoesUseCase);
  private readonly destroyRef = inject(DestroyRef);

  readonly prescricoes = signal<PrescricaoResumo[]>([]);
  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly pagina = signal(1);
  readonly totalPaginas = signal(0);
  readonly totalRegistros = signal(0);
  readonly tamanhoPagina = 10;

  readonly podeVoltar = computed(
    () => this.pagina() > 1 && !this.carregando()
  );

  readonly podeAvancar = computed(
    () => this.pagina() < this.totalPaginas() && !this.carregando()
  );

  readonly tipoEstruturada = TipoPrescricao.Estruturada;

  constructor() {
    this.carregar(1);
  }

  carregar(pagina: number): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.useCase
      .executar(pagina, this.tamanhoPagina)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.carregando.set(false))
      )
      .subscribe({
        next: resultado => {
          this.prescricoes.set(resultado.itens);
          this.pagina.set(resultado.pagina);
          this.totalPaginas.set(resultado.totalPaginas);
          this.totalRegistros.set(resultado.totalRegistros);
        },
        error: erro => {
          console.error('Falha ao carregar prescricoes:', erro);
          this.erro.set(
            'Nao foi possivel carregar as prescricoes. Verifique a API.'
          );
        }
      });
  }

  atualizar(): void {
    this.carregar(this.pagina());
  }

  anterior(): void {
    if (this.podeVoltar()) {
      this.carregar(this.pagina() - 1);
    }
  }

  proxima(): void {
    if (this.podeAvancar()) {
      this.carregar(this.pagina() + 1);
    }
  }
}
