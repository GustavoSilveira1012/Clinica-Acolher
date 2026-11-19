import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Observable, forkJoin } from 'rxjs';
import { ClinicalNotesApi } from './clinical-notes-api';
import { ClinicalNote, ClinicalNoteSummary, PatientBasics, RelationshipBasics } from './clinical-notes.models';

const LIMITE = 20_000;

@Component({
  selector: 'app-clinical-notes-page',
  imports: [DatePipe, NgTemplateOutlet, ReactiveFormsModule, RouterLink],
  templateUrl: './clinical-notes-page.html',
  styleUrl: './clinical-notes-page.css',
})
export class ClinicalNotesPage implements OnInit {
  private readonly api = inject(ClinicalNotesApi);

  readonly id = input.required<string>();

  protected readonly limite = LIMITE;
  protected readonly paciente = signal<PatientBasics | null>(null);
  protected readonly vinculoAberto = signal<RelationshipBasics | null>(null);
  protected readonly notas = signal<ClinicalNoteSummary[]>([]);
  protected readonly notaAberta = signal<ClinicalNote | null>(null);
  protected readonly modo = signal<'nova' | 'nota' | null>(null);
  protected readonly carregado = signal(false);
  protected readonly salvando = signal(false);
  protected readonly confirmandoFinalizacao = signal(false);
  protected readonly mensagem = signal('');
  protected readonly erro = signal<string | null>(null);

  protected readonly texto = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(LIMITE)],
  });
  protected readonly adendo = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(LIMITE)],
  });

  private readonly textoAtual = toSignal(this.texto.valueChanges, { initialValue: '' });
  private readonly textoSalvo = signal('');
  protected readonly alterado = computed(() => this.textoAtual() !== this.textoSalvo());
  protected readonly caracteres = computed(() => this.textoAtual().length);

  ngOnInit(): void {
    forkJoin({
      paciente: this.api.getPatient(this.id()),
      vinculos: this.api.listRelationships(this.id()),
      notas: this.api.listByPatient(this.id()),
    }).subscribe({
      next: ({ paciente, vinculos, notas }) => {
        this.paciente.set(paciente);
        this.vinculoAberto.set(vinculos.find((v) => v.status !== 'Ended') ?? null);
        this.notas.set(notas);
        this.carregado.set(true);
      },
      error: (erro: HttpErrorResponse) => this.erro.set(this.traduzir(erro)),
    });
  }

  protected nomeExibicao(p: PatientBasics): string {
    return p.socialName || p.fullName;
  }

  protected novaNota(): void {
    if (!this.podeTrocar()) return;
    this.notaAberta.set(null);
    this.prepararTexto('');
    this.modo.set('nova');
  }

  protected abrir(resumo: ClinicalNoteSummary): void {
    if (!this.podeTrocar()) return;
    this.erro.set(null);
    this.api.get(resumo.id).subscribe({
      next: (nota) => this.mostrar(nota),
      error: (erro: HttpErrorResponse) => this.erro.set(this.traduzir(erro)),
    });
  }

  protected salvar(): void {
    if (this.texto.invalid || this.texto.value.trim() === '') {
      this.texto.markAsTouched();
      return;
    }
    const nota = this.notaAberta();
    const chamada = nota
      ? this.api.saveDraft(nota.id, this.texto.value)
      : this.api.create(this.vinculoAberto()!.id, this.texto.value);

    this.executar(chamada, (salva) => `Rascunho salvo (versão ${salva.revisionNumber}).`);
  }

  protected pedirFinalizacao(): void {
    this.confirmandoFinalizacao.set(true);
  }

  protected cancelarFinalizacao(): void {
    this.confirmandoFinalizacao.set(false);
  }

  protected finalizar(): void {
    const nota = this.notaAberta();
    if (!nota) return;
    this.executar(this.api.finalize(nota.id), () => 'Anotação finalizada.');
  }

  protected acrescentarAdendo(): void {
    const nota = this.notaAberta();
    if (!nota || this.adendo.invalid || this.adendo.value.trim() === '') {
      this.adendo.markAsTouched();
      return;
    }
    this.executar(this.api.addAddendum(nota.id, this.adendo.value), () => 'Adendo acrescentado.');
  }

  private executar(chamada: Observable<ClinicalNote>, aviso: (nota: ClinicalNote) => string): void {
    this.salvando.set(true);
    this.erro.set(null);
    chamada.subscribe({
      next: (nota) => {
        this.salvando.set(false);
        this.mostrar(nota);
        this.mensagem.set(aviso(nota));
        this.recarregarLista();
      },
      error: (erro: HttpErrorResponse) => {
        this.salvando.set(false);
        this.confirmandoFinalizacao.set(false);
        this.erro.set(this.traduzir(erro));
      },
    });
  }

  private mostrar(nota: ClinicalNote): void {
    this.notaAberta.set(nota);
    this.modo.set('nota');
    this.confirmandoFinalizacao.set(false);
    this.adendo.reset();
    this.prepararTexto(nota.status === 'Draft' ? nota.content : '');
  }

  private prepararTexto(valor: string): void {
    this.texto.reset(valor);
    this.textoSalvo.set(valor);
    this.mensagem.set('');
  }

  private recarregarLista(): void {
    this.api.listByPatient(this.id()).subscribe({ next: (notas) => this.notas.set(notas) });
  }

  // Perder um texto clínico sem perceber é muito pior do que um clique a mais
  private podeTrocar(): boolean {
    return !this.alterado() || window.confirm('Há alterações não salvas nesta anotação. Deseja descartá-las?');
  }

  private traduzir(erro: HttpErrorResponse): string {
    if ([403, 404, 409, 422].includes(erro.status))
      return erro.error?.detail ?? 'Não foi possível concluir a ação.';
    return 'Não foi possível concluir a ação. Verifique a conexão e tente de novo.';
  }
}