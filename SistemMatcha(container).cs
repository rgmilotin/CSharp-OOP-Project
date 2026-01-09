namespace ConsoleApp5;
using System.Text.Json.Serialization;

public class SistemMatcha
{
    public List<Matcherie> Magazine { get; set; } = new();
    public List<Client> Clienti { get; set; } = new();
    public List<AdministratorMatcha> Administratori { get; set; } = new();
    
    public List<TipRezervare> TipuriRezervari { get; set; } = new();

    public SistemMatcha() { } // pt Json

    [JsonConstructor]
    public SistemMatcha(List<Matcherie> magazine, List<Client> clienti, List<AdministratorMatcha> administratori,  List<TipRezervare> tipurirezervari)
    {
        Magazine = magazine ?? new();
        Clienti = clienti ?? new();
        Administratori = administratori ?? new();
        TipuriRezervari =  tipurirezervari ?? new List<TipRezervare>();
    }
    
}