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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            // Această linie permite salvarea obiectelor care se referă unul la altul
            ReferenceHandler = ReferenceHandler.IgnoreCycles 
        };

        string jsonString = JsonSerializer.Serialize(sistem, options);
        File.WriteAllText("baza_date_matcha.json", jsonString);
    }

    public static SistemMatcha IncarcaTot()
    {
        if (!File.Exists("baza_date_matcha.json")) return new SistemMatcha();

        try
        {
            string jsonString = File.ReadAllText("baza_date_matcha.json");
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<SistemMatcha>(jsonString, options) ?? new SistemMatcha();
        }
        catch
        {
            // fișier invalid → pornești cu bază goală (nu crăpă aplicația)
            return new SistemMatcha();
        }
    }
}