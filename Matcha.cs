namespace ConsoleApp5;
using Spectre.Console;
using System.Text.Json.Serialization;
public class Matcha
{
    [JsonInclude]
    public string nume { get; set; }
    public string descriere { get; set; }
    public decimal pret { get; set; }
    public int cantitate { get; set; }
    public int calorii { get; set; }

    public Matcha(string nume, string descriere, decimal pret, int cantitate, int calorii)
    {
        this.nume = nume;
        this.descriere = descriere;
        this.pret = pret;
        this.cantitate = cantitate;
        this.calorii = calorii;
    }
}
    
public class Matcherie
{
    public string Nume { get; private set; }
    public string Program { get; private set;  } 
    public int Capacitate { get; private set; }
    public List<Matcha> Meniu {get; private set;}
    public List<Rezervare> Rezervari {get; private set;}
    [JsonConstructor]
    public Matcherie(string nume, string program, int Capacitate, List<Matcha> meniu, List<Rezervare> rezervari)
        {
            Nume = nume;
            Program = program;
            Capacitate= Capacitate;
            Meniu = meniu;
            Rezervari=rezervari;
        }public void AfiseazaMeniu()
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .Title($"[bold white on green] MENIU {Nume.ToUpper()} [/]");
        table.AddColumn("[bold]Produs[/]");
        table.AddColumn("[bold]Descriere[/]");
        table.AddColumn(new TableColumn("[bold]Preț[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Calorii[/]").Centered());
        foreach (var item in Meniu)
        {
            table.AddRow(
                $"[green]{item.nume}[/]",
                $"[grey]{item.descriere}[/]",
                $"[yellow]{item.pret} RON[/]",
                $"{item.calorii} kcal"
            );
        }
        AnsiConsole.Write(table);
    }
    
    public void SetProgram(string noulProgram)
    {
        if (!string.IsNullOrEmpty(noulProgram))
        {
            this.Program = noulProgram;
        }
    }

    public void SetCapacitate(int nouaCapacitate)
    {
        if (nouaCapacitate > 0) 
        {
            this.Capacitate = nouaCapacitate;
        }
    }

    public bool StergeRezervare(Rezervare rezervare)
    {
        if (rezervare == null) return false;
        if (Rezervari.Contains(rezervare))
        {
            Rezervari.Remove(rezervare);
            return true;
        }

        return false;
    }
}