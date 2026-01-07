namespace ConsoleApp5;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

public static class GestiuneDate
{
    private static JsonSerializerOptions _optiuni = new JsonSerializerOptions
    { 
        IncludeFields = true,//include proprietățile chiar dacă au set-er privat
        WriteIndented = true, // adauga in JSON  spații și indentare
        PropertyNameCaseInsensitive = true
    };
    private static string _caleFisier = "baza_date_matcha.json";
    public static void SalveazaTot(SistemMatcha sistem)
    {
        string json = JsonSerializer.Serialize(sistem, _optiuni);
        File.WriteAllText(_caleFisier, json);
    }

    public static SistemMatcha IncarcaTot()
    {
        if (!File.Exists(_caleFisier)) 
        {
            return new SistemMatcha(new List<Matcherie>(), new List<Client>(), new List<AdministratorMatcha>());// Dacă nu există fișierul, returnăm un sistem gol pentru a nu da eroare
        }
        string json = File.ReadAllText(_caleFisier);
        return JsonSerializer.Deserialize<SistemMatcha>(json, _optiuni);
    }
}