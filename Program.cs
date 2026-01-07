using ConsoleApp5;
using Spectre.Console;

namespace ConsoleApp5
{
    class Program
    {
        // Aceasta este poarta de intrare în program
        static void Main(string[] args)
        {
            // --- 1. INIȚIALIZARE ---
            SistemMatcha sistem = GestiuneDate.IncarcaTot();

            if (sistem.Administratori.Count == 0)
            {
                IncarcaDateTest(sistem);
            }

            AnsiConsole.Write(new FigletText("Matcha System").Color(Color.Green));

            // --- 2. BUCAL PRINCIPALĂ ---
            bool ruleazaProgramul = true;
            while (ruleazaProgramul)
            {
                Console.Clear();
                var rol = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold white]Cine folosește aplicația?[/]")
                        .AddChoices(new[] { "Administrator", "Client", "Ieșire" }));

                switch (rol)
                {
                    case "Administrator":
                        RulareMeniuAdmin(sistem);
                        break;
                    case "Client":
                        RulareMeniuClient(sistem);
                        break;
                    case "Ieșire":
                        SalvareSistem(sistem);
                        ruleazaProgramul = false;
                        break;
                }
            }
        }

        // --- 3. METODELE DE LOGICĂ (STATICE) ---

        static void RulareMeniuAdmin(SistemMatcha sistem)
        {
            // Login simplificat
            var admin = sistem.Administratori[0]; 
            bool inapoi = false;
            
            while (!inapoi)
            {
                Console.Clear();
                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold red]PANOU ADMIN[/] - Salut, {admin.Nume}")
                        .AddChoices(new[] { "Vezi Raport Magazine", "Modifică Magazin", "Înapoi" }));

                if (optiune == "Vezi Raport Magazine") 
                    admin.informatii();
                else if (optiune == "Modifică Magazin") 
                {
                    var nume = AnsiConsole.Ask<string>("Numele magazinului:");
                    admin.modificaMatcherie(nume);
                }
                else inapoi = true;

                if (!inapoi) { Console.WriteLine("\nApasă o tastă..."); Console.ReadKey(); }
            }
        }

        static void RulareMeniuClient(SistemMatcha sistem)
        {
            var client = sistem.Clienti[0];
            bool inapoi = false;

            while (!inapoi)
            {
                Console.Clear();
                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold green]MENIU CLIENT[/] - Salut, {client.Nume}")
                        .AddChoices(new[] { "Vezi Restaurante", "Comandă Matcha", "Rezervă Masă", "Anulează Rezervare", "Înapoi" }));

                switch (optiune)
                {
                    case "Vezi Restaurante":
                        client.AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        break;
                    case "Comandă Matcha":
                        var numeM = client.AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        if (numeM != null) client.Comanda(numeM, sistem.Magazine, sistem.Administratori[0]);
                        break;
                    case "Rezervă Masă":
                        client.rezervaMasa(sistem.Magazine[0], sistem.Administratori[0]);
                        break;
                    case "Anulează Rezervare":
                        client.anuleazaRezervare();
                        break;
                    case "Înapoi":
                        inapoi = true;
                        break;
                }
                if (!inapoi) { Console.WriteLine("\nApasă o tastă..."); Console.ReadKey(); }
            }
        }

        static void SalvareSistem(SistemMatcha sistem)
        {
            AnsiConsole.Status().Start("Se salvează datele...", ctx => {
                GestiuneDate.SalveazaTot(sistem);
                Thread.Sleep(800);
            });
        }

        static void IncarcaDateTest(SistemMatcha sistem)
        {
            var meniu = new List<Matcha> { new Matcha("Matcha Latte", "Clasic", 22.5m, 100, 120) };
            var m1 = new Matcherie("Matcha Zen", "08-22", 20, meniu, new List<Rezervare>());
            sistem.Magazine.Add(m1);
            sistem.Administratori.Add(new AdministratorMatcha("Admin", "ADM01", "1234", sistem.Magazine));
            sistem.Clienti.Add(new Client("Andrei", "andrei@email.com", new List<Tranzactie>(), new List<Rezervare>()));
        }
    }
}