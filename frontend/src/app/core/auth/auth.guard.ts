import { inject } from '@angular/core';
import { CanActivateChildFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

export const ROTA_DOIS_FATORES = '/seguranca/dois-fatores';

export const authGuard: CanActivateChildFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.loadSession().pipe(
    map((usuario) => {
      if (!usuario)
        return router.createUrlTree(['/login'], { queryParams: { voltar: state.url } });

      // RN-POR-04: o paciente só circula pelo portal, e a equipe, pela área da clínica
      const ehPaciente = usuario.roles.includes('Patient');
      if (ehPaciente && !state.url.startsWith('/portal')) return router.createUrlTree(['/portal']);
      if (!ehPaciente && state.url.startsWith('/portal')) return router.createUrlTree(['/agenda']);

      // RN-SEG-03: a equipe configura o 2FA antes de qualquer outra tela
      if (usuario.mustSetupTwoFactor && !state.url.startsWith(ROTA_DOIS_FATORES))
        return router.createUrlTree([ROTA_DOIS_FATORES]);

      return true;
    }),
  );
};
