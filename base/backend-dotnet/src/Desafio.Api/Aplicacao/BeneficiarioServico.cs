using Desafio.Api.Dominio;
using Desafio.Api.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Desafio.Api.Aplicacao;

public class BeneficiarioServico(AppDbContext db)
{
    private const string CodigoViolacaoDeUnicidade = "23505";

    public async Task<Beneficiario> CriarAsync(
        BeneficiarioRequestDados dados,
        CancellationToken cancellationToken)
    {
        var beneficiario = new Beneficiario(
            dados.NomeCompleto,
            dados.Cpf,
            dados.DataNascimento,
            dados.PlanoId);

        var cpfJaCadastrado = await db.Beneficiarios
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                b => b.Cpf == beneficiario.Cpf,
                cancellationToken);

        if (cpfJaCadastrado)
        {
            throw new ConflitoException(
                "Já existe beneficiário cadastrado com esse CPF",
                [new DetalheErro("cpf", "duplicado")]);
        }

        var plano = await db.Planos
            .FirstOrDefaultAsync(
                p => p.Id == beneficiario.PlanoId,
                cancellationToken);

        if (plano is null)
        {
            throw new NaoProcessavelException(
                "O plano informado não existe",
                [new DetalheErro("plano_id", "inexistente")]);
        }

        db.Beneficiarios.Add(beneficiario);

        await SalvarAsync(cancellationToken);

        return beneficiario;
    }

    private async Task SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excecao) when (EhViolacaoDeUnicidade(excecao))
        {
            throw new ConflitoException(
                "Já existe beneficiário cadastrado com esse CPF",
                [new DetalheErro("cpf", "duplicado")]);
        }
    }

    public async Task<Beneficiario> ObterAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await db.Beneficiarios
            .FirstOrDefaultAsync(
                b => b.Id == id,
                cancellationToken)
            ?? throw new NaoEncontradoException(
                "Beneficiário não encontrado");
    }

    public async Task<Beneficiario> AtualizarAsync(
        Guid id,
        BeneficiarioAtualizacaoDados dados,
        CancellationToken cancellationToken)
    {
        var beneficiario = await ObterAsync(id, cancellationToken);

        var plano = await db.Planos
            .FirstOrDefaultAsync(
                p => p.Id == dados.PlanoId,
                cancellationToken);

        if (plano is null)
        {
            throw new NaoProcessavelException(
                "O plano informado não existe",
                [new DetalheErro("plano_id", "inexistente")]);
        }

        beneficiario.DefinirDados(
            dados.NomeCompleto,
            beneficiario.Cpf,
            dados.DataNascimento,
            dados.PlanoId);

        beneficiario.DefinirStatus(dados.Status);

        await SalvarAsync(cancellationToken);

        return beneficiario;
    }

    public async Task ExcluirAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beneficiario = await ObterAsync(id, cancellationToken);

        beneficiario.Excluir();

        await SalvarAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Beneficiario> Dados, int Total)> ListarAsync(
        int pagina,
        int tamanho,
        StatusBeneficiario? status,
        Guid? planoId,
        CancellationToken cancellationToken)
    {
        var consulta = db.Beneficiarios
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            consulta = consulta.Where(
                b => b.Status == status.Value);
        }

        if (planoId.HasValue)
        {
            consulta = consulta.Where(
                b => b.PlanoId == planoId.Value);
        }

        var total = await consulta.CountAsync(cancellationToken);

        var dados = await consulta
            .OrderBy(b => b.NomeCompleto)
            .ThenBy(b => b.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        return (dados, total);
    }

    private static bool EhViolacaoDeUnicidade(DbUpdateException excecao) =>
        excecao.InnerException is PostgresException postgres &&
        postgres.SqlState == CodigoViolacaoDeUnicidade;
}

public sealed record BeneficiarioRequestDados(string? NomeCompleto, string? Cpf, DateOnly DataNascimento, Guid PlanoId); 

public sealed record BeneficiarioAtualizacaoDados(string? NomeCompleto, DateOnly DataNascimento, Guid PlanoId, StatusBeneficiario Status);