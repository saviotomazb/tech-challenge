# Entrega — Sávio Tomaz

---

## 1. Resumo da entrega

Corrigi os defeitos existentes no módulo de beneficiários da aplicação e implementei as funcionalidades definidas na SPEC.md

No backend, implementei o fluxo de beneficiários, incluindo:

criação de beneficiários;
consulta por identificador;
listagem paginada;
filtros por status e plano;
atualização;
exclusão lógica;
validação de CPF;
validação do vínculo com plano;
controle de status ATIVO e INATIVO;
manutenção da unicidade do CPF;
tratamento dos códigos de resposta previstos na especificação;
integração com o padrão de camadas já utilizado pelo módulo de Planos.

Também foram realizados os ajustes necessários para que o serviço de Beneficiários fosse corretamente registrado no container de injeção de dependência, permitindo que o controller fosse instanciado da maneira correta.

No banco de dados, foram realizados os ajustes necessários para persistir as alterações do módulo, incluindo uma nova migration que incluí a coluna "ExcluidoEm" para que seja realizada a exclusão lógica do beneficiário, conforme solicitado.

No frontend, implementei a tela de beneficiários seguindo o padrão já existente no módulo de Planos. A tela é possível realizar o cadastro, edição e exclusão, além de apresentar estados de carregamento, lista vazia e mensagens de erro retornadas pela API.

---

## 2. Decisões

### 2.1 Defeitos que encontrei no código base

1. Serviço de Beneficiários não registrado no container de DI

Onde: camada de configuração da aplicação/API.
O que estava errado: o BeneficiariosController dependia de BeneficiarioServico, mas o serviço não estava registrado corretamente no container de injeção de dependência.
Como percebi: a suíte inicial apresentava diversas respostas 500 Internal Server Error. A mensagem interna indicava Unable to resolve service for type 'Desafio.Api.Aplicacao.BeneficiarioServico'.
Como corrigi: registrei o serviço de Beneficiários no program.cs seguindo o padrão utilizado pelas demais dependências da aplicação.
O que quebraria em produção: qualquer requisição ao controller de Beneficiários poderia resultar em erro 500, pois o ASP.NET Core não conseguiria criar o controller de beneficiários.

2. Funcionalidades do módulo de Beneficiários estavam incompletas

Onde: controller, aplicação e persistência de Beneficiários.
O que estava errado: o código base possuía apenas parte da funcionalidade prevista na especificação. Consulta por ID, atualização, exclusão lógica, paginação e filtros ainda precisavam ser implementados.
Como percebi: comparação entre o código existente, os testes e a SPEC.md.
Como corrigi: implementei os fluxos restantes seguindo a estrutura e os padrões já utilizados pelo módulo de Planos.
O que quebraria em produção: as rotas previstas pelo contrato da API não estariam disponíveis ou não apresentariam o comportamento esperado.

3. Listagem sem o envelope paginado definido pela especificação

Onde: endpoint GET /beneficiarios.
O que estava errado: a listagem precisava retornar os registros dentro do envelope contendo dados, pagina, tamanho e total.
Como percebi: leitura da especificação e do teste Listar_deve_devolver_envelope_paginado.
Como corrigi: adaptei a resposta da listagem para o contrato paginado definido pela SPEC.md.
O que quebraria em produção: o frontend não conseguiria interpretar corretamente metadados de paginação e filtros.

4. Unicidade do CPF não poderia depender somente de uma consulta prévia

Onde: persistência de Beneficiários.
O que estava errado: uma verificação anterior ao INSERT não seria suficiente para garantir unicidade em uma situação de concorrência.
Como percebi: a SPEC.md determina explicitamente que duas requisições simultâneas não podem criar beneficiários com o mesmo CPF.
Como corrigi: a unicidade foi garantida no banco de dados por restrição/índice único, utilizando a exceção de violação de unicidade para tratar a tentativa duplicada.
O que quebraria em produção: duas requisições simultâneas poderiam passar pela consulta de existência e inserir o mesmo CPF caso a garantia existisse somente na aplicação.

