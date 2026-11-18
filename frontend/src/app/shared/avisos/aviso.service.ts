import { Injectable, signal } from '@angular/core';

export interface Aviso {
  id: number;
  texto: string;
}

@Injectable({ providedIn: 'root' })
export class AvisoService {
  static readonly DURACAO_MS = 5000;

  private readonly _avisos = signal<Aviso[]>([]);
  private proximoId = 0;

  readonly avisos = this._avisos.asReadonly();

  /**
   * Para confirmações curtas, como "Sessão cancelada.".
   * Erros ficam na própria tela, onde não somem sozinhos.
   * Nunca inclua nome de paciente: o aviso aparece por cima de tudo, à vista de quem passar.
   */
  mostrar(texto: string): void {
    const id = ++this.proximoId;
    this._avisos.update((lista) => [...lista, { id, texto }]);
    setTimeout(() => this.fechar(id), AvisoService.DURACAO_MS);
  }

  fechar(id: number): void {
    this._avisos.update((lista) => lista.filter((aviso) => aviso.id !== id));
  }
}   