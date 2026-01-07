using System.Transactions;
using Spectre.Console;
namespace ConsoleApp5;
using Spectre.Console;
using System.Text.Json.Serialization;
public class Client
{
    public string Nume{get; private set;}
    public string Email{get;private set;}
    public List<Tranzactie> Istoric { get; private set; }
    public  List<Rezervare> Rezervari { get; private set; }
    [JsonConstructor]
    public Client(string nume, string email, List<Tranzactie> istoric, List<Rezervare> rezervari)
    {
        Nume = nume;
        Email = email;
        Istoric = istoric;
        Rezervari = rezervari;
    }
    public void VeziIstoric ()
    {
        foreach (var var in Istoric)
        {
            Console.WriteLine(var);            
        }
    }
    //public void AlegeRestaurant()//aici mai bine aveam o metoda 
    public string AfiseazaRestauranteSiAlegeUnul(List<Matcherie> restaurante)
    {
        if (restaurante == null || restaurante.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Ne pare rău, momentan nu există matcherii deschise.[/]");
            return null;
        }
        var tabel = new Table();
        tabel.Border(TableBorder.Rounded);
        tabel.Title("[bold white on olive] REȚEAUA NOASTRĂ MATCHA [/]");
        tabel.Caption("[grey]Alege locația preferată pentru o experiență zen[/]");
        tabel.AddColumn("[bold green]Locație[/]");
        tabel.AddColumn("[bold yellow]Program Funcționare[/]");
        tabel.AddColumn(new TableColumn("[bold blue]Capacitate Maximă[/]").Centered());
        
        foreach (var m in restaurante)
        {
            tabel.AddRow(
                $"[white]{m.Nume}[/]", 
                $"[grey]{m.Program}[/]", 
                $"[blue]{m.Capacitate}[/] locuri"
            );
        }
        AnsiConsole.Write(tabel);
        var selectie = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Selectează magazinul unde dorești să mergi:")
                .PageSize(10)
                .AddChoices(restaurante.Select(r => r.Nume)));
        AnsiConsole.MarkupLine($"Ai ales să vizitezi: [bold green]{selectie}[/]");
        return selectie;
    }

    public void Comanda(string numeMagazin,List<Matcherie> restaurante, AdministratorMatcha admin)
    {
        Matcherie magazinSelectat = null;
        foreach (var r in restaurante)
        {
            if (r.Nume == numeMagazin)
            {
                magazinSelectat = r;
                break;             
            }
        }
        if (magazinSelectat == null)
        {
            AnsiConsole.MarkupLine("[red]Eroare: Magazinul nu a fost găsit![/]");
            return;
        }
        
        if (magazinSelectat.Meniu == null || magazinSelectat.Meniu.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Acest magazin nu are produse disponibile momentan.[/]");
            return;
        }
        var produsAlesNume = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Ce dorești să comanzi de la [green]{numeMagazin}[/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Mută în sus și în jos pentru a vedea tot meniul)[/]")
                .AddChoices(magazinSelectat.Meniu.Select(p => p.nume)));//=> ia din fiecare clasa Matcha proprietate nume
        
        var produsAles = magazinSelectat.Meniu.First(p => p.nume == produsAlesNume);

        if (AnsiConsole.Confirm($"Confirmi comanda pentru [teal]{produsAles.nume}[/] la prețul de [yellow]{produsAles.pret} RON[/]?"))
        {
            Tranzactie t = admin.vindeMatcha(magazinSelectat, produsAles.pret);
            this.Istoric.Add(t);
            AnsiConsole.MarkupLine("[bold green]Bucură-te de Matcha tău![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Comandă anulată.[/]");
        }
    }
    public Rezervare rezervaMasa(Matcherie m, AdministratorMatcha admin)//poate pus ca paramentru restuarantul pentru care se rezerva masa
    {
        if (m.Capacitate <= 0)
        {
            AnsiConsole.MarkupLine("[red]Ne pare rău, magazinul este plin![/]");
            return null;
        }
        Rezervare nouaRezervare = admin.creazaRezervare(m, this);
        nouaRezervare.SetClientID(this.Nume);
        m.Rezervari.Add(nouaRezervare);
        this.Rezervari.Add(nouaRezervare);
        return nouaRezervare;
    }

    public bool anuleazaRezervare() 
    {
        if (Rezervari == null || Rezervari.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă de anulat.[/]");
            return false;
        }

        var rezervareDeAnulatTip = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Alege rezervarea pe care dorești să o [red]anulezi[/]:")
                .PageSize(10)
                .AddChoices(Rezervari.Select(r => r.Tip)));

        Rezervare rezervareDeSters = null;
        foreach (var r in Rezervari)
        {
            if (r.Tip == rezervareDeAnulatTip)
            {
                rezervareDeSters = r;
                break;
            }
        }

        if (rezervareDeSters != null)
        {
            if (!AnsiConsole.Confirm($"Ești sigur că vrei să anulezi rezervarea: [teal]{rezervareDeSters.Tip}[/]?"))
            {
                AnsiConsole.MarkupLine("[grey]Anulare abandonată.[/]");
                return false;
            }
            
            var magazinul = rezervareDeSters.Matcherie;
        
            if (magazinul != null)
            {
                magazinul.StergeRezervare(rezervareDeSters);
            }
            
            Rezervari.Remove(rezervareDeSters);
            AnsiConsole.MarkupLine($"[green]Succes:[/] Rezervarea [white]{rezervareDeSters.Tip}[/] a fost eliminată!");
            return true;
        }
        return false;
    }
}