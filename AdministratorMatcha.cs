using Spectre.Console;
using System.Text.Json.Serialization;
namespace ConsoleApp5;

public class AdministratorMatcha
{
    public string Nume { get; private set; }
    public string AdminId { get; private set; }
    public string Parola { get; private set; }
    public List<Matcherie>  Matcherii { get; private set; }
    [JsonConstructor]
    public AdministratorMatcha(string nume, string adminId, string parola, List<Matcherie> matcherii)
    {
        this.Nume = nume;
        this.AdminId = adminId;
        this.Parola = parola;
        Matcherii = matcherii;
    }

    public bool creazaMatcherie(Matcherie m)// verificam in main cu un while(!creeazaMatcherie)
    {
        if (!Matcherii.Contains(m))
        {
            Matcherii.Add(m);
            AnsiConsole.MarkupLine("[green]succes:[/] Matcheria [blue]{0}[/] a fost adăugată.", m.Nume);
            return true;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]eroare:[/] O matcherie cu numele [yellow]{0}[/] există deja în rețea!", m.Nume);
            return false;
        }
        
    }

    public bool modificaMatcherie(string numeCautat)
    {
        Matcherie magazin = null;
        foreach (var m in Matcherii)
        {
            if (m.Nume == numeCautat) magazin = m;
        }
        if (magazin == null)
        {
            AnsiConsole.MarkupLine("[red]Eroare:[/] Magazinul nu a fost găsit!");
            return false;
        }

        // 3. Afișăm meniul de selecție
        var optiune = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Ce dorești să modifici la [green]{magazin.Nume}[/]?")
                .AddChoices(new[] { "Program", "Capacitate", "Anulează" }));

        // 4. Aplicăm modificarea în funcție de alegere
        if (optiune == "Program")
        {
            string noulProgram = AnsiConsole.Ask<string>("Introdu noul program (ex: 08:00-20:00):");
            magazin.SetProgram(noulProgram); 
            AnsiConsole.MarkupLine("[green]Programul a fost actualizat![/]");
        }
        else if (optiune == "Capacitate")
        {
            int nouaCapacitate = AnsiConsole.Ask<int>("Introdu noua capacitate:");
            magazin.SetCapacitate(nouaCapacitate);
            AnsiConsole.MarkupLine("[green]Capacitatea a fost actualizată![/]");
        }
        return true;
    }

    public void stergeMatcherie(Matcherie m)
    {
        if (Matcherii.Contains(m))
        {
            Matcherii.Remove(m);
            AnsiConsole.MarkupLine("[green]succes:[/] Matcheria [blue]{0}[/] a fost stearsa.", m.Nume);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]eroare: [/] Matcheria [blue]{0}[/] nu exista. ", m.Nume);
        }
    }

    public Tranzactie vindeMatcha(Matcherie m, decimal sumaPlatita)//probabil pus ca argument restuarantul la care se face
    {
        string idTemp = Guid.NewGuid().ToString();
        var TranzactieTemp= new Tranzactie(idTemp, DateTime.Now,  sumaPlatita, m);
        return TranzactieTemp;
    }

    public Rezervare creazaRezervare(Matcherie m, Client c) //probabil pus ca argument restaurantul la care se face
    {
        AnsiConsole.MarkupLine("[yellow]Rezervare nouă pentru:[/] " + m.Nume);
        string tip = AnsiConsole.Ask<string>("Introduceți tipul rezervării:");
        decimal pret = AnsiConsole.Ask<decimal>("Introduceți prețul rezervării:");
        string limitari = AnsiConsole.Ask<string>("Introduceti limitările rezervării: ");
        string beneficii = AnsiConsole.Ask<string>("Introduceti beneficiile rezervării: ");
        Rezervare nouaRezervare = new Rezervare(tip, pret, limitari, beneficii, c.Nume,m);
        AnsiConsole.MarkupLine("[bold green]Succes:[/] Rezervarea pentru [blue]{0}[/] a fost configurată!", m.Nume);
        return nouaRezervare;
    }

    public string informatii()// returneaza/afiseaza informatii despre fiecare restaurant al adminului
    {
        var tabel = new Table();
        tabel.Title("[bold magenta] Reteaua mea de matcherii[/]");
        tabel.AddColumn("Nume");
        tabel.AddColumn("Program");
        tabel.AddColumn("Capacitate");

        foreach (var m in Matcherii)
        {
            tabel.AddRow(m.Nume, m.Program, m.Capacitate.ToString());
        }

        AnsiConsole.Write(tabel);
        return "Raport generat cu succes.";   
    }
}