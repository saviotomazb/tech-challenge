import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';

import { mensagemDeErro } from '../nucleo/api';
import { Plano } from '../planos/plano';
import { PlanoServico } from '../planos/plano-servico';
import { Beneficiario } from './beneficiario';
import { BeneficiarioServico } from './beneficiario-servico';

@Component({
  selector: 'app-beneficiarios-lista',
  imports: [FormsModule],
  templateUrl: './beneficiarios-lista.html',
  styleUrl: './beneficiarios-lista.css'
})
export class BeneficiariosLista {
  private readonly servico = inject(BeneficiarioServico);
  private readonly planoServico = inject(PlanoServico);

  protected readonly beneficiarios = signal<Beneficiario[]>([]);
  protected readonly planos = signal<Plano[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);

  protected nomeCompleto = '';
  protected cpf = '';
  protected dataNascimento = '';
  protected planoId = '';

  protected cadastrando = signal(false);
  protected erroCadastro = signal<string | null>(null);

  constructor() {
    this.carregar();
    this.carregarPlanos();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.servico
      .listar()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (beneficiarios) => {
          this.beneficiarios.set(beneficiarios);
          this.carregando.set(false);
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta));
          this.carregando.set(false);
        }
      });
  }

  private carregarPlanos(): void {
    this.planoServico
      .listar()
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: (planos) => {
          this.planos.set(planos);
        },
        error: (resposta: HttpErrorResponse) => {
          this.erroCadastro.set(mensagemDeErro(resposta));
        }
      });
  }

  protected criar(): void {
    this.erroCadastro.set(null);

    if (!this.nomeCompleto || !this.cpf || !this.dataNascimento || !this.planoId) {
      this.erroCadastro.set('Preencha todos os campos.');
      return;
    }

    this.cadastrando.set(true);

    this.servico
      .criar({
        nomeCompleto: this.nomeCompleto,
        cpf: this.cpf,
        dataNascimento: this.dataNascimento,
        planoId: this.planoId
      })
      .pipe(takeUntilDestroyed())
      .subscribe({
        next: () => {
          this.nomeCompleto = '';
          this.cpf = '';
          this.dataNascimento = '';
          this.planoId = '';

          this.cadastrando.set(false);

          this.carregar();
        },
        error: (resposta: HttpErrorResponse) => {
          this.erroCadastro.set(mensagemDeErro(resposta));
          this.cadastrando.set(false);
        }
      });
  }
}