import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { ClinicalNotesPage } from './clinical-notes-page';

describe('ClinicalNotesPage', () => {
  let http: HttpTestingController;

  const vinculoAtivo = { id: 'v1', status: 'Active' };
  const resumoFinalizado = {
    id: 'n1', appointmentId: null, status: 'Finalized',
    createdAt: '2026-11-10T13:00:00Z', lastSavedAt: '2026-11-10T13:40:00Z', finalizedAt: '2026-11-10T13:45:00Z',
    revisionCount: 2, addendumCount: 0,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ClinicalNotesPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  async function abrirPagina(vinculos: unknown[], notas: unknown[]) {
    const fixture = TestBed.createComponent(ClinicalNotesPage);
    fixture.componentRef.setInput('id', 'pac1');
    fixture.detectChanges();

    http.expectOne('/api/v1/patients/pac1').flush({ id: 'pac1', fullName: 'Paciente Teste', socialName: null, active: true });
    http.expectOne('/api/v1/patients/pac1/relationships').flush(vinculos);
    http.expectOne('/api/v1/patients/pac1/clinical-notes').flush(notas);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  function botao(fixture: any, texto: string): HTMLButtonElement | undefined {
    return Array.from<HTMLButtonElement>(fixture.nativeElement.querySelectorAll('button'))
      .find((b) => b.textContent?.trim() === texto);
  }

  it('mostra o texto clínico como texto, nunca como HTML', async () => {
    const fixture = await abrirPagina([vinculoAtivo], [resumoFinalizado]);

    fixture.nativeElement.querySelector('.linha-do-tempo button').click();
    http.expectOne('/api/v1/clinical-notes/n1').flush({
      ...resumoFinalizado,
      patientId: 'pac1',
      content: '<img src=x onerror=alert(1)>Relato fictício',
      revisionNumber: 2,
      addenda: [],
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const texto: HTMLElement = fixture.nativeElement.querySelector('.texto-final');
    expect(texto.querySelector('img')).toBeNull();
    expect(texto.textContent).toContain('<img src=x');
  });

  it('cria a anotação no vínculo aberto, com o corretor ortográfico desligado', async () => {
    const fixture = await abrirPagina([vinculoAtivo], []);

    botao(fixture, 'Nova anotação')!.click();
    fixture.detectChanges();

    const campo: HTMLTextAreaElement = fixture.nativeElement.querySelector('#texto');
    expect(campo.getAttribute('spellcheck')).toBe('false');
    campo.value = 'Relato fictício da sessão.';
    campo.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    botao(fixture, 'Salvar rascunho')!.click();

    const req = http.expectOne('/api/v1/clinical-notes');
    expect(req.request.body).toEqual({ relationshipId: 'v1', appointmentId: null, content: 'Relato fictício da sessão.' });
  });

  it('com o vínculo encerrado, as anotações ficam só para leitura', async () => {
    const fixture = await abrirPagina([{ id: 'v1', status: 'Ended' }], [resumoFinalizado]);

    expect(fixture.nativeElement.textContent).toContain('só para leitura');
    expect(botao(fixture, 'Nova anotação')).toBeUndefined();
  });
});
