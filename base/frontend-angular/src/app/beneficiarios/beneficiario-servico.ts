import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE } from '../nucleo/api';
import { Beneficiario } from './beneficiario';

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
}