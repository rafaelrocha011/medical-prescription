import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { CatalogosPort } from '../../application/ports/catalogos.port';
import { Medicamento, Paciente } from '../../domain/models/catalogo.model';

@Injectable()
export class CatalogosHttpRepository implements CatalogosPort {
  private readonly http = inject(HttpClient);

  listarPacientes() {
    return this.http.get<Paciente[]>('/api/pacientes');
  }

  listarMedicamentos() {
    return this.http.get<Medicamento[]>('/api/medicamentos');
  }
}