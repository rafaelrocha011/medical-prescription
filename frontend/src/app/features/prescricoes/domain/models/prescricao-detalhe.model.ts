import { TipoPrescricao } from './prescricao.model';

export interface PrescricaoItemDetalhe {
  id: string;
  medicamentoId: string | null;
  nomeMedicamento: string;
  formaFarmaceutica: string;
  dose: number;
  unidadeMedida: string;
  frequencia: string;
  inicioTratamento: string;
  duracaoTratamentoDias: number | null;
  usoContinuo: boolean;
  viaAdministracao: string;
  prazoUtilizacao: string;
  observacao: string | null;
}

export interface PrescricaoDetalhe {
  id: string;
  pacienteId: string;
  nomePaciente: string;
  tipo: TipoPrescricao;
  textoLivre: string | null;
  criadaEmUtc: string;
  itens: PrescricaoItemDetalhe[];
}