namespace ConsoleApp5;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
public class SistemMatcha
{
    public List<Matcherie> Magazine { get; set; }
    public List<Client> Clienti { get; set; }
    public List<AdministratorMatcha> Administratori { get; set; }

    [JsonConstructor]
    public SistemMatcha(List<Matcherie> magazine, List<Client> clienti, List<AdministratorMatcha> administratori)
    {
        Magazine = magazine;
        Clienti = clienti;
        Administratori = administratori;
    }
}