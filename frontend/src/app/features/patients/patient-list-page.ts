import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { PatientsApi } from './patients-api';
import { PagedResult, PatientListItem } from './patient.models';

type Filtro = 'ativos' | 'inativos' | 'todos';

@Component({
  selector: 'app-patient-list-page',
  imports: [ReactiveFormsModule],
  templateUrl: './patient-list-page.html',
  styleUrl: './patient-list-page.css',
})
export class PatientListPage implements OnInit {
  private readonly api = inject(PatientsApi);
  private readonly pageSize = 20;

  protected readonly form = new FormGroup({
    busca: new FormControl('', { nonNullable: true }),
  });

  protected readonly opcoesFiltro: { valor: Filtro; rotulo: string }[] = [
    { valor: 'ativos', rotulo: 'Ativos' },
    { valor: 'inativos', rotulo: 'Inativos' },
    { valor: 'todos', rotulo: 'Todos' },
  ];

  protected readonly filtro = signal<Filtro>('ativos');
  protected readonly pagina = signal(1);
  protected readonly erro = signal(false);
  protected readonly resultado = signal<PagedResult<PatientListItem> | null>(null);

  protected readonly totalPaginas = computed(() => {
    const r = this.resultado();
    return r ? Math.max(1, Math.ceil(r.totalCount / r.pageSize)) : 1;
  });

  ngOnInit(): void {
    this.carregar();
  }

  protected buscar(): void {
    this.pagina.set(1);
    this.carregar();
  }

  protected mudarFiltro(filtro: Filtro): void {
    this.filtro.set(filtro);
    this.buscar();
  }

  protected irParaPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.carregar();
  }

  private carregar(): void {
    this.erro.set(false);
    const filtro = this.filtro();

    this.api
      .search({
        search: this.form.controls.busca.value.trim() || null,
        active: filtro === 'todos' ? null : filtro === 'ativos',
        page: this.pagina(),
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (r) => this.resultado.set(r),
        error: () => this.erro.set(true),
      });
  }
}