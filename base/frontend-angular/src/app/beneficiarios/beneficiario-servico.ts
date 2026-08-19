import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { API_BASE } from '../nucleo/api';
import { Beneficiario } from './beneficiario';

export interface CriarBeneficiario {
  nomeCompleto: string;
  cpf: string;
  dataNascimento: string;
  planoId: string;
}

@Injectable({
  providedIn: 'root'
})
export class BeneficiarioServico {
  private readonly http = inject(HttpClient);
  private readonly apiBase = inject(API_BASE);

  listar() {
    return this.http.get<Beneficiario[]>(
      `${this.apiBase}/beneficiarios`
    );
  }

  criar(beneficiario: CriarBeneficiario) {
    return this.http.post<Beneficiario>(
      `${this.apiBase}/beneficiarios`,
      beneficiario
    );
  }
}