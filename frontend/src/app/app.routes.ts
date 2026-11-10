import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/status/status-page/status-page').then((m) => m.StatusPage),
  },

  {
    path: 'pacientes',
    title: 'Pacientes',
    loadComponent: () => import('./features/patients/patient-list-page').then((m) => m.PatientListPage),
  },

  
];
