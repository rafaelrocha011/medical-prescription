import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { ExcluirPrescricaoUseCase } from '../../application/use-cases/excluir-prescricao.use-case';
import { ListarPrescricoesUseCase } from '../../application/use-cases/listar-prescricoes.use-case';
import { ObterPrescricaoUseCase } from '../../application/use-cases/obter-prescricao.use-case';
import { TipoPrescricao } from '../../domain/models/prescricao.model';
import { ListaPrescricoes } from './lista-prescricoes';

const resumo = {
  id: 'prescricao-teste-1',
  pacienteId: 'paciente-teste-1',
  nomePaciente: 'Paciente Nome Completo Teste',
  tipo: TipoPrescricao.NaoEstruturada,
  criadaEmUtc: '2026-11-17T15:00:00Z',
  quantidadeMedicamentos: 0
};

const detalhe = {
  ...resumo,
  textoLivre: 'Prescricao de teste',
  itens: []
};

describe('ListaPrescricoes', () => {
  const listar = { executar: vi.fn() };
  const obter = { executar: vi.fn() };
  const excluir = { executar: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    listar.executar.mockReturnValue(of({
      itens: [resumo], pagina: 1, tamanhoPagina: 10,
      totalRegistros: 1, totalPaginas: 1
    }));
    obter.executar.mockReturnValue(of(detalhe));
    excluir.executar.mockReturnValue(of(undefined));

    await TestBed.configureTestingModule({
      imports: [ListaPrescricoes],
      providers: [
        provideRouter([]),
        { provide: ListarPrescricoesUseCase, useValue: listar },
        { provide: ObterPrescricaoUseCase, useValue: obter },
        { provide: ExcluirPrescricaoUseCase, useValue: excluir }
      ]
    }).compileComponents();
  });

  it('exibe as acoes por registro', () => {
    const fixture = TestBed.createComponent(ListaPrescricoes);
    fixture.detectChanges();
    const elemento = fixture.nativeElement as HTMLElement;
    expect(elemento.querySelector('[data-testid="expand-details"]')).toBeTruthy();
    expect(elemento.querySelector('[data-testid="delete"]')).toBeTruthy();
    expect(elemento.querySelector('a[href*="editar"]')).toBeTruthy();
  });

  it('mostra nome do paciente, sem GUIDs na linha', () => {

    const fixture = TestBed.createComponent(ListaPrescricoes);

    fixture.detectChanges();

    const elemento = fixture.nativeElement as HTMLElement;

    const linha = elemento.querySelector('tbody tr');

    expect(linha?.textContent).toContain('Paciente Nome Completo Teste');

    expect(linha?.textContent).not.toContain('paciente-teste-1');

    expect(linha?.textContent).not.toContain('prescricao-teste-1');

  });


  it('abre detalhes diretamente abaixo do registro', () => {
    const fixture = TestBed.createComponent(ListaPrescricoes);
    fixture.detectChanges();
    const elemento = fixture.nativeElement as HTMLElement;
    (elemento.querySelector('[data-testid="expand-details"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(obter.executar).toHaveBeenCalledWith(resumo.id);
    expect(elemento.querySelector('.linha-expandida')?.textContent).toContain('Prescricao de teste');
  });

  it('cancelar confirmacao nao deve excluir o registro', () => {
    const fixture = TestBed.createComponent(ListaPrescricoes);
    fixture.detectChanges();
    const elemento = fixture.nativeElement as HTMLElement;
    const dialogo = elemento.querySelector('dialog') as HTMLDialogElement;

    // jsdom pode nao implementar os metodos nativos de HTMLDialogElement.
    dialogo.showModal = () => dialogo.setAttribute('open', '');
    dialogo.close = () => dialogo.removeAttribute('open');

    (elemento.querySelector('[data-testid="delete"]') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(dialogo.hasAttribute('open')).toBe(true);

    fixture.componentInstance.cancelarExclusao();
    fixture.detectChanges();
    expect(dialogo.hasAttribute('open')).toBe(false);
    expect(excluir.executar).not.toHaveBeenCalled();
  });
});