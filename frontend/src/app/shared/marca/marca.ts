import { Component } from '@angular/core';

@Component({
  selector: 'app-marca',
  template: `
    <span class="marca">
      <svg width="28" height="28" viewBox="-4 -4 48 48" aria-hidden="true" focusable="false">
        <g transform="rotate(-14 20 20)">
          <path class="folha"
                d="M20 0 A20 20 0 0 1 40 20 V20.8 A19.2 19.2 0 0 1 20.8 40 H8 A8 8 0 0 1 0 32 V20 A20 20 0 0 1 20 0 Z" />
          <path class="broto" transform="translate(12.5 9) rotate(28 7.5 11)"
                d="M7.5 0 A7.5 11 0 0 1 15 11 V11.4 A7.2 10.6 0 0 1 7.8 22 H3 A3 4.4 0 0 1 0 17.6 V11 A7.5 11 0 0 1 7.5 0 Z" />
        </g>
      </svg>
      <span class="nome">Clínica Acolher</span>
    </span>
  `,
  styles: `
    .marca { display: inline-flex; align-items: center; gap: var(--espaco-2); }
    .folha { fill: var(--cor-primaria); }
    .broto { fill: var(--cor-superficie-destaque); }
    .nome { font: var(--texto-titulo-secao); color: var(--cor-primaria); letter-spacing: -0.5px; }
  `,
})
export class Marca {}