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
        var dados = new BeneficiarioRequestDados(requisicao.NomeCompleto, requisicao.Cpf, requisicao.DataNascimento, requisicao.PlanoId);

        var beneficiario = await servico.CriarAsync(dados, cancellationToken);

        return CreatedAtAction(nameof(Obter), new { id = beneficiario.Id }, BeneficiarioResponse.De(beneficiario));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BeneficiarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beneficiario = await servico.ObterAsync(id, cancellationToken);

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
        var dados = new BeneficiarioAtualizacaoDados(
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
        await servico.ExcluirAsync(id, cancellationToken);

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
        var (dados, total) = await servico.ListarAsync(pagina, tamanho, status, planoId, cancellationToken);

        return Ok(new
        {
            dados = dados.Select(BeneficiarioResponse.De).ToList(),
            pagina,
            tamanho,
            total
        });
    }
}