import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { Avisos } from './shared/avisos/avisos';
import { Icone, NomeIcone } from './shared/icone/icone';
import { Marca } from './shared/marca/marca';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Avisos, Icone, Marca],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  // O menu mostra só o que existe para quem está logado: o Financeiro entra com a M7
  protected readonly menu = computed(() => {
    const usuario = this.auth.user();

    if (usuario?.roles.includes('Patient'))
      return [{ rota: '/portal', rotulo: 'Minhas sessões', icone: 'agenda' as NomeIcone }];

    const itens: { rota: string; rotulo: string; icone: NomeIcone }[] = [
      { rota: '/agenda', rotulo: 'Agenda', icone: 'agenda' },
      { rota: '/pacientes', rotulo: 'Pacientes', icone: 'pessoas' },
    ];
    if (usuario?.professionalId) itens.push({ rota: '/horarios', rotulo: 'Horários', icone: 'relogio' });
    return itens;
  });

  protected readonly papel = computed(() => {
    const papeis = this.auth.user()?.roles ?? [];
    return [
      papeis.includes('Professional') && 'Profissional',
      papeis.includes('Admin') && 'Administração',
      papeis.includes('Patient') && 'Paciente',
    ]
      .filter(Boolean)
      .join(' · ');
  });

  protected readonly inicial = computed(() => (this.auth.user()?.email[0] ?? '?').toUpperCase());

  protected sair(): void {
    const irParaLogin = () => this.router.navigate(['/login']);
    this.auth.logout().subscribe({ complete: irParaLogin, error: irParaLogin });
  }
}