5. Exclusão precisava ser lógica

Onde: fluxo de exclusão de Beneficiários.
O que estava errado: o comportamento esperado não era remover fisicamente o registro do banco.
Como percebi: comparação com o comportamento do módulo de Planos e leitura da SPEC.md.
Como corrigi: implementei a exclusão lógica através da inclusão da coluna "ExcluidoEm", mantendo o registro no banco e retirando-o das consultas normais.
O que quebraria em produção: uma exclusão física poderia eliminar o histórico do beneficiário e permitir a reutilização indevida de dados que a especificação determina que permaneçam ocupados, como o CPF.

### 2.2 Pontos em que a especificação não definiu o comportamento

1. Ordenação padrão da listagem

O que a spec não define: a ordem padrão dos beneficiários quando nenhum critério de ordenação é informado.
O que decidi: utilizar uma ordenação estável baseada no identificador do registro.
Por quê: a própria especificação exige estabilidade da paginação, garantindo que percorrer as páginas não repita nem perca registros.
O que eu consideraria se fosse decidir diferente: utilizar data_cadastro como critério principal, adicionando o id como critério secundário para desempate.

### 2.3 Inconsistências que percebi

1. Contrato HTTP da API e modelo TypeScript

A spec/código da API diz: os campos JSON de Beneficiário são retornados em snake_case, como nome_completo, data_nascimento, plano_id e data_cadastro.
O frontend inicialmente esperava: propriedades como nomeCompleto, dataNascimento, planoId e dataCadastro.
Segui: mantive o contrato HTTP da API e adaptei o modelo/consumo no frontend.
Por quê: o contrato da API é a fonte de verdade para a comunicação entre frontend e backend. Alterar a API apenas para adequá-la ao modelo TypeScript quebraria o contrato definido pela especificação.

Durante os testes de integração, essa diferença ficou evidente quando o backend retornava corretamente os dados, mas o Angular conseguia exibir somente alguns campos.

### 2.4 Decisões técnicas

Segui o módulo de Planos como referência para organização das camadas, tratamento de erros, nomenclatura e integração.
Mantive a regra de negócio fora do controller.
Utilizei injeção de dependência para os serviços.
Mantive operações de acesso ao banco de forma assíncrona.
Utilizei migration do Entity Framework Core para refletir as alterações do modelo no banco.
No frontend, utilizei signals para o estado da tela e takeUntilDestroyed para o ciclo de vida das inscrições RxJS.
Mantive o CPF somente para cadastro na interface; durante a edição ele permanece somente leitura, conforme a especificação.

### 2.5 O que ficou de fora

A parte principal do módulo de Beneficiários foi implementada e integrada.

O frontend foi priorizado para garantir o fluxo principal de cadastro, consulta, edição e exclusão e para manter a comunicação correta com a API.

---

## 3. Uso de IA

Nível de uso: intenso

### 3.1 Ferramentas

ChatGPT: utilizado como apoio durante a leitura da especificação, análise do código existente, explicação de conceitos implementados e implementação.

### 3.2 Os 3 prompts que mais influenciaram o resultado

Os prompts utilizados durante o desenvolvimento foram usados principalmente para:

analisar a estrutura existente do projeto e entender o padrão utilizado pelo módulo de Planos antes de implementar Beneficiários;
implementar e revisar as funcionalidades do módulo de Beneficiários de acordo com a SPEC.md.

Prompt 1

``
Perfeito. Agora temos os **dois documentos fundamentais** do desafio: o `README.md`, que define o processo de entrega, e o `SPEC.md`, que define o comportamento que nossa aplicação precisa ter.

E a `SPEC.md` deixa o escopo bem mais claro. 

### O mapa do desafio

O coração do desafio é o módulo de **Beneficiários**.

O estado inicial é:

