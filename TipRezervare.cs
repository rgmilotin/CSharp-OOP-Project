namespace ConsoleApp5;
using System.Text.Json.Serialization;

public class TipRezervare
{
    public string Nume { get; set; }
    public decimal Pret { get; set; }
    public string Limitari { get; set; }
    public string Beneficii { get; set; }

    [JsonConstructor]
    public TipRezervare(string nume, decimal pret, string limitari, string beneficii)
    {
        Nume = nume;
        Pret = pret;
        Limitari = limitari;
        Beneficii = beneficii;
    }

    public override string ToString() => $"{Nume} - {Pret} RON";
}