import { Component, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { PatientAccessApi } from './patient-access-api';

@Component({
  selector: 'app-convite-portal',
  imports: [DatePipe],
  template: `
    <section class="painel" aria-labelledby="portal-titulo">
      <h2 id="portal-titulo">Acesso ao portal</h2>
      <p class="texto-suave">
        No portal, o paciente acompanha as próprias sessões. Ele entra por um link de convite que você envia.
      </p>

      @if (erro(); as msg) {
        <p role="alert" class="mensagem mensagem-erro">{{ msg }}</p>
      }

      @if (link(); as url) {
        <label for="link-convite">Link de convite</label>
        <div class="linha">
          <input id="link-convite" readonly [value]="url" (focus)="$any($event.target).select()" />
          <button type="button" class="botao botao-secundario" (click)="copiar(url)">
            {{ copiado() ? 'Copiado!' : 'Copiar' }}
          </button>
        </div>
        <p class="mensagem mensagem-atencao">
          Válido até {{ expiraEm() | date: "dd/MM 'às' HH:mm" }}, para um único uso.
          Envie só para o próprio paciente. Gerar um novo link invalida este.
        </p>
      }

      <button type="button" class="botao botao-primario" (click)="gerar()" [disabled]="gerando()">
        {{ link() ? 'Gerar um novo link' : 'Gerar link de convite' }}
      </button>
    </section>
  `,
  styles: `
    .painel { display: grid; gap: var(--espaco-2); justify-items: start; }
    .linha { display: flex; gap: var(--espaco-2); width: 100%; }
    .linha input { flex: 1; min-width: 0; }
  `,
})
export class ConvitePortal {
  private readonly api = inject(PatientAccessApi);

  readonly pacienteId = input.required<string>();

  protected readonly link = signal<string | null>(null);
  protected readonly expiraEm = signal<string | null>(null);
  protected readonly erro = signal<string | null>(null);
  protected readonly gerando = signal(false);
  protected readonly copiado = signal(false);

  protected gerar(): void {
    this.gerando.set(true);
    this.erro.set(null);
    this.copiado.set(false);

    this.api.issueInvitation(this.pacienteId()).subscribe({
      next: (convite) => {
        this.gerando.set(false);
        // RN-POR-03: o código vai no fragmento (#), que o navegador não envia ao servidor
        this.link.set(`${location.origin}/convite#${convite.token}`);
        this.expiraEm.set(convite.expiresAt);
      },
      error: (erro: HttpErrorResponse) => {
        this.gerando.set(false);
        this.link.set(null);
        this.erro.set(erro.error?.detail ?? 'Não foi possível gerar o convite.');
      },
    });
  }

  protected async copiar(url: string): Promise<void> {
    await navigator.clipboard.writeText(url);
    this.copiado.set(true);
  }
}