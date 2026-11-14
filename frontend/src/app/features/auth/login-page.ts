import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { destinoSeguro } from '../../core/auth/safe-redirect';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly enviando = signal(false);
  protected readonly mensagemErro = signal<string | null>(null);
  protected readonly sessaoExpirou = this.route.snapshot.queryParamMap.has('expirou');

  protected readonly form = new FormGroup({
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    senha: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  protected entrar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, senha } = this.form.getRawValue();
    this.enviando.set(true);
    this.mensagemErro.set(null);

    this.auth.login(email, senha).subscribe({
      next: () => this.router.navigateByUrl(destinoSeguro(this.route.snapshot.queryParamMap.get('voltar'))),
      error: (erro: HttpErrorResponse) => {
        this.enviando.set(false);
        this.form.controls.senha.reset();
        this.mensagemErro.set(
          erro.status === 401
            ? (erro.error?.detail ?? 'E-mail ou senha incorretos.')
            : 'Não foi possível entrar agora. Tente novamente em instantes.',
        );
      },
    });
  }
}