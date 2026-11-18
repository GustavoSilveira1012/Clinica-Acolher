import { Component, computed, input } from '@angular/core';

export type NomeIcone = 'agenda' | 'pessoas' | 'sair' | 'x' | 'check' | 'alerta';

// Desenhos do protótipo, num quadro de 24 × 24
const CAMINHOS: Record<NomeIcone, string[]> = {
  agenda: [
    'M6 5h12a3 3 0 0 1 3 3v10a3 3 0 0 1-3 3H6a3 3 0 0 1-3-3V8a3 3 0 0 1 3-3z',
    'M16 3v4M8 3v4M3 10h18',
    'M8 14h.01M12 14h.01M16 14h.01M8 18h.01M12 18h.01',
  ],
  pessoas: [
    'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2',
    'M5 7a4 4 0 1 0 8 0a4 4 0 1 0-8 0',
    'M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75',
  ],
  sair: ['M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4', 'm16 17 5-5-5-5', 'M21 12H9'],
  x: ['m6 6 12 12M18 6 6 18'],
  check: ['m5 12 4 4L19 6'],
  alerta: [
    'M10.3 3.5 2.4 18a2 2 0 0 0 1.8 3h15.6a2 2 0 0 0 1.8-3L13.7 3.5a2 2 0 0 0-3.4 0',
    'M12 9v4M12 17h.01',
  ],
};

@Component({
  selector: 'app-icone',
  template: `
    <svg [attr.width]="tamanho()" [attr.height]="tamanho()" viewBox="0 0 24 24" fill="none"
         stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"
         aria-hidden="true" focusable="false">
      @for (d of caminhos(); track $index) {
        <path [attr.d]="d" />
      }
    </svg>
  `,
  styles: `:host { display: inline-flex; }`,
})
export class Icone {
  readonly nome = input.required<NomeIcone>();
  readonly tamanho = input(20);
  protected readonly caminhos = computed(() => CAMINHOS[this.nome()]);
}