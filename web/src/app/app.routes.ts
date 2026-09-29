import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard/jobs' },
  {
    path: 'dashboard/jobs',
    loadComponent: () => import('./features/jobs/pages/jobs-page/jobs-page.component')
      .then(module => module.JobsPageComponent),
  },
  {
    path: '**',
    loadComponent: () => import('./shared/pages/not-found/not-found.component')
      .then(module => module.NotFoundComponent),
  },
];
