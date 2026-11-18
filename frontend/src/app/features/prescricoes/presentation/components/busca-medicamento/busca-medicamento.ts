import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, debounceTime, distinctUntilChanged, finalize, map, of, switchMap } from 'rxjs';

import { CATALOGOS_PORT } from '../../../../catalogos/application/ports/catalogos.port';
import { Medicamento } from '../../../../catalogos/domain/models/catalogo.model';

@Component({
  selector: 'app-busca-medicamento',
  standalone: true,
  templateUrl: './busca-medicamento.html',
  styleUrl: './busca-medicamento.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BuscaMedicamento {
  private readonly catalogos = inject(CATALOGOS_PORT);
  private readonly destroyRef = inject(DestroyRef);
  private readonly termos = new Subject<string>();

  readonly indice = input.required<number>();
  readonly nomeSelecionado = input<string | null>(null);
  readonly selecionado = output<Medicamento>();
  readonly manual = output<void>();

  readonly termo = signal('');
  readonly resultados = signal<Medicamento[]>([]);
  readonly carregando = signal(false);
  readonly pesquisou = signal(false);
  readonly erro = signal<string | null>(null);

  constructor() {
    this.termos.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(busca => {
        if (busca.length < 2) {
          this.carregando.set(false);
          this.erro.set(null);
          return of({ itens: [] as Medicamento[], pesquisou: false });
        }

        this.carregando.set(true);
        this.erro.set(null);
        return this.catalogos.listarMedicamentos(busca).pipe(
          map(itens => ({ itens, pesquisou: true })),
          catchError(() => {
            this.erro.set('Não foi possível pesquisar medicamentos.');
            return of({ itens: [] as Medicamento[], pesquisou: true });
          }),
          finalize(() => this.carregando.set(false))
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(resultado => {
      this.resultados.set(resultado.itens);
      this.pesquisou.set(resultado.pesquisou);
    });
  }

  pesquisar(valor: string): void {
    const busca = valor.slice(0, 100);
    this.termo.set(busca);
    this.termos.next(busca.trim());
    this.resultados.set([]);
    this.pesquisou.set(false);
  }

  escolher(medicamento: Medicamento): void {
    this.selecionado.emit(medicamento);
    this.termo.set('');
    this.resultados.set([]);
    this.pesquisou.set(false);
  }

  usarManual(): void {
    this.manual.emit();
    this.termo.set('');
    this.resultados.set([]);
    this.pesquisou.set(false);
  }
}
