import { TipoPrescricao } from '../../domain/models/prescricao.model';

export interface CriarPrescricaoItemDto {
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

export interface CriarPrescricaoDto {
  pacienteId: string;
  tipo: TipoPrescricao;
  textoLivre: string | null;
  itens: CriarPrescricaoItemDto[];
}

export interface PrescricaoCriadaDto {
  id: string;
}