import { Component, ElementRef, effect, input, output, viewChild } from '@angular/core';
import { Icone } from '../icone/icone';

let contador = 0;

@Component({
  selector: 'app-dialogo',
  imports: [Icone],
  template: `
    <dialog #dialogo class="dialogo" [attr.aria-labelledby]="idTitulo" (close)="fechar.emit()">
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
      if (this.aberto() && !elemento.open) elemento.showModal();
      if (!this.aberto() && elemento.open) elemento.close();
    });
  }
}