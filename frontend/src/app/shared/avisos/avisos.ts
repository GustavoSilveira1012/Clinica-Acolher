import { Component, inject } from '@angular/core';
import { Icone } from '../icone/icone';
import { AvisoService } from './aviso.service';

@Component({
  selector: 'app-avisos',
  imports: [Icone],
  template: `
    <div class="avisos" role="status" aria-live="polite">
      @for (aviso of servico.avisos(); track aviso.id) {
        <div class="aviso">
          <app-icone nome="check" [tamanho]="16" />
          <span>{{ aviso.texto }}</span>
          <button type="button" (click)="servico.fechar(aviso.id)" aria-label="Fechar aviso">
            <app-icone nome="x" [tamanho]="16" />
          </button>
        </div>
      }
    </div>
  `,
  styles: `
    .avisos {
      position: fixed; right: var(--espaco-6); bottom: var(--espaco-6); z-index: 80;
      display: grid; gap: var(--espaco-2);
    }
    .aviso {
      display: flex; align-items: center; gap: var(--espaco-2);
      min-height: 48px; padding: 0 var(--espaco-2) 0 var(--espaco-3);
      background: var(--cor-primaria-forte); color: var(--cor-sobre-primaria);
      border-radius: 10px; box-shadow: 0 14px 40px rgb(0 0 0 / 20%);
      font: var(--texto-rotulo);
    }
    .aviso button {
      display: inline-flex; padding: var(--espaco-2);
      background: none; border: none; color: inherit; cursor: pointer; border-radius: var(--raio-campo);
    }
    @media (max-width: 980px) {
      .avisos { right: var(--espaco-4); left: var(--espaco-4); bottom: 88px; }
    }
  `,
})
export class Avisos {
  protected readonly servico = inject(AvisoService);
}