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

    public bool creazaMatcherie(Matcherie m, List <Matcherie> ListaGlobala)// verificam in main cu un while(!creeazaMatcherie)
    {
        // 1. Verificăm dacă există deja în TOATĂ rețeaua, nu doar la mine
        foreach (var MatcherieGlobala in ListaGlobala)
        {
            if (MatcherieGlobala.Nume.Equals(m.Nume, StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine($"[red]Eroare:[/] O matcherie cu numele [yellow]{m.Nume}[/] există deja în sistem!");
                return false;
            }
        }
        // 2. Adăugăm în lista adminului (proprietate)
        this.Matcherii.Add(m);
        // 3. Adăugăm în lista globală (referință) - AICI E FIX-UL
        ListaGlobala.Add(m);

        AnsiConsole.MarkupLine($"[green]Succes:[/] Matcheria [blue]{m.Nume}[/] a fost adăugată în sistem și în portofoliul tău.");
        return true;
        
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

    public void stergeMatcherie(Matcherie m, List<Matcherie> listaGlobala) // <--- Parametru NOU
    {
        // Ștergem de la admin
        if (this.Matcherii.Contains(m))
        {
            this.Matcherii.Remove(m);
        }

        // Ștergem și din global ca să nu mai apară la client
        if (listaGlobala.Contains(m))
        {
            listaGlobala.Remove(m);
            AnsiConsole.MarkupLine($"[green]Succes:[/] Matcheria [blue]{m.Nume}[/] a fost ștearsă definitiv.");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]Atenție:[/] Matcheria a fost ștearsă din lista ta, dar nu a fost găsită în sistem.");
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