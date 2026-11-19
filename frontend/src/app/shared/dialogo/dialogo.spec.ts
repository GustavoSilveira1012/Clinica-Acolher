import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Dialogo } from './dialogo';

@Component({
  imports: [Dialogo],
  template: `<app-dialogo titulo="Teste" [aberto]="aberto()" (fechar)="fechamentos = fechamentos + 1" />`,
})
class Hospedeiro {
  readonly aberto = signal(false);
  fechamentos = 0;
}

describe('Dialogo', () => {
  function montar() {
    const fixture = TestBed.createComponent(Hospedeiro);
    fixture.detectChanges();
    const dialogo = fixture.nativeElement.querySelector('dialog') as HTMLDialogElement;
    return { fixture, hospedeiro: fixture.componentInstance, dialogo };
  }

  it('avisa o fechamento quando o usuário fecha a janela (Esc)', () => {
    const { fixture, hospedeiro, dialogo } = montar();
    hospedeiro.aberto.set(true);
    fixture.detectChanges();

    dialogo.dispatchEvent(new Event('close'));

    expect(hospedeiro.fechamentos).toBe(1);
  });

  it('não avisa o fechamento quando a própria tela fechou a janela', () => {
    // Abrir outra janela a partir desta (ex.: "Remarcar") fecha esta; o evento close chega depois
    // e não pode desfazer a janela recém-aberta
    const { fixture, hospedeiro, dialogo } = montar();
    hospedeiro.aberto.set(true);
    fixture.detectChanges();
    hospedeiro.aberto.set(false);
    fixture.detectChanges();

    dialogo.dispatchEvent(new Event('close'));

    expect(hospedeiro.fechamentos).toBe(0);
  });
});
