import { CriarPrescricaoDto } from './criar-prescricao.dto';

export type AtualizarPrescricaoDto = Pick<
  CriarPrescricaoDto,
  'textoLivre' | 'itens'
>;