import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((erro: HttpErrorResponse) => {
      // Um 401 fora das rotas de login quer dizer que a sessão acabou no meio do trabalho
      if (erro.status === 401 && !req.url.startsWith('/api/v1/auth/')) {
        auth.clearSession();
        router.navigate(['/login'], { queryParams: { voltar: router.url, expirou: 1 } });
      }
      return throwError(() => erro);
    }),
  );
};