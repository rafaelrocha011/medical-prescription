import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { CatalogosPort } from '../../application/ports/catalogos.port';
import { Medicamento, Paciente } from '../../domain/models/catalogo.model';

@Injectable()
export class CatalogosHttpRepository implements CatalogosPort {
  private readonly http = inject(HttpClient);

  listarPacientes() {
    return this.http.get<Paciente[]>('/api/pacientes');
  }

  listarMedicamentos(busca?: string) {
    const termo = busca?.trim();
    const params = termo ? new HttpParams().set('busca', termo) : undefined;
    return this.http.get<Medicamento[]>('/api/medicamentos', { params });
  }
}
