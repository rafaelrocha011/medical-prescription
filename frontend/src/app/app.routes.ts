import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'prescricoes'
  },
  {
    path: 'prescricoes/nova',
    loadComponent: () =>
      import('./features/prescricoes/presentation/pages/formulario-prescricao')
        .then(m => m.FormularioPrescricao)
  },
  {
    path: 'prescricoes',
    loadComponent: () =>
      import('./features/prescricoes/presentation/pages/lista-prescricoes')
        .then(m => m.ListaPrescricoes)
  },
  {
    path: 'prescricoes/:id/editar',
    loadComponent: () =>
      import('./features/prescricoes/presentation/pages/formulario-prescricao')
        .then(m => m.FormularioPrescricao)
  },
  {
    path: 'prescricoes/:id',
    loadComponent: () =>
      import('./features/prescricoes/presentation/pages/detalhe-prescricao')
        .then(m => m.DetalhePrescricao)
  },
  {
    path: '**',
    redirectTo: 'prescricoes'
  }
];
