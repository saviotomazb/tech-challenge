import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { API_BASE } from '../nucleo/api';
import { Beneficiario } from './beneficiario';

export interface CriarBeneficiario {
  nome_completo: string;
  cpf: string;
  data_nascimento: string;
  plano_id: string;
}

export interface AtualizarBeneficiario {
  nome_completo: string;
  data_nascimento: string;
  plano_id: string;
  status: string;
}

export interface ListaBeneficiarios {
  dados: Beneficiario[];
  pagina: number;
  tamanho: number;
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class BeneficiarioServico {
  private readonly http = inject(HttpClient);
  private readonly apiBase = inject(API_BASE);

  listar() {
    return this.http.get<ListaBeneficiarios>(
      `${this.apiBase}/beneficiarios`
    );
  }

  criar(beneficiario: CriarBeneficiario) {
    return this.http.post<Beneficiario>(
      `${this.apiBase}/beneficiarios`,
      beneficiario
    );
  }

  atualizar(id: string, beneficiario: AtualizarBeneficiario) {
    return this.http.put<Beneficiario>(
      `${this.apiBase}/beneficiarios/${id}`,
      beneficiario
    );
  }

  excluir(id: string) {
    return this.http.delete<void>(
      `${this.apiBase}/beneficiarios/${id}`
    );
  }
}