* **Health check:** pronto.
* **Planos:** pronto e deve ser usado como referência.
* **POST Beneficiários:** existe, mas possui defeitos.
* **GET Beneficiários:** existe, mas possui defeitos.
* **GET por ID:** não implementado.
* **PUT:** não implementado.
* **DELETE:** não implementado.
* **Paginação e filtros:** não implementados. 

Ou seja, nosso trabalho não é simplesmente "criar um CRUD de beneficiários". Precisamos **entender o padrão existente em Planos e fazer Beneficiários chegar ao mesmo nível**, respeitando várias regras de negócio.

### O que já sabemos que teremos que implementar

#### API

**Beneficiário**

```text id="g5p7y3"
POST   /beneficiarios
GET    /beneficiarios
GET    /beneficiarios/{id}
PUT    /beneficiarios/{id}
DELETE /beneficiarios/{id}
```

Além disso:

```text id="4r6p6r"
GET /beneficiarios
    ├── pagina
    ├── tamanho
    ├── status
    └── plano_id
```

Os filtros podem ser combinados e a resposta precisa utilizar um envelope paginado com `dados`, `pagina`, `tamanho` e `total`. 

#### Regras importantes

Já identifiquei algumas que provavelmente serão pontos fortes da avaliação:

**CPF**

* 11 dígitos.
* Somente números.
* Dígitos verificadores válidos.
* Sequências repetidas são inválidas.
* Unicidade garantida no banco mesmo sob concorrência. 

**Plano**

* Deve existir.
* Plano excluído logicamente é considerado inexistente para novos vínculos.
* Beneficiários existentes continuam vinculados ao plano excluído. 

**Status**

* Novo beneficiário nasce `ATIVO`.
* `INATIVO` é um registro congelado.
* Seus dados cadastrais não podem ser alterados enquanto estiver inativo.
* Porém, o status pode ser alterado para reativá-lo. 

**Exclusão**

Não é exclusão física.

Tanto Plano quanto Beneficiário utilizam **soft delete**, mantendo o registro no banco.  

---

### E tem uma exigência arquitetural importante

A especificação determina explicitamente:

> regra de negócio não vive no Controller.

Também exige:

* domínio isolado de framework e acesso a dados;
* DI;
* acesso ao banco assíncrono;
* logs estruturados;
* OpenAPI;
* testes. 

Isso é interessante para nós porque **vai exigir que primeiro entendamos a arquitetura que a 4Tech já criou**, em vez de simplesmente aplicar a arquitetura que usamos no Momentum.

---

## Portanto, eu mudaria um pouco nossa estratégia

Agora que temos a `SPEC.md`, **ainda não começaria a implementar**.

Nossa sequência ideal é:

```text
1. Fork
      ↓
2. Clone
      ↓
3. Branch
      ↓
4. Docker Compose
      ↓
5. Executar aplicação original
      ↓
6. dotnet test
      ↓
7. Entender arquitetura existente
      ↓
8. Estudar módulo Planos
      ↓
9. Estudar módulo Beneficiários
      ↓
10. Comparar código × SPEC × testes
      ↓
11. Criar mapa dos defeitos
      ↓
12. Corrigir/implementar API
      ↓
13. Testes
      ↓
14. Angular
      ↓
15. Docker Hub
      ↓
16. participantes/<id>
      ↓
17. verificar.sh
      ↓
18. PR
```

### E uma coisa particularmente importante

A `SPEC.md` **já nos avisou que haverá ambiguidades e possíveis contradições**:

> "Se encontrar algo omisso ou contraditório, decida, siga em frente e registre a decisão..." 

Isso significa que, durante o desenvolvimento, eu sugiro mantermos uma espécie de **"diário de decisões"**.

Por exemplo:

```text
DECISÃO #01

Problema:
A SPEC não define a ordenação padrão da listagem.

Decisão:
...

Motivo:
...
```

No final, isso alimentará diretamente a seção **Decisões** do README da entrega, que é uma parte que a 4Tech diz explicitamente ter bastante peso. 

---

**Então estamos no ponto certo agora.**

Se você já fez o **fork + clone + branch** e executou:

