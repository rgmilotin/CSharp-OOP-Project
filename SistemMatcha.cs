namespace ConsoleApp5;
using System.Text.Json.Serialization;

public class SistemMatcha
{
    public List<Matcherie> Magazine { get; set; } = new();
    public List<ClientAccount> Clienti { get; set; } = new();
    public List<AdminAccount> Administratori { get; set; } = new();
    public List<TipRezervare> TipuriRezervari { get; set; } = new();

    public SistemMatcha() { }

    [JsonConstructor]
    public SistemMatcha(List<Matcherie> magazine, List<ClientAccount> clienti, List<AdminAccount> administratori, List<TipRezervare> tipurirezervari)
    {
        Magazine = magazine ?? new();
        Clienti = clienti ?? new();
        Administratori = administratori ?? new();
        TipuriRezervari = tipurirezervari ?? new();
    }
}