import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { CriarPrescricaoDto } from '../../application/dtos/criar-prescricao.dto';
import { PRESCRICOES_CONSULTA } from '../../application/ports/prescricoes-consulta.port';
import { PRESCRICAO_DETALHE_PORT } from '../../application/ports/prescricao-detalhe.port';
import { PRESCRICOES_ESCRITA_PORT } from '../../application/ports/prescricoes-escrita.port';
import { PRESCRICAO_ATUALIZACAO_PORT } from '../../application/ports/prescricao-atualizacao.port';
import { PRESCRICAO_EXCLUSAO_PORT } from '../../application/ports/prescricao-exclusao.port';
import { TipoPrescricao } from '../../domain/models/prescricao.model';
import { PrescricaoReadHttpRepository } from './prescricao-read-http.repository';
import { PrescricaoWriteHttpRepository } from './prescricao-write-http.repository';

const id = '11111111-1111-4111-8111-111111111111';

describe('Adaptadores HTTP de prescricoes', () => {
  let http: HttpTestingController;
  let leitura: PrescricaoReadHttpRepository;
  let escrita: PrescricaoWriteHttpRepository;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        PrescricaoReadHttpRepository,
        PrescricaoWriteHttpRepository,
        { provide: PRESCRICOES_CONSULTA, useExisting: PrescricaoReadHttpRepository },
        { provide: PRESCRICAO_DETALHE_PORT, useExisting: PrescricaoReadHttpRepository },
        { provide: PRESCRICOES_ESCRITA_PORT, useExisting: PrescricaoWriteHttpRepository },
        { provide: PRESCRICAO_ATUALIZACAO_PORT, useExisting: PrescricaoWriteHttpRepository },
        { provide: PRESCRICAO_EXCLUSAO_PORT, useExisting: PrescricaoWriteHttpRepository }
      ]
    });

    http = TestBed.inject(HttpTestingController);
    leitura = TestBed.inject(PrescricaoReadHttpRepository);
    escrita = TestBed.inject(PrescricaoWriteHttpRepository);
  });

  afterEach(() => http.verify());

  it('compartilha a instancia entre as portas de leitura e entre as de escrita', () => {
    expect(TestBed.inject(PRESCRICOES_CONSULTA)).toBe(leitura);
    expect(TestBed.inject(PRESCRICAO_DETALHE_PORT)).toBe(leitura);
    expect(TestBed.inject(PRESCRICOES_ESCRITA_PORT)).toBe(escrita);
    expect(TestBed.inject(PRESCRICAO_ATUALIZACAO_PORT)).toBe(escrita);
    expect(TestBed.inject(PRESCRICAO_EXCLUSAO_PORT)).toBe(escrita);
  });

  it('consulta listagem paginada', () => {
    let quantidade = 0;
    leitura.listar(2, 10).subscribe(resultado => quantidade = resultado.totalRegistros);

    const req = http.expectOne(request =>
      request.url === '/api/prescricoes' &&
      request.params.get('pagina') === '2' &&
      request.params.get('tamanhoPagina') === '10'
    );
    expect(req.request.method).toBe('GET');
    req.flush({ itens: [], pagina: 2, tamanhoPagina: 10, totalRegistros: 3, totalPaginas: 1 });
    expect(quantidade).toBe(3);
  });

  it('consulta os detalhes sem mudar a rota HTTP', () => {
    let recebido = '';
    leitura.obterPorId(id).subscribe(resultado => recebido = resultado.id);

    const req = http.expectOne(`/api/prescricoes/${id}`);
    expect(req.request.method).toBe('GET');
    req.flush({ id, pacienteId: id, tipo: TipoPrescricao.Estruturada, textoLivre: null, criadaEmUtc: '2026-11-17T12:00:00Z', itens: [] });
    expect(recebido).toBe(id);
  });

  it('cria uma prescricao usando POST', () => {
    const body: CriarPrescricaoDto = {
      pacienteId: id,
      tipo: TipoPrescricao.NaoEstruturada,
      textoLivre: 'Teste',
      itens: []
    };
    let recebido = '';
    escrita.criar(body).subscribe(resultado => recebido = resultado.id);

    const req = http.expectOne('/api/prescricoes');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({ id });
    expect(recebido).toBe(id);
  });

  it('atualiza usando PUT', () => {
    const body = { textoLivre: 'Novo texto', itens: [] };
    let concluido = false;
    escrita.atualizar(id, body).subscribe(() => concluido = true);

    const req = http.expectOne(`/api/prescricoes/${id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);
    req.flush(null);
    expect(concluido).toBe(true);
  });

  it('exclui usando DELETE', () => {
    let concluido = false;
    escrita.excluir(id).subscribe(() => concluido = true);

    const req = http.expectOne(`/api/prescricoes/${id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    expect(concluido).toBe(true);
  });
});
