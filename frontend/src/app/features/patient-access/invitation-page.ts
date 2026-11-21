import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { Marca } from '../../shared/marca/marca';
import { PatientAccessApi } from './patient-access-api';

/** Validador do formulário inteiro: as duas senhas precisam ser iguais. */
function senhasIguais(grupo: AbstractControl): ValidationErrors | null {
  const senha = grupo.get('senha')?.value;
  const confirmacao = grupo.get('confirmacao')?.value;
  return senha && confirmacao && senha !== confirmacao ? { senhasDiferentes: true } : null;
}

@Component({
  selector: 'app-invitation-page',
  imports: [ReactiveFormsModule, Marca],
  templateUrl: './invitation-page.html',
  styleUrl: './invitation-page.css',
})
export class InvitationPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PatientAccessApi);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  private codigo = '';

  protected readonly semCodigo = signal(false);
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mostrarSenha = signal(false);

  protected readonly form = new FormGroup(
    {
      senha: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(10)] }),
      confirmacao: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    },
    { validators: senhasIguais },
  );

  ngOnInit(): void {
    this.codigo = this.route.snapshot.fragment ?? '';
    this.semCodigo.set(!this.codigo);

    // Tira o código da barra de endereços: ele não fica no histórico nem aparece numa captura de tela
    if (this.codigo) history.replaceState(history.state, '', location.pathname);
  }

  protected criarAcesso(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    this.api.accept(this.codigo, this.form.controls.senha.value).subscribe({
      next: () => this.auth.refreshSession().subscribe(() => this.router.navigateByUrl('/portal')),
      error: (erro: HttpErrorResponse) => {
        this.enviando.set(false);
        this.erro.set(
          erro.status === 429
            ? 'Muitas tentativas seguidas. Aguarde um minuto e tente de novo.'
            : (erro.error?.detail ?? 'Não foi possível criar o acesso. Tente de novo.'),
        );
      },
    });
  }
}