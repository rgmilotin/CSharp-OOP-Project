using System.Runtime.CompilerServices;
using ConsoleApp5;
using Spectre.Console;
namespace ConsoleApp5
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            SistemMatcha sistem = GestiuneDate.IncarcaTot();//INIȚIALIZARE
            sistem.Magazine ??= new List<Matcherie>();
            sistem.Clienti ??= new List<Client>();
            sistem.Administratori ??= new List<AdministratorMatcha>();
            if (sistem.Administratori.Count == 0)
                
            {
                IncarcaDateTest(sistem);
            }

            AnsiConsole.Write(new FigletText("Matcha System").Color(Color.Green));

            // --- 2. BUCAL PRINCIPALĂ ---
            bool ruleazaProgramul = true;
            while (ruleazaProgramul)
            {
                var rol = AfiseazaEcranStartSiAlegeRol(sistem);

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
            var admin = LoginAdmin(sistem);
            if (admin == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();
                AfiseazaDashboardAdmin(admin, sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold red]PANOU ADMIN[/] - Salut, [white]{Markup.Escape(admin.Nume)}[/]")
                        .AddChoices(new[]
                        {
                            "1) Administrare Matcherii (CRUD)",
                            "2) Tipuri Rezervări (CRUD)",
                            "3) Tranzacții (creare/modificare/asociere client)",
                            "4) Monitorizare activitate",
                            "Deconectare"
                        }));

                switch (optiune)
                {
                    case "1) Administrare Matcherii (CRUD)":
                        SubmeniuMatcherii(admin, sistem);
                        break;

                    case "2) Tipuri Rezervări (CRUD)":
                        SubmeniuTipuriRezervari(sistem);
                        break;

                    case "3) Tranzacții (creare/modificare/asociere client)":
                        SubmeniuTranzactii(sistem);
                        break;

                    case "4) Monitorizare activitate":
                        AfiseazaMonitorizare(sistem);
                        Pauza();
                        break;

                    case "Deconectare":
                        inapoi = true;
                        break;
                }
            }

        }
        
        // -------------------- ADMIN HELPERS --------------------

        static AdministratorMatcha? LoginAdmin(SistemMatcha sistem)
        {
            if (sistem.Administratori == null || sistem.Administratori.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există administratori în sistem.[/]");
                Pauza();
                return null;
            }

            for (int incercari = 0; incercari < 3; incercari++)
            {
                Console.Clear();
                AnsiConsole.Write(new Rule("[red]Autentificare Administrator[/]").RuleStyle("grey"));

                string id = AnsiConsole.Ask<string>("Admin ID:");
                string parola = AnsiConsole.Prompt(new TextPrompt<string>("Parola:").Secret());

                foreach (var a in sistem.Administratori)
                {
                    if (a.AdminId == id && a.Parola == parola)
                    {
                        AnsiConsole.MarkupLine("[green]Autentificare reușită![/]");
                        Thread.Sleep(300);
                        return a;
                    }
                }

                AnsiConsole.MarkupLine("[red]Date invalide. Mai încearcă.[/]");
                Thread.Sleep(600);
            }

            AnsiConsole.MarkupLine("[red]Prea multe încercări. Revenire la meniu.[/]");
            Pauza();
            return null;
        }

        static void AfiseazaDashboardAdmin(AdministratorMatcha admin, SistemMatcha sistem)
        {
            int nrMagazine = sistem.Magazine?.Count ?? 0;
            int nrClienti = sistem.Clienti?.Count ?? 0;

            int rezervariActive = 0;
            if (sistem.Magazine != null)
            {
                foreach (var m in sistem.Magazine)
                    rezervariActive += (m.Rezervari?.Count ?? 0);
            }

            int tranzactii = 0;
            if (sistem.Clienti != null)
            {
                foreach (var c in sistem.Clienti)
                    tranzactii += (c.Istoric?.Count ?? 0);
            }

            var panel = new Panel(
                new Rows(
                    new Markup($"[bold]Admin:[/] {Markup.Escape(admin.Nume)} ([grey]{Markup.Escape(admin.AdminId)}[/])"),
                    new Markup($"[bold]Magazine:[/] {nrMagazine}"),
                    new Markup($"[bold]Clienți:[/] {nrClienti}"),
                    new Markup($"[bold]Rezervări active:[/] {rezervariActive}"),
                    new Markup($"[bold]Tranzacții totale:[/] {tranzactii}")
                ))
                .Header("[bold red]📌 DASHBOARD ADMIN[/]")
                .Expand();

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }

        static void SubmeniuMatcherii(AdministratorMatcha admin, SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Administrare Matcherii[/]")
                        .AddChoices(new[]
                        {
                            "Vezi raport matcherii",
                            "Creează matcherie",
                            "Modifică matcherie (program/capacitate)",
                            "Șterge matcherie",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Vezi raport matcherii":
                        Console.Clear();
                        admin.informatii();
                        Pauza();
                        break;

                    case "Creează matcherie":
                        CreeazaMatcherie(admin, sistem);
                        Pauza();
                        break;

                    case "Modifică matcherie (program/capacitate)":
                        {
                            string nume = AnsiConsole.Ask<string>("Numele matcheriei:");
                            admin.modificaMatcherie(nume);
                            Pauza();
                            break;
                        }

                    case "Șterge matcherie":
                        StergeMatcherie(admin, sistem);
                        Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        static void CreeazaMatcherie(AdministratorMatcha admin, SistemMatcha sistem)
        {
            string nume = AnsiConsole.Ask<string>("Nume matcherie:");
            string program = AnsiConsole.Ask<string>("Program (ex: 08:00-22:00):");
            int capacitate = AnsiConsole.Ask<int>("Capacitate:");

            var m = new Matcherie(nume, program, capacitate, new List<Matcha>(), new List<Rezervare>());
            admin.creazaMatcherie(m, sistem.Magazine);
        }

        static void StergeMatcherie(AdministratorMatcha admin, SistemMatcha sistem)
        {
            if (admin.Matcherii == null || admin.Matcherii.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există matcherii.[/]");
                return;
            }

            var numeList = new List<string>();
            foreach (var x in admin.Matcherii) numeList.Add(x.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege matcheria de șters:")
                    .AddChoices(numeList));

            Matcherie? target = null;
            foreach (var m in admin.Matcherii)
                if (m.Nume == ales) { target = m; break; }

            if (target != null) admin.stergeMatcherie(target, sistem.Magazine);
        }

        // -------------------- TIPURI REZERVARI (ADMIN) --------------------

        static void SubmeniuTipuriRezervari(SistemMatcha sistem)
        {
            sistem.TipuriRezervari ??= new List<TipRezervare>();

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Tipuri Rezervări (admin)[/]")
                        .AddChoices(new[]
                        {
                            "Listă tipuri",
                            "Adaugă tip",
                            "Modifică tip",
                            "Șterge tip",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Listă tipuri":
                        AfiseazaTipuriRezervari(sistem);
                        Pauza();
                        break;

                    case "Adaugă tip":
                        AdaugaTipRezervare(sistem);
                        Pauza();
                        break;

                    case "Modifică tip":
                        ModificaTipRezervare(sistem);
                        Pauza();
                        break;

                    case "Șterge tip":
                        StergeTipRezervare(sistem);
                        Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        static void AfiseazaTipuriRezervari(SistemMatcha sistem)
        {
            Console.Clear();

            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri definite.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Tipuri rezervări[/]");
            t.AddColumn("Nume");
            t.AddColumn(new TableColumn("Preț").RightAligned());
            t.AddColumn("Limitări");
            t.AddColumn("Beneficii");

            foreach (var tr in sistem.TipuriRezervari)
            {
                t.AddRow(
                    Markup.Escape(tr.Nume),
                    $"{tr.Pret} RON",
                    Markup.Escape(tr.Limitari),
                    Markup.Escape(tr.Beneficii)
                );
            }

            AnsiConsole.Write(t);
        }

        static void AdaugaTipRezervare(SistemMatcha sistem)
        {
            string nume = AnsiConsole.Ask<string>("Nume tip (ex: Familie, Prieteni):");
            decimal pret = AnsiConsole.Ask<decimal>("Preț:");
            string lim = AnsiConsole.Ask<string>("Limitări:");
            string ben = AnsiConsole.Ask<string>("Beneficii:");

            foreach (var x in sistem.TipuriRezervari)
            {
                if (x.Nume == nume)
                {
                    AnsiConsole.MarkupLine("[red]Există deja un tip cu acest nume.[/]");
                    return;
                }
            }

            sistem.TipuriRezervari.Add(new TipRezervare(nume, pret, lim, ben));
            AnsiConsole.MarkupLine("[green]Tip adăugat.[/]");
        }

        static void ModificaTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri de modificat.[/]");
                return;
            }

            var numeList = new List<string>();
            foreach (var x in sistem.TipuriRezervari) numeList.Add(x.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege tipul de modificat:")
                    .AddChoices(numeList));

            TipRezervare? tip = null;
            foreach (var x in sistem.TipuriRezervari)
                if (x.Nume == ales) { tip = x; break; }

            if (tip == null) return;

            tip.Pret = AnsiConsole.Ask<decimal>($"Preț nou (curent {tip.Pret}):");
            tip.Limitari = AnsiConsole.Ask<string>($"Limitări noi (curent: {tip.Limitari}):");
            tip.Beneficii = AnsiConsole.Ask<string>($"Beneficii noi (curent: {tip.Beneficii}):");

            AnsiConsole.MarkupLine("[green]Tip modificat.[/]");
        }

        static void StergeTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri de șters.[/]");
                return;
            }

            var numeList = new List<string>();
            foreach (var x in sistem.TipuriRezervari) numeList.Add(x.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege tipul de șters:")
                    .AddChoices(numeList));

            TipRezervare? tip = null;
            foreach (var x in sistem.TipuriRezervari)
                if (x.Nume == ales) { tip = x; break; }

            if (tip != null)
            {
                sistem.TipuriRezervari.Remove(tip);
                AnsiConsole.MarkupLine("[green]Tip șters.[/]");
            }
        }

        // -------------------- TRANZACTII (ADMIN) --------------------

        static void SubmeniuTranzactii(SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Tranzacții (admin)[/]")
                        .AddChoices(new[]
                        {
                            "Vezi toate tranzacțiile (din istoricul clienților)",
                            "Creează tranzacție pentru un client",
                            "Modifică o tranzacție (înlocuire)",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Vezi toate tranzacțiile (din istoricul clienților)":
                        AfiseazaToateTranzactiile(sistem);
                        Pauza();
                        break;

                    case "Creează tranzacție pentru un client":
                        CreeazaTranzactiePentruClient(sistem);
                        Pauza();
                        break;

                    case "Modifică o tranzacție (înlocuire)":
                        ModificaTranzactiePentruClient(sistem);
                        Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        static void AfiseazaToateTranzactiile(SistemMatcha sistem)
        {
            Console.Clear();

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Toate tranzacțiile[/]");
            table.AddColumn("Client");
            table.AddColumn("Dată");
            table.AddColumn("Magazin");
            table.AddColumn(new TableColumn("Sumă").RightAligned());

            int count = 0;

            if (sistem.Clienti != null)
            {
                foreach (var c in sistem.Clienti)
                {
                    if (c.Istoric == null) continue;

                    foreach (var t in c.Istoric)
                    {
                        count++;
                        table.AddRow(
                            Markup.Escape(c.Nume),
                            t.Data.ToString("dd/MM/yyyy HH:mm"),
                            Markup.Escape(t.Matcherie?.Nume ?? "N/A"),
                            $"{t.suma} RON"
                        );
                    }
                }
            }

            if (count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tranzacții.[/]");
                return;
            }

            AnsiConsole.Write(table);
        }

        static void CreeazaTranzactiePentruClient(SistemMatcha sistem)
        {
            if (sistem.Clienti == null || sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există clienți.[/]");
                return;
            }
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există matcherii.[/]");
                return;
            }

            // alegere client
            var clientiNume = new List<string>();
            foreach (var c in sistem.Clienti) clientiNume.Add(c.Nume);

            string clientAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează clientul:")
                    .AddChoices(clientiNume));

            // alegere magazin
            var magazineNume = new List<string>();
            foreach (var m in sistem.Magazine) magazineNume.Add(m.Nume);

            string magazinAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează matcheria:")
                    .AddChoices(magazineNume));

            decimal suma = AnsiConsole.Ask<decimal>("Sumă (RON):");

            Client? client = null;
            foreach (var c in sistem.Clienti)
                if (c.Nume == clientAles) { client = c; break; }

            Matcherie? magazin = null;
            foreach (var m in sistem.Magazine)
                if (m.Nume == magazinAles) { magazin = m; break; }

            if (client == null || magazin == null) return;

            client = FixClientIfNeeded(sistem, client);

            if (client.Istoric == null)
            {
                AnsiConsole.MarkupLine("[red]Istoricul clientului este null și nu poate fi modificat.[/]");
                return;
            }

            client.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), DateTime.Now, suma, magazin));
            AnsiConsole.MarkupLine("[green]Tranzacție adăugată și asociată clientului.[/]");
        }

        static void ModificaTranzactiePentruClient(SistemMatcha sistem)
        {
            if (sistem.Clienti == null || sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există clienți.[/]");
                return;
            }

            var clientiNume = new List<string>();
            foreach (var c in sistem.Clienti) clientiNume.Add(c.Nume);

            string clientAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează clientul:")
                    .AddChoices(clientiNume));

            Client? client = null;
            foreach (var c in sistem.Clienti)
                if (c.Nume == clientAles) { client = c; break; }

            if (client == null) return;

            client = FixClientIfNeeded(sistem, client);

            if (client.Istoric == null || client.Istoric.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Clientul nu are tranzacții.[/]");
                return;
            }

            var tranzList = new List<string>();
            foreach (var t in client.Istoric)
            {
                tranzList.Add($"{t.Data:dd/MM HH:mm} - {t.Matcherie?.Nume ?? "N/A"} - {t.suma} RON");
            }

            string tranzAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează tranzacția de modificat:")
                    .AddChoices(tranzList));

            int index = tranzList.IndexOf(tranzAles);
            if (index < 0) return;

            var tranzSelectata = client.Istoric[index];

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există matcherii.[/]");
                return;
            }

            var magazineNume = new List<string>();
            foreach (var m in sistem.Magazine) magazineNume.Add(m.Nume);

            string magazinNouNume = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Matcherie nouă:")
                    .AddChoices(magazineNume));

            Matcherie? magazinNou = null;
            foreach (var m in sistem.Magazine)
                if (m.Nume == magazinNouNume) { magazinNou = m; break; }

            if (magazinNou == null) return;

            decimal sumaNoua = AnsiConsole.Ask<decimal>($"Sumă nouă (curent {tranzSelectata.suma}):");

            // Înlocuire (pentru că setteri sunt private)
            client.Istoric[index] = new Tranzactie(tranzSelectata.Id, tranzSelectata.Data, sumaNoua, magazinNou);

            AnsiConsole.MarkupLine("[green]Tranzacție modificată (înlocuită).[/]");
        }

        // în cazul în care JSON-ul a încărcat liste null la Client (nu putem seta din Program pentru că private set)
        static Client FixClientIfNeeded(SistemMatcha sistem, Client client)
        {
            bool needsFix = (client.Istoric == null) || (client.Rezervari == null);
            if (!needsFix) return client;

            var ist = client.Istoric ?? new List<Tranzactie>();
            var rez = client.Rezervari ?? new List<Rezervare>();

            var fixedClient = new Client(client.Nume, client.Email, ist, rez);

            int idx = sistem.Clienti.IndexOf(client);
            if (idx >= 0) sistem.Clienti[idx] = fixedClient;

            return fixedClient;
        }

        // -------------------- MONITORIZARE --------------------

        static void AfiseazaMonitorizare(SistemMatcha sistem)
        {
            Console.Clear();

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există matcherii.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Monitorizare activitate[/]");
            t.AddColumn("Matcherie");
            t.AddColumn(new TableColumn("Rezervări active").RightAligned());
            t.AddColumn(new TableColumn("Capacitate").RightAligned());
            t.AddColumn(new TableColumn("Ocupare").RightAligned());

            foreach (var m in sistem.Magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                int pct = (int)Math.Round(100.0 * rez / cap);

                t.AddRow(
                    Markup.Escape(m.Nume),
                    rez.ToString(),
                    m.Capacitate.ToString(),
                    $"{pct}%"
                );
            }

            AnsiConsole.Write(t);
        }

        // -------------------- GENERIC --------------------

        static void Pauza()
        {
            AnsiConsole.MarkupLine("\n[grey]Apasă orice tastă pentru a continua...[/]");
            Console.ReadKey(true);
        }


        static void RulareMeniuClient(SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                // Apelăm noua funcție de UI
                Meniuri.AfiseazaDashboardClient(sistem.Clienti[0], sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Ce dorești să faci?[/]")
                        .AddChoices(new[] { 
                            "Comandă Matcha", 
                            "Rezervă Masă", 
                            "Anuleaza Rezervare Masă",
                            "Istoric Tranzacții",
                            "Vizulaizează Rezervări",
                            "Deconectare" 
                        }));
                var NumeMagazinAles = "";
                switch (optiune)
                {
                    case "Comandă Matcha":
                         NumeMagazinAles = sistem.Clienti[0].AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        sistem.Clienti[0].Comanda(NumeMagazinAles, sistem.Magazine, sistem.Administratori[0]);
                        break;
                    case "Vizulaizează Rezervări":
                        Console.Clear(); // Curățăm ecranul pentru a vedea doar tabelul
    
                        if (sistem.Clienti[0].Rezervari == null || sistem.Clienti[0].Rezervari.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă momentan.[/]");
                        }
                        else
                        {
                            // Creăm tabelul
                            var tabel = new Table()
                                .Border(TableBorder.Rounded)
                                .BorderColor(Color.Orange1)
                                .Title("[bold orange1]📅 REZERVĂRILE TALE[/]")
                                .Caption("[grey]Total rezervări active: " + sistem.Clienti[0].Rezervari.Count + "[/]");

                            tabel.AddColumn("[bold]Nr.[/]");
                            tabel.AddColumn("[bold]Locație[/]");
                            tabel.AddColumn("[bold]Tip Rezervare[/]");
                            tabel.AddColumn("[bold]Beneficii[/]");
                            tabel.AddColumn(new TableColumn("[bold]Pret[/]").Centered());

                            for (int i = 0; i < sistem.Clienti[0].Rezervari.Count; i++)
                            {
                                var rez = sistem.Clienti[0].Rezervari[i];
                                tabel.AddRow(
                                    (i + 1).ToString(),
                                    $"[cyan]{rez.Matcherie?.Nume ?? "Nespecificat"}[/]",
                                    rez.Tip,
                                    $"[italic grey]{rez.Beneficii}[/]",
                                    $"[green]{rez.Pret} RON[/]"
                                );
                            }
                            AnsiConsole.Write(tabel);
                        }
                        // --- ELEMENTUL CRUCIAL ---
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[grey]Apasă orice tastă pentru a reveni la meniu...[/]");
                        Console.ReadKey(true); // Oprește execuția până la apăsarea unei taste
                        break;
                    case "Rezervă Masă":
                        NumeMagazinAles = sistem.Clienti[0].AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        if (string.IsNullOrEmpty(NumeMagazinAles)) break;
                        foreach (var magazin in sistem.Magazine)
                        {
                            if (magazin.Nume==NumeMagazinAles)
                            {
                                /*
                                // Verificăm dacă lista Rezervari nu este null înainte de Add
                                if (sistem.Clienti[0].Rezervari == null) 
                                    sistem.Clienti[0].Rezervari = new List<Rezervare>();
                                */
                                var nouaRezervare = sistem.Clienti[0].rezervaMasa(magazin, sistem.Administratori[0]);
            
                                // Adăugăm în listă DOAR dacă metoda returnează obiectul și nu l-a adăugat deja intern
                                if (nouaRezervare != null)
                                {
                                    sistem.Clienti[0].Rezervari.Add(nouaRezervare);
                                    AnsiConsole.MarkupLine("[green]Rezervare adăugată cu succes![/]");
                                }
                            }
                        }
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[grey]Apasă orice tastă pentru a reveni la meniu...[/]");
                        Console.ReadKey(true); // Oprește execuția până la apăsarea unei taste
                        break;
                    case "Anuleaza Rezervare Masă":
                        if (sistem.Clienti[0].Rezervari == null || sistem.Clienti[0].Rezervari.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă de anulat.[/]");
                            Console.ReadKey(true);
                            break;
                        }

                        var rezervareDeAnulat = AnsiConsole.Prompt(
                            new SelectionPrompt<Rezervare>()
                                .Title("Selectează rezervarea pe care dorești să o [red]anulezi[/]:")
                                .PageSize(10)
                                .AddChoices(sistem.Clienti[0].Rezervari)
                                .UseConverter(r => {
                                    // ESCAPARE: Protejăm textul împotriva interpretării ca stil/culoare
                                    string numeEscapat = Markup.Escape(r.Matcherie?.Nume ?? "Nespecificat");
                                    string tipEscapat = Markup.Escape(r.Tip ?? "Rezervare");
                
                                    return $"[[{numeEscapat}]] {tipEscapat} - [green]{r.Pret} RON[/]";
                                }));

                        if (AnsiConsole.Confirm($"Sigur dorești să anulezi rezervarea [yellow]{Markup.Escape(rezervareDeAnulat.Tip)}[/]?"))
                        {
                            // Ștergem din ambele liste
                            rezervareDeAnulat.Matcherie?.Rezervari.Remove(rezervareDeAnulat);//?-null conditional operator
                            sistem.Clienti[0].Rezervari.Remove(rezervareDeAnulat);

                            AnsiConsole.MarkupLine("[bold green]Rezervarea a fost anulată cu succes![/]");
                        }
    
                        Console.ReadKey(true);
                        break;
                    case "Istoric Tranzacții":
                        Console.Clear();
                        if (sistem.Clienti[0].Istoric == null || sistem.Clienti[0].Istoric.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]Nu ai nicio tranzacție înregistrată.[/]");
                        }
                        else
                        {
                            var tabel = new Table()
                                .Border(TableBorder.DoubleEdge)
                                .Title("[bold magenta]🧾 ISTORIC CUMPĂRĂTURI[/]")
                                .BorderColor(Color.Magenta1);

                            tabel.AddColumn("Dată");
                            tabel.AddColumn("Magazin");
                            tabel.AddColumn(new TableColumn("Preț").RightAligned());

                            foreach (var t in sistem.Clienti[0].Istoric)
                            {
                                tabel.AddRow(
                                    t.Data.ToString("dd/MM/yyyy HH:mm"),
                                    Markup.Escape(t.Matcherie.Nume),
                                    $"[green]{t.suma} RON[/]"
                                );
                            }
                            AnsiConsole.Write(tabel);
                        }

                        AnsiConsole.WriteLine("\nApasă orice tastă pentru a reveni...");
                        Console.ReadKey(true);
                        break;
                        
                        break;
                    case "Deconectare":
                        inapoi = true;
                        break;
                }
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
            var meniu2 = new List<Matcha> { new Matcha("Matcha Latte vf. Odaia", "Clasic amar", 22.5m, 100, 120) };
            var m3 = new Matcherie("Matcha urzica sanicolau nou", "08-22", 20, meniu2, new List<Rezervare>());
            sistem.Magazine.Add(m1);
            sistem.Magazine.Add(m3);
            sistem.Administratori.Add(new AdministratorMatcha("Admin", "ADM01", "1234", sistem.Magazine));
            sistem.Administratori.Add(new AdministratorMatcha("Admin22", "ADM02", "1234", sistem.Magazine));
            sistem.Clienti.Add(new Client("Andrei", "andrei@email.com", new List<Tranzactie>(), new List<Rezervare>()));
        }
        
        static string AfiseazaEcranStartSiAlegeRol(SistemMatcha sistem)
{
    // dacă terminalul e mic sau raportează aiurea -> fallback simplu
    int w = AnsiConsole.Profile.Width;
    int h = AnsiConsole.Profile.Height;
    if (w < 90 || h < 28)
    {
        AnsiConsole.Clear();
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold green]X Matcha[/] - alege rolul")
                .AddChoices("Client", "Administrator", "Ieșire")
        );
    }

    // animație (dacă aici merge, e ok)
    RulareAnimatieMatchaSteam();

    // ASCII cup Starbucks (profil) cu gheață + matcha latte
    var cupStatic = new Text(@"
             ||
             ||
          ___||___
         /________\
        /  ______  \
       /  /=====\   \
      |  | [] [] |   |
      |  | [] [] |   |   ice
      |  | [] [] |   |
      |  |  (S)  |   |   logo
      |  |~~~~~~~|   |   matcha latte
      |  |~~~~~~~|   |
      |  |~~~~~~~|   |
      |  |_______|   |
       \           _/
        \_________/
");

    AnsiConsole.Clear();

    // TOP BAR (tabs)
    var tabs = new Grid();
    tabs.AddColumn(new GridColumn().LeftAligned());
    tabs.AddColumn(new GridColumn().Centered());
    tabs.AddColumn(new GridColumn().RightAligned());
    tabs.AddRow(
        new Markup("[bold green]General[/] [grey]|[/] Static [grey]|[/] Dynamic [grey]|[/] Autentificare"),
        new Markup("[grey]X Matcha v1.0[/]"),
        new Markup($"[grey]{DateTime.Now:HH:mm}[/]")
    );

    AnsiConsole.Write(new Panel(tabs)
        .Border(BoxBorder.Double)
        .BorderColor(Color.Green)
        .Expand());

    // HEADER (fără Figlet, ca să nu mai forțeze înălțimea)
    var header = new Panel(
        new Rows(
            Align.Center(new Markup("[bold green]X Matcha[/]")),
            Align.Center(new Markup("[green]X marchează matcha[/]")),
            Align.Center(new Markup("[grey]Prima aplicatie care aduce in acelasi loc clientii, managerii si matcheriile din propriul tau oras![/]"))
        ))
        .Border(BoxBorder.Double)
        .BorderColor(Color.Green)
        .Expand();

    AnsiConsole.Write(header);

    // BODY: 2 panouri în grid (mult mai stabil decât Layout)
    int nrMagazine = sistem.Magazine?.Count ?? 0;
    int nrClienti = sistem.Clienti?.Count ?? 0;
    int nrAdmins = sistem.Administratori?.Count ?? 0;

    var left = new Panel(
        new Rows(
            new Markup("[bold green]📌 Dashboard[/]"),
            new Rule().RuleStyle("green"),
            new Markup($"[green]Matcherii:[/] {nrMagazine}"),
            new Markup($"[green]Clienți:[/] {nrClienti}"),
            new Markup($"[green]Administratori:[/] {nrAdmins}"),
            new Rule().RuleStyle("green"),
            new Markup("[grey]Navighează cu ↑↓ și Enter.[/]")
        ))
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Green)
        .Expand();

    var right = new Panel(Align.Center(cupStatic))
        .Header("[bold green]🍵 Matcha Latte[/]")
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Green)
        .Expand();

    var body = new Grid();
    body.AddColumn(new GridColumn()); // stânga
    body.AddColumn(new GridColumn()); // dreapta
    body.AddRow(left, right);

    AnsiConsole.Write(body);

    // “butoane” + selection prompt cu highlight
    AnsiConsole.Write(new Panel(new Markup(
        "[black on green]   1  LOGARE CLIENT           [/]\n" +
        "[black on green]   2  LOGARE ADMINISTRATOR    [/]\n\n" +
        "[white on darkred]   3  IESIRE                  [/]"
    ))
    .Header("[bold green]🔐 Autentificare[/]")
    .Border(BoxBorder.Double)
    .BorderColor(Color.Green)
    .Expand());

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("\n[grey]Selectează o opțiune:[/]")
            .AddChoices("LOGARE CLIENT", "LOGARE ADMINISTRATOR", "IESIRE")
            .HighlightStyle(new Style(foreground: Color.Black, background: Color.Green))
    );

    return choice switch
    {
        "LOGARE CLIENT" => "Client",
        "LOGARE ADMINISTRATOR" => "Administrator",
        _ => "Ieșire"
    };
}

            static void RulareAnimatieMatchaSteam()
            {
                var frames = new[]
                {
                    @"
               ~  ~
                ~
                 ||
                 ||
              ___||___
             /________\
            /  ______  \
           /  /=====\   \
          |  | [] [] |   |
          |  | [] [] |   |
          |  | [] [] |   |
          |  |  (S)  |   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |_______|   |
           \           _/
            \_________/
    ",
                        @"
                ~  ~
               ~
                 ||
                 ||
              ___||___
             /________\
            /  ______  \
           /  /=====\   \
          |  | [] [] |   |
          |  | [] [] |   |
          |  | [] [] |   |
          |  |  (S)  |   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |_______|   |
           \           _/
            \_________/
    ",
                        @"
               ~
                ~  ~
                 ||
                 ||
              ___||___
             /________\
            /  ______  \
           /  /=====\   \
          |  | [] [] |   |
          |  | [] [] |   |
          |  | [] [] |   |
          |  |  (S)  |   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |_______|   |
           \           _/
            \_________/
    ",
                        @"
                ~
               ~  ~
                 ||
                 ||
              ___||___
             /________\
            /  ______  \
           /  /=====\   \
          |  | [] [] |   |
          |  | [] [] |   |
          |  | [] [] |   |
          |  |  (S)  |   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |~~~~~~~|   |
          |  |_______|   |
           \           _/
            \_________/
            "
                };


                    // Panel mic + live update
                    var panel = new Panel(Align.Center(new Text(frames[0])))
                        .Header("[bold green]Se încălzește matcha...[/]")
                        .Border(BoxBorder.Double)
                        .BorderColor(Color.Green)
                        .Expand();

                    AnsiConsole.Clear();

                    AnsiConsole.Live(panel)
                        .AutoClear(true)
                        .Start(ctx =>
                        {
                            // ~2 sec total
                            for (int i = 0; i < 16; i++)
                            {
                                panel = new Panel(Align.Center(new Text(frames[i % frames.Length])))
                                    .Header("[bold green]Se încălzește matcha...[/]")
                                    .Border(BoxBorder.Double)
                                    .BorderColor(Color.Green)
                                    .Expand();

                                ctx.UpdateTarget(panel);
                                ctx.Refresh();
                                Thread.Sleep(120);
                            }
                        });
                }





    }
}