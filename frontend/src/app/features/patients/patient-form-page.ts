import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { PatientsApi } from './patients-api';
import { PatientRequest, PatientResponse } from './patient.models';

type Campo = 'fullName' | 'socialName' | 'birthDate' | 'email' | 'phone';

@Component({
  selector: 'app-patient-form-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './patient-form-page.html',
  styleUrl: './patient-form-page.css',
})
export class PatientFormPage implements OnInit {
  private readonly api = inject(PatientsApi);
  private readonly router = inject(Router);

  readonly id = input<string>();

  protected readonly editando = computed(() => !!this.id());
  protected readonly salvando = signal(false);
  protected readonly mensagemErro = signal<string | null>(null);
  protected readonly ativo = signal(true);
  protected readonly confirmandoDesativacao = signal(false);
  protected readonly motivo = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.maxLength(500)],
  });

  protected readonly form = new FormGroup({
    fullName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2), Validators.maxLength(160)],
    }),
    socialName: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(160)] }),
    birthDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.email, Validators.maxLength(254)] }),
    phone: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(30)] }),
  });

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;

    this.api.getById(id).subscribe({
      next: (p) => {
        this.ativo.set(p.active);
        this.form.setValue({
          fullName: p.fullName,
          socialName: p.socialName ?? '',
          birthDate: p.birthDate,
          email: p.email ?? '',
          phone: p.phone ?? '',
        });
      },
      error: () => this.mensagemErro.set('Não foi possível carregar o paciente.'),
    });
  }

  protected mostrarErro(campo: Campo): boolean {
    const controle = this.form.controls[campo];
    return controle.invalid && controle.touched;
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const request: PatientRequest = {
      fullName: v.fullName.trim(),
      birthDate: v.birthDate,
      socialName: v.socialName.trim() || null,
      email: v.email.trim() || null,
      phone: v.phone.trim() || null,
    };

    const id = this.id();
    const chamada = id ? this.api.update(id, request) : this.api.create(request);

    this.salvando.set(true);
    this.mensagemErro.set(null);

    chamada.subscribe({
      next: () => this.router.navigate(['/pacientes']),
      error: (erro: HttpErrorResponse) => {
        this.salvando.set(false);
        this.mensagemErro.set(this.traduzirErro(erro));
      },
    });
  }

  protected iniciarDesativacao(): void {
    this.motivo.reset();
    this.confirmandoDesativacao.set(true);
  }

  protected cancelarDesativacao(): void {
    this.confirmandoDesativacao.set(false);
  }

  protected confirmarDesativacao(): void {
    const id = this.id();
    const motivo = this.motivo.value.trim();
    if (!id) return;
    if (!motivo || this.motivo.invalid) {
      this.motivo.markAsTouched();
      return;
    }

    this.executar(this.api.deactivate(id, motivo));
  }

  protected reativar(): void {
    const id = this.id();
    if (id) this.executar(this.api.reactivate(id));
  }

  private executar(chamada: Observable<PatientResponse>): void {
    this.salvando.set(true);
    this.mensagemErro.set(null);

    chamada.subscribe({
      next: (p) => {
        this.ativo.set(p.active);
        this.confirmandoDesativacao.set(false);
        this.salvando.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        this.salvando.set(false);
        this.mensagemErro.set(this.traduzirErro(erro));
      },
    });
  }

  private traduzirErro(erro: HttpErrorResponse): string {
    if (erro.status === 422) return erro.error?.detail ?? 'Os dados não atendem às regras do cadastro.';
    if (erro.status === 400) return 'Alguns campos estão inválidos. Confira e tente de novo.';
    return 'Não foi possível salvar agora. Tente novamente em instantes.';
  }
}
