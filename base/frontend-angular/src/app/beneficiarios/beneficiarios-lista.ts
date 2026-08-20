import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, DestroyRef, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';

import { mensagemDeErro } from '../nucleo/api';
import { Plano } from '../planos/plano';
import { PlanoServico } from '../planos/plano-servico';
import { Beneficiario } from './beneficiario';
import { AtualizarBeneficiario, BeneficiarioServico } from './beneficiario-servico';

@Component({
  selector: 'app-beneficiarios-lista',
  imports: [FormsModule, DatePipe],
  templateUrl: './beneficiarios-lista.html',
  styleUrl: './beneficiarios-lista.css'
})
export class BeneficiariosLista {
  private readonly servico = inject(BeneficiarioServico);
  private readonly planoServico = inject(PlanoServico);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly beneficiarios = signal<Beneficiario[]>([]);
  protected readonly planos = signal<Plano[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);

  protected readonly cadastrando = signal(false);
  protected readonly erroCadastro = signal<string | null>(null);

  protected beneficiarioEmEdicao: Beneficiario | null = null;

  protected nome_completo = '';
  protected cpf = '';
  protected data_nascimento = '';
  protected plano_id = '';
  protected status = 'ATIVO';

  constructor() {
    this.carregar();
    this.carregarPlanos();
  }

  protected carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.servico
      .listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (resposta) => {
          this.beneficiarios.set(resposta.dados);
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
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (planos) => {
          this.planos.set(planos);
        },
        error: (resposta: HttpErrorResponse) => {
          this.erroCadastro.set(mensagemDeErro(resposta));
        }
      });
  }

  protected nomePlano(plano_id: string): string {
    return this.planos().find(plano => plano.id === plano_id)?.nome ?? 'Plano não encontrado';
  }

  protected criar(): void {
    this.erroCadastro.set(null);

    if (!this.nome_completo || !this.cpf || !this.data_nascimento || !this.plano_id) {
      this.erroCadastro.set('Preencha todos os campos.');
      return;
    }

    this.cadastrando.set(true);

    this.servico
      .criar({
        nome_completo: this.nome_completo,
        cpf: this.cpf,
        data_nascimento: this.data_nascimento,
        plano_id: this.plano_id
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.limparFormulario();

          this.cadastrando.set(false);

          this.carregar();
        },
        error: (resposta: HttpErrorResponse) => {
          this.erroCadastro.set(mensagemDeErro(resposta));
          this.cadastrando.set(false);
        }
      });
  }

  protected editar(beneficiario: Beneficiario): void {
    this.beneficiarioEmEdicao = beneficiario;

    this.nome_completo = beneficiario.nome_completo;
    this.cpf = beneficiario.cpf;
    this.data_nascimento = beneficiario.data_nascimento;
    this.plano_id = beneficiario.plano_id;
    this.status = beneficiario.status;

    this.erroCadastro.set(null);
  }

  protected atualizar(): void {
    if (!this.beneficiarioEmEdicao) {
      return;
    }

    this.erroCadastro.set(null);

    if (!this.nome_completo || !this.data_nascimento || !this.plano_id || !this.status) {
      this.erroCadastro.set('Preencha todos os campos.');
      return;
    }

    this.cadastrando.set(true);

    const dados: AtualizarBeneficiario = {
      nome_completo: this.nome_completo,
      data_nascimento: this.data_nascimento,
      plano_id: this.plano_id,
      status: this.status
    };

    this.servico
      .atualizar(this.beneficiarioEmEdicao.id, dados)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.limparFormulario();

          this.cadastrando.set(false);

          this.carregar();
        },
        error: (resposta: HttpErrorResponse) => {
          this.erroCadastro.set(mensagemDeErro(resposta));
          this.cadastrando.set(false);
        }
      });
  }

  protected cancelarEdicao(): void {
    this.limparFormulario();
  }

  private limparFormulario(): void {
    this.beneficiarioEmEdicao = null;

    this.nome_completo = '';
    this.cpf = '';
    this.data_nascimento = '';
    this.plano_id = '';
    this.status = 'ATIVO';

    this.erroCadastro.set(null);
  }

  protected excluir(beneficiario: Beneficiario): void {
    const confirmar = confirm(
      `Deseja realmente excluir o beneficiário "${beneficiario.nome_completo}"?`
    );

    if (!confirmar) {
      return;
    }

    this.servico
      .excluir(beneficiario.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.carregar();
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta));
        }
      });
  }
}