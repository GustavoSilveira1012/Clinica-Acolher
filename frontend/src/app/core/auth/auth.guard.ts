import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    return auth
        .loadSession()
        .pipe(map((usuario) => usuario ? true : router.createUrlTree(['/login'], {queryParams: {voltar: state.url}})));
};