```bash
cd base
docker compose up --build
```

o próximo passo que eu faria é **rodar a aplicação original e a suíte de testes antes de tocar em qualquer código**.

A partir daí, começamos nossa investigação pelo **módulo Planos**, porque a própria SPEC diz que ele é o padrão da casa. 
``

O que aceitei: utilizei a análise da estrutura existente como orientação para entender o padrão do projeto antes de alterar o código.
O que descartei e por quê: descartei qualquer sugestão que criasse uma estrutura diferente da utilizada pelo módulo de Planos sem necessidade prevista na especificação.

Prompt 2

``
Sim. Agora temos uma mudança importante de arquitetura: o `BeneficiariosController` atual ainda está seguindo a implementação inicial do desafio e **precisa ser completamente adaptado ao padrão que estabelecemos para `Plano`**.

A responsabilidade do Controller será apenas:

```text
HTTP
 ↓
Contrato HTTP
 ↓
BeneficiarioServico
 ↓
Domínio / Infraestrutura
```

Ele **não deve mais conhecer `AppDbContext`**.

## 1. O que sai do Controller atual

Hoje temos:

```csharp
private readonly AppDbContext _db;
```

Isso deve desaparecer.

Também saem:

```csharp
_db.Beneficiarios.Any(...)
_db.Beneficiarios.Add(...)
_db.SaveChangesAsync()
_db.Beneficiarios.ToListAsync()
_db.Planos.FindAsync(...)
```

Tudo isso já pertence ao `BeneficiarioServico`.

---

## 2. `Criar`

O método atual recebe diretamente a entidade:

```csharp
public async Task<IActionResult> Criar([FromBody] Beneficiario beneficiario)
```

Isso também não deve acontecer.

Devemos receber:

```csharp
[FromBody] BeneficiarioRequest requisicao
```

e transformar o contrato HTTP no contrato da aplicação:

```csharp
var dados = new BeneficiarioRequestDados(
    requisicao.NomeCompleto,
    requisicao.Cpf,
    requisicao.DataNascimento,
    requisicao.PlanoId);
```

Depois:

```csharp
var beneficiario = await servico.CriarAsync(dados, cancellationToken);
```

E, assim como fizemos em `PlanosController`, devolver:

```csharp
CreatedAtAction(...)
```

Isso é importante porque um dos testes exige:

```text
201 Created
Location: /beneficiarios/{id}
```

---

## 3. `Obter`

O Controller precisa ganhar:

```http
GET /beneficiarios/{id}
```

A implementação será conceitualmente igual à de `PlanosController`:

```csharp
var beneficiario = await servico.ObterAsync(id, cancellationToken);

return Ok(BeneficiarioResponse.De(beneficiario));
```

O `NaoEncontradoException` será tratado automaticamente pelo nosso:

```text
TratamentoDeErroMiddleware
```

Portanto, o Controller não precisa fazer:

```csharp
if (beneficiario is null)
```

---

## 4. `Atualizar`

Teremos:

```http
PUT /beneficiarios/{id}
```

Recebendo:

```csharp
BeneficiarioRequestAtualizacao
```

E o Controller fará somente a transformação:

```csharp
var dados = new BeneficiarioRequestAtualizacao(
    requisicao.NomeCompleto,
    requisicao.DataNascimento,
    requisicao.PlanoId,
    requisicao.Status);
```

Depois delega:

```csharp
var beneficiario = await servico.AtualizarAsync(
    id,
    dados,
    cancellationToken);
```

E retorna:

```csharp
return Ok(BeneficiarioResponse.De(beneficiario));
```

---

## 5. `Excluir`

Teremos:

```http
DELETE /beneficiarios/{id}
```

O Controller simplesmente chama:

```csharp
await servico.ExcluirAsync(id, cancellationToken);

return NoContent();
```

A exclusão lógica já está encapsulada no domínio:

```csharp
beneficiario.Excluir();
```

---

## 6. `Listar`

