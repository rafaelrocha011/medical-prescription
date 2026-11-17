import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy, Component, DestroyRef, ElementRef,
  ViewChild, computed, inject, signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ExcluirPrescricaoUseCase } from '../../application/use-cases/excluir-prescricao.use-case';
import { ListarPrescricoesUseCase } from '../../application/use-cases/listar-prescricoes.use-case';
import { ObterPrescricaoUseCase } from '../../application/use-cases/obter-prescricao.use-case';
import { PrescricaoDetalhe } from '../../domain/models/prescricao-detalhe.model';
import { PrescricaoResumo, TipoPrescricao } from '../../domain/models/prescricao.model';

@Component({
  selector: 'app-lista-prescricoes',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './lista-prescricoes.html',
  styleUrl: './lista-prescricoes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListaPrescricoes {
  private readonly listarUseCase = inject(ListarPrescricoesUseCase);
  private readonly obterUseCase = inject(ObterPrescricaoUseCase);
  private readonly excluirUseCase = inject(ExcluirPrescricaoUseCase);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('modalExclusao') private modalExclusao?: ElementRef<HTMLDialogElement>;

  readonly prescricoes = signal<PrescricaoResumo[]>([]);
  readonly carregando = signal(false);
  readonly erro = signal<string | null>(null);
  readonly pagina = signal(1);
  readonly totalPaginas = signal(0);
  readonly totalRegistros = signal(0);
  readonly tamanhoPagina = 10;
  readonly tipoEstruturada = TipoPrescricao.Estruturada;

  readonly idExpandido = signal<string | null>(null);
  readonly detalhe = signal<PrescricaoDetalhe | null>(null);
  readonly carregandoDetalhe = signal(false);
  readonly erroDetalhe = signal<string | null>(null);

  readonly itemParaExcluir = signal<PrescricaoResumo | null>(null);
  readonly excluindo = signal(false);
  readonly erroExclusao = signal<string | null>(null);

  readonly podeVoltar = computed(() => this.pagina() > 1 && !this.carregando());
  readonly podeAvancar = computed(
    () => this.pagina() < this.totalPaginas() && !this.carregando()
  );

  private sequenciaLista = 0;
  private sequenciaDetalhe = 0;

  constructor() {
    this.carregar(1);
  }

  carregar(pagina: number): void {
    const sequencia = ++this.sequenciaLista;
    this.fecharDetalhes();
    this.carregando.set(true);
    this.erro.set(null);

    this.listarUseCase.executar(pagina, this.tamanhoPagina)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          if (sequencia === this.sequenciaLista) this.carregando.set(false);
        })
      )
      .subscribe({
        next: resultado => {
          if (sequencia !== this.sequenciaLista) return;
          this.prescricoes.set(resultado.itens);
          this.pagina.set(resultado.pagina);
          this.totalPaginas.set(resultado.totalPaginas);
          this.totalRegistros.set(resultado.totalRegistros);
        },
        error: erro => {
          if (sequencia !== this.sequenciaLista) return;
          console.error('Falha ao carregar prescricoes:', erro);
          this.prescricoes.set([]);
          this.erro.set('N\u00e3o foi poss\u00edvel carregar as prescri\u00e7\u00f5es. Verifique a API.');
        }
      });
  }

  atualizar(): void {
    this.carregar(this.pagina());
  }

  anterior(): void {
    if (this.podeVoltar()) this.carregar(this.pagina() - 1);
  }

  proxima(): void {
    if (this.podeAvancar()) this.carregar(this.pagina() + 1);
  }

  alternarDetalhes(item: PrescricaoResumo): void {
    if (this.idExpandido() === item.id) {
      this.fecharDetalhes();
      return;
    }

    const sequencia = ++this.sequenciaDetalhe;
    this.idExpandido.set(item.id);
    this.detalhe.set(null);
    this.erroDetalhe.set(null);
    this.carregandoDetalhe.set(true);

    this.obterUseCase.executar(item.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          if (sequencia === this.sequenciaDetalhe) this.carregandoDetalhe.set(false);
        })
      )
      .subscribe({
        next: resultado => {
          if (sequencia !== this.sequenciaDetalhe) return;
          this.detalhe.set(resultado);
        },
        error: (erro: HttpErrorResponse) => {
          if (sequencia !== this.sequenciaDetalhe) return;
          this.erroDetalhe.set(erro.status === 404
            ? 'Prescri\u00e7\u00e3o n\u00e3o encontrada.'
            : 'N\u00e3o foi poss\u00edvel carregar os detalhes.');
        }
      });
  }

  private fecharDetalhes(): void {
    ++this.sequenciaDetalhe;
    this.idExpandido.set(null);
    this.detalhe.set(null);
    this.carregandoDetalhe.set(false);
    this.erroDetalhe.set(null);
  }

  solicitarExclusao(item: PrescricaoResumo): void {
    if (this.excluindo()) return;
    this.itemParaExcluir.set(item);
    this.erroExclusao.set(null);
    this.modalExclusao?.nativeElement.showModal();
  }

  cancelarExclusao(): void {
    if (this.excluindo()) return;
    this.modalExclusao?.nativeElement.close();
    this.limparConfirmacao();
  }

  aoCancelarDialogo(evento: Event): void {
    if (this.excluindo()) evento.preventDefault();
  }

  aoFecharDialogo(): void {
    if (!this.excluindo()) this.limparConfirmacao();
  }

  confirmarExclusao(): void {
    const item = this.itemParaExcluir();
    if (!item || this.excluindo()) return;

    this.excluindo.set(true);
    this.erroExclusao.set(null);

    this.excluirUseCase.executar(item.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.excluindo.set(false))
      )
      .subscribe({
        next: () => {
          this.modalExclusao?.nativeElement.close();
          this.limparConfirmacao();
          const paginaDestino = this.prescricoes().length === 1 && this.pagina() > 1
            ? this.pagina() - 1
            : this.pagina();
          this.carregar(paginaDestino);
        },
        error: (erro: HttpErrorResponse) => {
          this.erroExclusao.set(
            typeof erro.error?.detail === 'string'
              ? erro.error.detail
              : 'N\u00e3o foi poss\u00edvel excluir a prescri\u00e7\u00e3o.'
          );
        }
      });
  }

  private limparConfirmacao(): void {
    this.itemParaExcluir.set(null);
    this.erroExclusao.set(null);
  }
}