import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'browse/all',
    loadComponent: () =>
      import('./features/products/product-list/product-list.component').then(m => m.ProductListComponent)
  },
  {
    path: 'browse/:categoryId',
    loadComponent: () =>
      import('./features/products/product-browse/product-browse.component').then(m => m.ProductBrowseComponent)
  },
  {
    path: 'products',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/products/product-list/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/products/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'edit/:id',
        loadComponent: () =>
          import('./features/products/product-form/product-form.component').then(m => m.ProductFormComponent)
      }
    ]
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/register/register.component').then(
        m => m.RegisterComponent
      )
  },
  { path: '', redirectTo: '/browse/all', pathMatch: 'full' },
  { path: '**', redirectTo: '/browse/all' }
];