Aqui temos a parte mais interessante porque precisamos atender aos testes:

```http
GET /beneficiarios
```

com:

```text
pagina
tamanho
status
plano_id
```

O Controller recebe esses parâmetros:

```csharp
[FromQuery] int pagina = 1,
[FromQuery] int tamanho = 20,
[FromQuery] StatusBeneficiario? status = null,
[FromQuery(Name = "plano_id")] Guid? planoId = null
```

E simplesmente delega:

```csharp
var (dados, total) = await servico.ListarAsync(
    pagina,
    tamanho,
    status,
    planoId,
    cancellationToken);
```

Depois montamos o envelope:

```json
{
  "dados": [],
  "pagina": 1,
  "tamanho": 20,
  "total": 25
}
```

Isso explica exatamente os erros que vimos anteriormente:

> `target element has type 'Array'`

O Controller antigo devolvia diretamente:

```json
[
    {...},
    {...}
]
```

mas a SPEC/testes esperam um **objeto contendo `dados`, `pagina`, `tamanho` e `total`**.

---

# Controller completo

Seguindo exatamente o padrão de `PlanosController`, eu deixaria o arquivo assim:

```csharp
using Desafio.Api.Api.Contratos;
using Desafio.Api.Aplicacao;
using Desafio.Api.Dominio;
using Microsoft.AspNetCore.Mvc;

namespace Desafio.Api.Controllers;

[ApiController]
[Route("beneficiarios")]
[Produces("application/json")]
public class BeneficiariosController(BeneficiarioServico servico) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<BeneficiarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar(
        [FromBody] BeneficiarioRequest requisicao,
        CancellationToken cancellationToken)
    {
        var dados = new BeneficiarioRequestDados(
            requisicao.NomeCompleto,
            requisicao.Cpf,
            requisicao.DataNascimento,
            requisicao.PlanoId);

        var beneficiario = await servico.CriarAsync(
            dados,
            cancellationToken);

        return CreatedAtAction(
            nameof(Obter),
            new { id = beneficiario.Id },
            BeneficiarioResponse.De(beneficiario));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BeneficiarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beneficiario = await servico.ObterAsync(
            id,
            cancellationToken);

        return Ok(BeneficiarioResponse.De(beneficiario));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<BeneficiarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] BeneficiarioRequestAtualizacao requisicao,
        CancellationToken cancellationToken)
    {
        var dados = new BeneficiarioRequestAtualizacao(
            requisicao.NomeCompleto,
            requisicao.DataNascimento,
            requisicao.PlanoId,
            requisicao.Status);

        var beneficiario = await servico.AtualizarAsync(
            id,
            dados,
            cancellationToken);

        return Ok(BeneficiarioResponse.De(beneficiario));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(
        Guid id,
        CancellationToken cancellationToken)
    {
        await servico.ExcluirAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        [FromQuery] StatusBeneficiario? status = null,
        [FromQuery(Name = "plano_id")] Guid? planoId = null,
        CancellationToken cancellationToken = default)
    {
        var (dados, total) = await servico.ListarAsync(
            pagina,
            tamanho,
            status,
            planoId,
            cancellationToken);

        return Ok(new
        {
            dados = dados.Select(BeneficiarioResponse.De).ToList(),
            pagina,
            tamanho,
            total
        });
    }
}
```

### Um detalhe para observar

No `Atualizar`, estamos usando o mesmo nome:

```csharp
BeneficiarioRequestAtualizacao
```

tanto no contrato HTTP quanto na aplicação.

Isso **funciona**, mas gera uma sobreposição de responsabilidades que eu prefiro evitar. Como já criamos o `BeneficiarioRequestAtualizacao` dentro de `BeneficiarioServico.cs`, temos duas classes com o mesmo nome em namespaces diferentes.

Antes de compilar, eu sugiro resolver isso com uma pequena mudança de nomenclatura no próximo passo, para manter a separação clara:

