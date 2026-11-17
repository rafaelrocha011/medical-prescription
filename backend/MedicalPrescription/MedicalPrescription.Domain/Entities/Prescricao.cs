using MedicalPrescription.Domain.Exceptions;

using MedicalPrescription.Domain.Enums;

namespace MedicalPrescription.Domain.Entities;

public class Prescricao
{
    private readonly List<PrescricaoItem> _itens = new();

    public Guid Id { get; private set; }
    public Guid PacienteId { get; private set; }
    public TipoPrescricao Tipo { get; private set; }
    public string? TextoLivre { get; private set; }
    public DateTime CriadaEmUtc { get; private set; }

    public IReadOnlyCollection<PrescricaoItem> Itens =>
        _itens.AsReadOnly();

    private Prescricao() { }

    private Prescricao(Guid pacienteId, TipoPrescricao tipo)
    {
        if (pacienteId == Guid.Empty)
            throw new ArgumentException("Paciente inválido.");

        Id = Guid.NewGuid();
        PacienteId = pacienteId;
        Tipo = tipo;
        CriadaEmUtc = DateTime.UtcNow;
    }

    public static Prescricao CriarNaoEstruturada(
        Guid pacienteId,
        string textoLivre)
    {
        if (string.IsNullOrWhiteSpace(textoLivre))
            throw new ArgumentException(
                "O texto da prescrição é obrigatório.");

        return new Prescricao(
            pacienteId,
            TipoPrescricao.NaoEstruturada)
        {
            TextoLivre = textoLivre.Trim()
        };
    }

    public static Prescricao CriarEstruturada(
        Guid pacienteId,
        IEnumerable<PrescricaoItem> itens)
    {
        if (itens is null)
            throw new ArgumentNullException(nameof(itens));

        var prescricao = new Prescricao(
            pacienteId,
            TipoPrescricao.Estruturada);

        foreach (var item in itens)
            prescricao.AdicionarItem(item);

        if (prescricao._itens.Count == 0)
            throw new ArgumentException(
                "A prescrição deve conter pelo menos um medicamento.");

        return prescricao;
    }

    public void AtualizarTextoLivre(string textoLivre)
    {
        if (Tipo != TipoPrescricao.NaoEstruturada)
            throw new RegraNegocioException(
                "A prescrição não permite texto livre.");

        if (string.IsNullOrWhiteSpace(textoLivre))
            throw new ArgumentException(
                "O texto da prescrição é obrigatório.");

        TextoLivre = textoLivre.Trim();
    }

    public void AdicionarItem(PrescricaoItem item)
    {
        if (Tipo != TipoPrescricao.Estruturada)
            throw new RegraNegocioException(
                "Não é possível adicionar itens a uma prescrição não estruturada.");

        ArgumentNullException.ThrowIfNull(item);

        if (_itens.Any(i => i.Id == item.Id))
            throw new RegraNegocioException(
                "O item já pertence à prescrição.");

        item.VincularPrescricao(Id);
        _itens.Add(item);
    }

    public void SubstituirItens(IEnumerable<PrescricaoItem> itens)
    {
        if (Tipo != TipoPrescricao.Estruturada)
            throw new RegraNegocioException(
                "A prescricao nao e estruturada.");

        ArgumentNullException.ThrowIfNull(itens);

        var novosItens = itens.ToList();

        if (novosItens.Count == 0)
            throw new ArgumentException(
                "Informe pelo menos um medicamento.");

        if (novosItens.Any(item => item is null))
            throw new ArgumentException(
                "A lista contem um item invalido.");

        if (novosItens.Select(item => item.Id).Distinct().Count()
            != novosItens.Count)
            throw new ArgumentException(
                "A lista contem itens duplicados.");

        // Validar todos os vinculos antes de alterar qualquer item.
        if (novosItens.Any(item =>
                item.PrescricaoId != Guid.Empty &&
                item.PrescricaoId != Id))
            throw new RegraNegocioException(
                "Um item ja pertence a outra prescricao.");

        foreach (var item in novosItens)
            item.VincularPrescricao(Id);

        _itens.Clear();
        _itens.AddRange(novosItens);
    }
    public void RemoverItem(Guid itemId)
    {
        if (Tipo != TipoPrescricao.Estruturada)
            throw new RegraNegocioException(
                "A prescrição não é estruturada.");

        var item = _itens.FirstOrDefault(i => i.Id == itemId);

        if (item is null)
            throw new RegraNegocioException(
                "Item não encontrado.");

        if (_itens.Count == 1)
            throw new RegraNegocioException(
                "Uma prescrição estruturada deve conter pelo menos um medicamento.");

        _itens.Remove(item);
    }
}

