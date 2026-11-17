import type { Observable } from 'rxjs';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, of } from 'rxjs';

import { CATALOGOS_PORT } from '../../../catalogos/application/ports/catalogos.port';
import { Medicamento, Paciente } from '../../../catalogos/domain/models/catalogo.model';
import { TipoPrescricao } from '../../domain/models/prescricao.model';
import { PrescricaoDetalhe } from '../../domain/models/prescricao-detalhe.model';
import { CriarPrescricaoDto } from '../../application/dtos/criar-prescricao.dto';
import { CriarPrescricaoUseCase } from '../../application/use-cases/criar-prescricao.use-case';
import { AtualizarPrescricaoUseCase } from '../../application/use-cases/atualizar-prescricao.use-case';
import { ObterPrescricaoUseCase } from '../../application/use-cases/obter-prescricao.use-case';
import { BuscaMedicamento } from '../components/busca-medicamento/busca-medicamento';
import { dosagemValidator } from '../validators/dosagem.validator';
import { dataLocalHoje } from '../utils/data-local';

@Component({
  selector: 'app-formulario-prescricao',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, BuscaMedicamento],
  templateUrl: './formulario-prescricao.html',
  styleUrl: './formulario-prescricao.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormularioPrescricao {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly catalogos = inject(CATALOGOS_PORT);
  private readonly criarUseCase = inject(CriarPrescricaoUseCase);
  private readonly atualizarUseCase = inject(AtualizarPrescricaoUseCase);
  private readonly obterUseCase = inject(ObterPrescricaoUseCase);

  readonly id = this.route.snapshot.paramMap.get('id');
  readonly editando = !!this.id;
  readonly titulo = this.editando
    ? 'Editar prescrição médica'
    : 'Nova prescrição médica';
  readonly rotuloSalvar = this.editando
    ? 'Salvar alterações'
    : 'Salvar prescrição';

  readonly tipos = TipoPrescricao;
  readonly pacientes = signal<Paciente[]>([]);
  readonly carregandoCatalogos = signal(true);
  readonly salvando = signal(false);
  readonly erro = signal<string | null>(null);

  readonly formulario = this.fb.group({
    pacienteId: this.fb.nonNullable.control('', Validators.required),
    tipo: this.fb.nonNullable.control(TipoPrescricao.NaoEstruturada),
    textoLivre: this.fb.nonNullable.control(''),
    itens: this.fb.array<ReturnType<FormularioPrescricao['criarItemForm']>>([])
  });

  get itens() {
    return this.formulario.controls.itens;
  }

  constructor() {
    // O catálogo de medicamentos é pesquisado sob demanda, sem limite local de 50 opções.
    forkJoin({
      pacientes: this.catalogos.listarPacientes(),
      prescricao: this.id
        ? this.obterUseCase.executar(this.id)
        : of<PrescricaoDetalhe | null>(null)
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: dados => {
          this.pacientes.set(dados.pacientes);
          if (dados.prescricao) {
            this.preencherEdicao(dados.prescricao);
          }
          this.carregandoCatalogos.set(false);
        },
        error: (erro: HttpErrorResponse) => {
          this.erro.set(erro.status === 404
            ? 'Prescrição não encontrada.'
            : 'Não foi possível carregar os dados. Verifique a API.');
          this.carregandoCatalogos.set(false);
        }
      });
  }

  private preencherEdicao(prescricao: PrescricaoDetalhe): void {
    this.formulario.patchValue({
      pacienteId: prescricao.pacienteId,
      tipo: prescricao.tipo,
      textoLivre: prescricao.textoLivre ?? ''
    });

    this.itens.clear();
    for (const item of prescricao.itens) {
      const grupo = this.criarItemForm();
      grupo.patchValue({
        medicamentoId: item.medicamentoId ?? '',
        nomeMedicamento: item.nomeMedicamento,
        formaFarmaceutica: item.formaFarmaceutica,
        dose: item.dose,
        unidadeMedida: item.unidadeMedida,
        frequencia: item.frequencia,
        inicioTratamento: item.inicioTratamento.slice(0, 10),
        duracaoTratamentoDias: item.duracaoTratamentoDias,
        usoContinuo: item.usoContinuo,
        viaAdministracao: item.viaAdministracao,
        prazoUtilizacao: item.prazoUtilizacao,
        observacao: item.observacao ?? ''
      });
      this.ajustarDuracao(grupo);
      this.itens.push(grupo);
    }

    this.formulario.controls.pacienteId.disable();
    this.formulario.controls.tipo.disable();
  }

  alterarTipo(): void {
    if (this.editando) return;
    this.erro.set(null);

    if (this.formulario.controls.tipo.value === TipoPrescricao.Estruturada) {
      if (this.itens.length === 0) this.adicionarItem();
    } else {
      this.itens.clear();
    }
  }

  adicionarItem(): void {
    this.itens.push(this.criarItemForm());
  }

  removerItem(indice: number): void {
    if (this.itens.length > 1) this.itens.removeAt(indice);
  }

  selecionarMedicamento(indice: number, medicamento: Medicamento): void {
    this.itens.at(indice).patchValue({
      medicamentoId: medicamento.id,
      nomeMedicamento: medicamento.nome
    });
  }

  usarMedicamentoManual(indice: number): void {
    this.itens.at(indice).patchValue({
      medicamentoId: '',
      nomeMedicamento: ''
    });
  }

  alterarUsoContinuo(indice: number): void {
    this.ajustarDuracao(this.itens.at(indice));
  }

  private ajustarDuracao(grupo: ReturnType<FormularioPrescricao['criarItemForm']>): void {
    const duracao = grupo.controls.duracaoTratamentoDias;
    if (grupo.controls.usoContinuo.value) {
      duracao.setValue(null);
      duracao.disable();
    } else {
      duracao.enable();
      if (duracao.value === null) duracao.setValue(7);
    }
  }

  salvar(): void {
    if (this.salvando() || this.carregandoCatalogos()) return;
    const valor = this.formulario.getRawValue();

    if (!valor.pacienteId) {
      this.erro.set('Selecione um paciente.');
      return;
    }

    if (valor.tipo === TipoPrescricao.NaoEstruturada && !valor.textoLivre.trim()) {
      this.erro.set('Informe o texto da receita.');
      return;
    }

    if (valor.tipo === TipoPrescricao.Estruturada) {
      if (this.itens.length === 0 || this.itens.invalid) {
        this.erro.set('Preencha os campos obrigatórios dos medicamentos e verifique a dosagem.');
        this.itens.markAllAsTouched();
        return;
      }

      const camposObrigatorios = valor.itens.some(item =>
        [item.nomeMedicamento, item.formaFarmaceutica, item.unidadeMedida,
          item.frequencia, item.viaAdministracao, item.prazoUtilizacao]
          .some(campo => !campo.trim())
      );
      if (camposObrigatorios) {
        this.erro.set('Preencha todos os campos obrigatórios dos medicamentos.');
        return;
      }

      if (valor.itens.some(item =>
        !item.usoContinuo &&
        (!item.duracaoTratamentoDias || item.duracaoTratamentoDias <= 0)
      )) {
        this.erro.set('Informe a duração do tratamento.');
        return;
      }
    }

    const request: CriarPrescricaoDto = {
      pacienteId: valor.pacienteId,
      tipo: valor.tipo,
      textoLivre:
        valor.tipo === TipoPrescricao.NaoEstruturada
          ? valor.textoLivre.trim()
          : null,
      itens:
        valor.tipo === TipoPrescricao.Estruturada
          ? valor.itens.map(item => ({
              medicamentoId: item.medicamentoId || null,
              nomeMedicamento: item.nomeMedicamento.trim(),
              formaFarmaceutica: item.formaFarmaceutica.trim(),
              dose: Number(item.dose),
              unidadeMedida: item.unidadeMedida.trim(),
              frequencia: item.frequencia.trim(),
              inicioTratamento: item.inicioTratamento,
              duracaoTratamentoDias: item.usoContinuo
                ? null : Number(item.duracaoTratamentoDias),
              usoContinuo: item.usoContinuo,
              viaAdministracao: item.viaAdministracao.trim(),
              prazoUtilizacao: item.prazoUtilizacao.trim(),
              observacao: item.observacao.trim() || null
            }))
          : []
    };

    this.salvando.set(true);
    this.erro.set(null);

    try {
      const operacao$: Observable<unknown> = this.id
        ? this.atualizarUseCase.executar(this.id, {
            textoLivre: request.textoLivre,
            itens: request.itens
          })
        : this.criarUseCase.executar(request);

      operacao$.pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.salvando.set(false))
      ).subscribe({
        next: () => void this.router.navigate(['/prescricoes']),
        error: (erro: HttpErrorResponse) => {
          this.erro.set(typeof erro.error?.detail === 'string'
            ? erro.error.detail
            : 'Não foi possível salvar a prescrição.');
        }
      });
    } catch (erro) {
      this.salvando.set(false);
      this.erro.set(erro instanceof Error ? erro.message : 'Erro de validação.');
    }
  }

  private criarItemForm() {
    return this.fb.group({
      medicamentoId: this.fb.nonNullable.control(''),
      nomeMedicamento: this.fb.nonNullable.control('', Validators.required),
      formaFarmaceutica: this.fb.nonNullable.control('', Validators.required),
      dose: this.fb.nonNullable.control(500, [Validators.required, dosagemValidator]),
      unidadeMedida: this.fb.nonNullable.control('mg', Validators.required),
      frequencia: this.fb.nonNullable.control('', Validators.required),
      inicioTratamento: this.fb.nonNullable.control(dataLocalHoje(), Validators.required),
      duracaoTratamentoDias: this.fb.control<number | null>(7, Validators.min(1)),
      usoContinuo: this.fb.nonNullable.control(false),
      viaAdministracao: this.fb.nonNullable.control('Oral', Validators.required),
      prazoUtilizacao: this.fb.nonNullable.control('', Validators.required),
      observacao: this.fb.nonNullable.control('')
    });
  }
}
