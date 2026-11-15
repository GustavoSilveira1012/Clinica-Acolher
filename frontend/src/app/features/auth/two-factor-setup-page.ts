import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, TwoFactorSetup } from '../../core/auth/auth.service';

type Etapa = 'carregando' | 'configurar' | 'codigos' | 'ja-ativo' | 'erro';

@Component({
  selector: 'app-two-factor-setup-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './two-factor-setup-page.html',
  styleUrl: './two-factor-setup-page.css',
})
export class TwoFactorSetupPage implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly etapa = signal<Etapa>('carregando');
  protected readonly configuracao = signal<TwoFactorSetup | null>(null);
  protected readonly codigosRecuperacao = signal<string[]>([]);
  protected readonly mensagemErro = signal<string | null>(null);
  protected readonly enviando = signal(false);
  protected readonly copiado = signal(false);

  protected readonly formCodigo = new FormGroup({
    codigo: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });
  protected readonly guardou = new FormControl(false, { nonNullable: true });

  ngOnInit(): void {
    this.auth.setupTwoFactor().subscribe({
      next: (configuracao) => {
        this.configuracao.set(configuracao);
        this.etapa.set('configurar');
      },
      error: (erro: HttpErrorResponse) => this.etapa.set(erro.status === 409 ? 'ja-ativo' : 'erro'),
    });
  }

  protected ativar(): void {
    if (this.formCodigo.invalid) {
      this.formCodigo.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.mensagemErro.set(null);

    this.auth.enableTwoFactor(this.formCodigo.controls.codigo.value).subscribe({
      next: (codigos) => {
        // A chave secreta não é mais necessária: sai da tela e da memória
        this.configuracao.set(null);
        this.codigosRecuperacao.set(codigos);
        this.etapa.set('codigos');
        this.enviando.set(false);
      },
      error: (erro: HttpErrorResponse) => {
        this.enviando.set(false);
        this.formCodigo.reset();
        this.mensagemErro.set(erro.error?.detail ?? 'Não foi possível ativar agora. Tente novamente.');
      },
    });
  }

  protected async copiar(): Promise<void> {
    await navigator.clipboard.writeText(this.codigosRecuperacao().join('\n'));
    this.copiado.set(true);
  }

  protected baixar(): void {
    const texto = [
      'Clínica Acolher: códigos de recuperação',
      'Cada código funciona uma única vez.',
      '',
      ...this.codigosRecuperacao(),
    ].join('\n');

    const url = URL.createObjectURL(new Blob([texto], { type: 'text/plain' }));
    const link = document.createElement('a');
    link.href = url;
    link.download = 'clinica-acolher-codigos-de-recuperacao.txt';
    link.click();
    URL.revokeObjectURL(url);
  }

  protected concluir(): void {
    this.codigosRecuperacao.set([]);
    this.auth.refreshSession().subscribe(() => this.router.navigateByUrl('/agenda'));
  }
}