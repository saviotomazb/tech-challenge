namespace Desafio.Api.Dominio;

public enum StatusBeneficiario
{
    ATIVO,
    INATIVO
}

public class Beneficiario
{

    private Beneficiario()
    {
    }

    public Beneficiario(
        string? nomeCompleto,
        string? cpf,
        DateOnly dataNascimento,
        Guid planoId)
    {
        Id = Guid.NewGuid();
        DataCadastro = DateTime.UtcNow;
        Status = StatusBeneficiario.ATIVO;

        DefinirDados(nomeCompleto, cpf, dataNascimento, planoId);
    }

    public Guid Id { get; set; }

    public string NomeCompleto { get; set; } = null!;

    public string Cpf { get; set; } = null!;

    public DateOnly DataNascimento { get; set; }

    public StatusBeneficiario Status { get; set; }

    public Guid PlanoId { get; set; }

    public Plano? Plano { get; set; }

    public DateTime DataCadastro { get; set; }

    public void DefinirDados(
        string? nomeCompleto,
        string? cpf,
        DateOnly dataNascimento,
        Guid planoId)
    {
        nomeCompleto = nomeCompleto?.Trim() ?? string.Empty;
        cpf = cpf?.Trim() ?? string.Empty;

        var detalhes = new List<DetalheErro>();

        if (nomeCompleto.Length == 0)
        {
            detalhes.Add(new DetalheErro("nome_completo", "obrigatorio"));
        }

        if (cpf.Length == 0)
        {
            detalhes.Add(new DetalheErro("cpf", "obrigatorio"));
        }
        else if (!CpfValido(cpf))
        {
            detalhes.Add(new DetalheErro("cpf", "invalido"));
        }

        if (dataNascimento > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            detalhes.Add(new DetalheErro("data_nascimento", "futuro"));
        }

        if (planoId == Guid.Empty)
        {
            detalhes.Add(new DetalheErro("plano_id", "obrigatorio"));
        }

        if (detalhes.Count > 0)
        {
            throw new ValidacaoException(
                "Dados do beneficiário inválidos",
                detalhes);
        }

        NomeCompleto = nomeCompleto;
        Cpf = cpf;
        DataNascimento = dataNascimento;
        PlanoId = planoId;
    }

    public void DefinirStatus(StatusBeneficiario status)
    {
        Status = status;
    }

    private static bool CpfValido(string cpf)
    {
        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
        {
            return false;
        }

        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var primeiroDigito = CalcularDigito(cpf[..9]);

        if (primeiroDigito != cpf[9] - '0')
        {
            return false;
        }

        var segundoDigito = CalcularDigito(cpf[..10]);

        return segundoDigito == cpf[10] - '0';
    }

    private static int CalcularDigito(string cpfParcial)
    {
        var soma = 0;
        var peso = cpfParcial.Length + 1;

        foreach (var caractere in cpfParcial)
        {
            soma += (caractere - '0') * peso;
            peso--;
        }

        var resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }
}