```text
Api.Contratos
    BeneficiarioRequest
    BeneficiarioRequestAtualizacao

Aplicacao
    BeneficiarioRequestDados
    BeneficiarioAtualizacaoDados
```

Ou seja, a aplicação ficaria com:

```csharp
public sealed record BeneficiarioAtualizacaoDados(...)
```

Isso deixa a arquitetura muito mais explícita:

```text
HTTP Request
     ↓
BeneficiarioRequestAtualizacao
     ↓
Controller
     ↓
BeneficiarioAtualizacaoDados
     ↓
BeneficiarioServico
```

**Eu faria essa pequena correção antes do build.** Depois disso, podemos compilar e partir para o `Program.cs`/DI, caso o `BeneficiarioServico` ainda não esteja registrado.
``

O que aceitei: utilizei as sugestões como apoio para implementar os endpoints e regras previstas na especificação.
O que descartei e por quê: validei as sugestões contra a SPEC.md, os testes e o comportamento existente do projeto antes de aplicá-las.

### 3.3 O que fiz sem IA

execução dos testes;
análise das mensagens de erro;
execução da aplicação em Docker;
validação das requisições HTTP;
conferência do payload real retornado pela API;
confirmação do comportamento de cadastro;
confirmação da listagem após o cadastro;
validação do fluxo de edição e exclusão;
avaliação das diferenças entre a especificação, os testes e o código existente.

### 3.4 O que ainda não domino

detalhes da garantia de unicidade do CPF em cenários concorrentes;
tratamento da exceção de violação de unicidade;
implementação da paginação e filtros no Entity Framework Core;
alguns detalhes do ciclo de vida das subscriptions RxJS.

---

## 4. Perguntas de compreensão


### 4.1 Concorrência

**O que acontece se duas requisições simultâneas tentarem criar beneficiários com o mesmo
CPF? Onde exatamente, na sua implementação, a unicidade é garantida?**

A unicidade do CPF não deve depender somente da consulta antes de realizar o cadastro. Duas requisições podem consultar o banco praticamente ao mesmo tempo, ambas concluírem que o CPF não existe e tentarem inserir o registro. A garantia definitiva precisa estar no banco de dados.

Na implementação, o CPF possui uma restrição de unicidade. Assim, quando duas requisições tentam inserir o mesmo CPF, somente uma consegue persistir. A outra recebe uma violação de unicidade.

### 4.2 Um defeito que você corrigiu

**Escolha um dos defeitos que encontrou no código base e explique: por que o código original
estava errado, e em que situação real ele quebraria em produção?**

Um dos problemas encontrados foi a ausência de registro funcional de BeneficiarioServico no container.

O controller recebia BeneficiarioServico como dependência, mas o ASP.NET Core não conseguia resolver essa dependência ao tentar criar BeneficiariosController.

O problema apareceu claramente na suíte inicial, que apresentava erros em 'Desafio.Api.Aplicacao.BeneficiarioServico'.

Corrigi o registro do serviço seguindo o padrão utilizado. Depois disso, o controller passou a ser criado normalmente.

Em produção, esse problema impediria o módulo de Beneficiários de funcionar corretamente, independentemente de a lógica dos endpoints estar correta.

### 4.3 O trecho mais complexo

**Escolha o trecho mais complexo que a IA gerou para você, ou o trecho mais complexo do
projeto se você não usou IA, e explique linha a linha o que ele faz.**

O trecho que considero mais complexo é a implementação da listagem com filtros.

Primeiro, a consulta base é criada sobre os beneficiários que ainda não foram excluídos logicamente. Em seguida, os filtros opcionais de status e plano_id são aplicados somente quando foram informados na requisição.

Depois disso, a aplicação calcula o total de registros que atendem aos filtros. Esse valor é diferente da quantidade de registros retornados na página.

Na sequência, Skip e Take são utilizados para obter somente os registros correspondentes à página solicitada. A consulta também precisa possuir uma ordenação estável para garantir que a navegação entre páginas não repita ou perca registros.

Por fim, os registros são convertidos para BeneficiarioResponse.