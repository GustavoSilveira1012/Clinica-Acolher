import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Entrar',
    loadComponent: () => import('./features/auth/login-page').then((m) => m.LoginPage),
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'agenda' },
      {
        path: 'agenda',
        title: 'Agenda',
        loadComponent: () => import('./features/agenda/agenda-page').then((m) => m.AgendaPage),
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
      {
        path: 'status',
        title: 'Status da API',
        loadComponent: () => import('./features/status/status-page/status-page').then((m) => m.StatusPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
