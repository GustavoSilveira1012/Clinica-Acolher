import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/status/status-page/status-page').then((m) => m.StatusPage),
  },

  {
    path: 'pacientes/novo',
    title: 'Novo paciente',
    loadComponent: () => import('./features/patients/patient-form-page').then((m) => m.PatientFormPage),
  },
  {
    path: 'pacientes/:id',
    title: 'Editar paciente',
    loadComponent: () => import('./features/patients/patient-form-page').then((m) => m.PatientFormPage),
  },
  {
    path: 'pacientes',
    title: 'Pacientes',
    loadComponent: () => import('./features/patients/patient-list-page').then((m) => m.PatientListPage),
  },
];
