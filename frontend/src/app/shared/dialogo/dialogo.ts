import { Component, ElementRef, effect, input, output, viewChild } from '@angular/core';
import { Icone } from '../icone/icone';

let contador = 0;

@Component({
  selector: 'app-dialogo',
  imports: [Icone],
  template: `
    <dialog #dialogo class="dialogo" [attr.aria-labelledby]="idTitulo" (close)="aoFecharNativo()">
      <header>
        <h2 [id]="idTitulo">{{ titulo() }}</h2>
        <button type="button" class="fechar" (click)="fechar.emit()" aria-label="Fechar">
          <app-icone nome="x" />
        </button>
      </header>
      <div class="conteudo">
        <ng-content />
      </div>
      <footer>
        <ng-content select="[acoes]" />
      </footer>
    </dialog>
  `,
  styles: `
    .dialogo {
      width: min(560px, calc(100% - 32px)); padding: 0; border: none; border-radius: 16px;
      background: var(--cor-superficie); color: var(--cor-texto);
      box-shadow: 0 25px 70px rgb(0 0 0 / 20%);
    }
    .dialogo::backdrop { background: var(--cor-veu); }
    header {
      display: flex; align-items: center; justify-content: space-between; gap: var(--espaco-4);
      padding: var(--espaco-4) var(--espaco-6); border-bottom: 1px solid var(--cor-borda);
    }
    .fechar {
      display: inline-flex; padding: var(--espaco-2); background: none; border: none;
      color: var(--cor-texto-suave); cursor: pointer; border-radius: var(--raio-campo);
    }
    .conteudo { padding: var(--espaco-4) var(--espaco-6); }
    footer { padding: 0 var(--espaco-6) var(--espaco-4); }
  `,
})
export class Dialogo {
  readonly titulo = input.required<string>();
  readonly aberto = input(false);
  readonly fechar = output<void>();

  private readonly dialogo = viewChild.required<ElementRef<HTMLDialogElement>>('dialogo');
  protected readonly idTitulo = `dialogo-titulo-${++contador}`;

  constructor() {
    effect(() => {
      const elemento = this.dialogo().nativeElement;
      const aberto = elemento.hasAttribute('open');

      if (this.aberto() && !aberto) {
        // Ambientes sem suporte ao <dialog> (como o navegador simulado dos testes) só ganham o atributo
        if (typeof elemento.showModal === 'function') elemento.showModal();
        else elemento.setAttribute('open', '');
      }

      if (!this.aberto() && aberto) {
        if (typeof elemento.close === 'function') elemento.close();
        else elemento.removeAttribute('open');
      }
    });
  }

  // O evento close chega depois, também quando a própria tela fechou a janela (ex.: ao abrir
  // "Remarcar" a partir de "Sessão"). Só o fechamento pelo usuário (Esc) deve ser avisado;
  // senão o aviso atrasado desfaria a janela que acabou de abrir.
  protected aoFecharNativo(): void {
    if (this.aberto()) this.fechar.emit();
  }
}