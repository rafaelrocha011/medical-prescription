import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { Medicamento, Paciente } from '../../domain/models/catalogo.model';

export interface CatalogosPort {
  listarPacientes(): Observable<Paciente[]>;
  listarMedicamentos(): Observable<Medicamento[]>;
}

export const CATALOGOS_PORT = new InjectionToken<CatalogosPort>(
  'CATALOGOS_PORT'
);