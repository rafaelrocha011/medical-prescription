export enum TipoPrescricao {
  NaoEstruturada = 1,
  Estruturada = 2
}

export interface PrescricaoResumo {
  id: string;
  pacienteId: string;
  tipo: TipoPrescricao;
  criadaEmUtc: string;
  quantidadeMedicamentos: number;
}

export interface ResultadoPaginado<T> {
  itens: T[];
  pagina: number;
  tamanhoPagina: number;
  totalRegistros: number;
  totalPaginas: number;
}
