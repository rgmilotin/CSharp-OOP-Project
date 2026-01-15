using System.Runtime.CompilerServices;
using ConsoleApp5;
using Spectre.Console.Rendering;
using System.Text;
using System.IO;
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
                        if (a.AdminId == id && a.Parola == parola)
                        {
                            AnsiConsole.MarkupLine("[green]Autentificare reușită![/]");
                            Thread.Sleep(300);

                            // Rebind: admin.Matcherii = sistem.Magazine (fără setteri publici)
                            var adminLegatDeSistem = new AdministratorMatcha(a.Nume, a.AdminId, a.Parola, sistem.Magazine);

                            int idx = sistem.Administratori.IndexOf(a);
                            if (idx >= 0) sistem.Administratori[idx] = adminLegatDeSistem;

                            return adminLegatDeSistem;
                        }
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
                foreach (var m in sistem.Magazine)
                    rezervariActive += (m.Rezervari?.Count ?? 0);

            int tranzactii = 0;
            if (sistem.Clienti != null)
                foreach (var c in sistem.Clienti)
                    tranzactii += (c.Istoric?.Count ?? 0);

            // Panel stânga (info)
            var info = new Panel(
                new Rows(
                    new Markup($"[bold]Admin:[/] {Markup.Escape(admin.Nume)} ([grey]{Markup.Escape(admin.AdminId)}[/])"),
                    new Markup($"[bold]Magazine:[/] {nrMagazine}"),
                    new Markup($"[bold]Clienți:[/] {nrClienti}"),
                    new Markup($"[bold]Rezervări active:[/] {rezervariActive}"),
                    new Markup($"[bold]Tranzacții totale:[/] {tranzactii}")
                ))
                .Header("[bold red]📌 DASHBOARD ADMIN[/]")
                .BorderColor(Color.Red)
                .Expand();

            // Grafic: vânzări pe ultimele 7 zile (din Tranzactie.Data)
            var chart = new BarChart()
                .Label("[green]Vânzări (ultimele 7 zile)[/]")
                .CenterLabel();

            int width = Math.Max(30, Math.Min(60, AnsiConsole.Profile.Width / 2 - 10));
            chart.Width(width);

            DateTime azi = DateTime.Today;
            for (int i = 6; i >= 0; i--)
            {
                DateTime zi = azi.AddDays(-i);
                int countZi = 0;

                if (sistem.Clienti != null)
                {
                    foreach (var c in sistem.Clienti)
                    {
                        if (c.Istoric == null) continue;
                        foreach (var t in c.Istoric)
                            if (t.Data.Date == zi.Date) countZi++;
                    }
                }

                chart.AddItem(zi.ToString("dd/MM"), countZi, Color.Green);
            }

            var chartPanel = new Panel(chart)
                .Header("[bold green]📈 Trend[/]")
                .BorderColor(Color.Green)
                .Expand();

            // Afișare în 2 coloane dacă avem loc, altfel una sub alta
            int w = AnsiConsole.Profile.Width;
            if (w >= 120)
            {
                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(info, chartPanel);
                AnsiConsole.Write(grid);
            }
            else
            {
                AnsiConsole.Write(info);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(chartPanel);
            }

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
                            "Meniu produse (CRUD)",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Meniu produse (CRUD)":
                        SubmeniuMeniuProduse(admin);
                        break;
                    
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
        static void SubmeniuMeniuProduse(AdministratorMatcha admin)
        {
            if (admin.Matcherii == null || admin.Matcherii.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există matcherii. Creează una mai întâi.[/]");
                Pauza();
                return;
            }

            // alegem matcheria o dată, apoi operăm în ea
            Matcherie matcherie = AlegeMatcherieDinAdmin(admin);
            if (matcherie == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Meniu produse[/] pentru: [white]{Markup.Escape(matcherie.Nume)}[/]");
                AnsiConsole.WriteLine();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Alege acțiunea:")
                        .AddChoices(new[]
                        {
                            "Vezi meniul",
                            "Adaugă produs",
                            "Modifică produs",
                            "Șterge produs",
                            "Schimbă matcheria",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Vezi meniul":
                        Console.Clear();
                        AfiseazaMeniuSafe(matcherie);
                        Pauza();
                        break;

                    case "Adaugă produs":
                        AdaugaProdus(matcherie);
                        Pauza();
                        break;

                    case "Modifică produs":
                        ModificaProdus(matcherie);
                        Pauza();
                        break;

                    case "Șterge produs":
                        StergeProdus(matcherie);
                        Pauza();
                        break;

                    case "Schimbă matcheria":
                        matcherie = AlegeMatcherieDinAdmin(admin);
                        if (matcherie == null) return;
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        static Matcherie AlegeMatcherieDinAdmin(AdministratorMatcha admin)
        {
            var numeList = new List<string>();
            foreach (var m in admin.Matcherii)
                numeList.Add(m.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege matcheria:")
                    .AddChoices(numeList));

            foreach (var m in admin.Matcherii)
                if (m.Nume == ales) return m;

            return null;
        }

        static void AfiseazaMeniuSafe(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Meniul este gol.[/]");
                return;
            }

            matcherie.AfiseazaMeniu();
        }

        static void AdaugaProdus(Matcherie matcherie)
        {

            string nume = AnsiConsole.Ask<string>("Nume produs:");
            foreach (var p in matcherie.Meniu)
            {
                if (p.nume == nume)
                {
                    AnsiConsole.MarkupLine("[red]Există deja un produs cu acest nume.[/]");
                    return;
                }
            }

            string descriere = AnsiConsole.Ask<string>("Descriere:");
            decimal pret = AnsiConsole.Ask<decimal>("Preț (RON):");
            int cantitate = AnsiConsole.Ask<int>("Cantitate (stoc):");
            int calorii = AnsiConsole.Ask<int>("Calorii:");

            matcherie.Meniu.Add(new Matcha(nume, descriere, pret, cantitate, calorii));
            AnsiConsole.MarkupLine("[green]Produs adăugat în meniu.[/]");
        }

        static void ModificaProdus(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există produse de modificat.[/]");
                return;
            }

            var numeProduse = new List<string>();
            foreach (var p in matcherie.Meniu) numeProduse.Add(p.nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege produsul de modificat:")
                    .AddChoices(numeProduse));

            Matcha produs = null;
            foreach (var p in matcherie.Meniu)
                if (p.nume == ales) { produs = p; break; }

            if (produs == null) return;

            var camp = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Ce dorești să modifici?")
                    .AddChoices(new[]
                    {
                        "Nume",
                        "Descriere",
                        "Preț",
                        "Cantitate",
                        "Calorii",
                        "Anulează"
                    }));

            if (camp == "Anulează") return;

            if (camp == "Nume")
            {
                string nou = AnsiConsole.Ask<string>($"Nume nou (curent: {produs.nume}):");

                // verificăm duplicat
                foreach (var p in matcherie.Meniu)
                    if (p != produs && p.nume == nou)
                    {
                        AnsiConsole.MarkupLine("[red]Există deja un produs cu acest nume.[/]");
                        return;
                    }

                produs.nume = nou;
            }
            else if (camp == "Descriere")
            {
                produs.descriere = AnsiConsole.Ask<string>($"Descriere nouă (curent: {produs.descriere}):");
            }
            else if (camp == "Preț")
            {
                produs.pret = AnsiConsole.Ask<decimal>($"Preț nou (curent: {produs.pret}):");
            }
            else if (camp == "Cantitate")
            {
                produs.cantitate = AnsiConsole.Ask<int>($"Cantitate nouă (curent: {produs.cantitate}):");
            }
            else if (camp == "Calorii")
            {
                produs.calorii = AnsiConsole.Ask<int>($"Calorii noi (curent: {produs.calorii}):");
            }

            AnsiConsole.MarkupLine("[green]Produs modificat.[/]");
        }

        static void StergeProdus(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există produse de șters.[/]");
                return;
            }

            var numeProduse = new List<string>();
            foreach (var p in matcherie.Meniu) numeProduse.Add(p.nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege produsul de șters:")
                    .AddChoices(numeProduse));

            Matcha produs = null;
            foreach (var p in matcherie.Meniu)
                if (p.nume == ales) { produs = p; break; }

            if (produs == null) return;

            if (!AnsiConsole.Confirm($"Sigur vrei să ștergi [red]{Markup.Escape(produs.nume)}[/]?"))
                return;

            matcherie.Meniu.Remove(produs);
            AnsiConsole.MarkupLine("[green]Produs șters.[/]");
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
            static string IncarcaAsciiArtStart()
            {
                string fallback = """
                                                                         
                                                   +++                 
                                                 ++**                  
                                               +*+                   
                                             +#+                       
                                            =*+                        
                                         ...=*...                      
                                      ......=+......                   
                                     ... ...+=........                 
                                   ...   ...*=::.....:                 
                                   .. ...::-#+::.:-:.:.                
                                  .::...::-===--=---::-:               
                                  .:....:--=====-----=:.               
                                   ::---==++******++*=                 
                                   ::--=++#**####****+                 
                                   ::--==++*#####***+=                 
                                   ::---=+**#####***+=                 
                                    ::--==+*####****+=                 
                                    :::-==+***#****++                  
                                    .::--==+++**++++=                  
                                    ..:::--=++++=+==-                  
                                     ....=:-==#+==--:                  
                                     .....:::--=----:                  
                                     .....:--:----:::                  
                                     ......::::::::-:                  
                                     ......::::::---                   
                                     ......:::::---=                   
                                       ....:---===+                    
                                                                       
                                  """;

                try
                {
                    string path = Path.Combine(AppContext.BaseDirectory, "assets", "start_art.txt");
                    if (File.Exists(path))
                        return File.ReadAllText(path);
                }
                catch
                {
                    // ignorăm și folosim fallback
                }

                return fallback;
            }

            // ASCII art: fie din fișier assets/start_art.txt, fie fallback
            string ascii = IncarcaAsciiArtStart();

            int lastW = -1, lastH = -1;

            while (true)
            {
                int w = Console.WindowWidth;
                int h = Console.WindowHeight;

                // re-randăm doar când se schimbă dimensiunea (windowed <-> fullscreen)
                if (w != lastW || h != lastH)
                {
                    lastW = w;
                    lastH = h;
                    RandareEcranStart(sistem, ascii);
                }

                // input non-blocking: 1/2/3 (se simte ca "butoane" și e robust la resize)
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1)
                        return "Client";

                    if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2)
                        return "Administrator";

                    if (key.Key == ConsoleKey.D3 || key.Key == ConsoleKey.NumPad3 || key.Key == ConsoleKey.Escape)
                        return "Ieșire";

                    // R = refresh manual (dacă vrei)
                    if (key.Key == ConsoleKey.R)
                    {
                        lastW = -1;
                        lastH = -1;
                    }
                }

                Thread.Sleep(80);
            }
        }
        static List<(string nume, int val)> GetTopMatcheriiByRezervari(SistemMatcha sistem, int max)
        {
            var list = new List<(string nume, int val)>();

            if (sistem.Magazine == null) return list;

            foreach (var m in sistem.Magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                list.Add((m.Nume, rez));
            }

            // sort desc
            list.Sort((a, b) => b.val.CompareTo(a.val));

            if (list.Count > max) list = list.GetRange(0, max);
            return list;
        }
          
        // =================== START SCREEN ===================

        static void RandareEcranStart(SistemMatcha sistem, string asciiArt)
        {
            AnsiConsole.Clear();

            int w = AnsiConsole.Profile.Width;
            int h = AnsiConsole.Profile.Height;

            if (w < 90 || h < 28)
            {
                AnsiConsole.MarkupLine("[bold green]X Matcha[/]");
                AnsiConsole.MarkupLine("[green]X marchează matcha[/]");
                AnsiConsole.MarkupLine("[grey]1) Logare Client  2) Logare Administrator  3) Ieșire[/]");
                AnsiConsole.MarkupLine("[grey](Mărește fereastra pentru UI complet)[/]");
                return;
            }

            // TOP BAR
            var tabs = new Grid();
            tabs.AddColumn(new GridColumn().LeftAligned());
            tabs.AddColumn(new GridColumn().Centered());
            tabs.AddColumn(new GridColumn().RightAligned());
            tabs.AddRow(
                new Markup("[bold green]General[/] [grey]|[/] Statistici [grey]|[/] Reviews [grey]|[/] Start"),
                new Markup("[grey]X Matcha v1.0[/]"),
                new Markup($"[grey]{DateTime.Now:HH:mm}[/]")
            );

            AnsiConsole.Write(
                new Panel(tabs)
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Green)
                    .Expand()
            );

            // TITLU
            if (h >= 34)
                AnsiConsole.Write(Align.Center(new FigletText("X Matcha").Color(Color.Green)));
            else
                AnsiConsole.Write(
                    new Panel(Align.Center(new Markup("[bold green]X Matcha[/]")))
                        .Border(BoxBorder.Double)
                        .BorderColor(Color.Green)
                        .Expand()
                );

            // SUBTITLU
            var header = new Panel(
                    new Rows(
                        Align.Center(new Markup("[green]X marchează matcha[/]")),
                        Align.Center(new Markup("[grey]Prima aplicație care aduce în același loc clienții, managerii și matcheriile din propriul tău oraș![/]"))
                    ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();

            AnsiConsole.Write(header);

            // =================== AICI E DIFERENȚA: ASCII dictează înălțimea ===================
            // max înălțime permisă de ecran
            int maxBody = Math.Clamp(h - 26, 12, 30);

            // calculăm câte linii are ASCII-ul (și îl limităm ca să nu iasă din ecran)
            string asciiNormalized = (asciiArt ?? "").Replace("\r", "").TrimEnd('\n');
            int asciiNaturalLines = CountLines(asciiNormalized);
            int cardLines = Math.Clamp(asciiNaturalLines, 12, maxBody); // ASCII -> cardLines

            if (w >= 140)
            {
                int colW = Math.Max(34, (w - 10) / 3);

                // IMPORTANT: construim ASCII cu exact cardLines (clip+pad),
                // iar celelalte două coloane primesc padding până la cardLines.
                var asciiPanel = ConstruiestePanelAscii(asciiArt, colW, cardLines);
                var statsPanel = ConstruiestePanelStatisticiSiGrafice(sistem, colW, cardLines);
                var reviewsPanel = ConstruiestePanelTestimoniale(colW, cardLines);

                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(statsPanel, reviewsPanel, asciiPanel);

                AnsiConsole.Write(grid);
            }
            else if (w >= 105)
            {
                int colW = Math.Max(40, (w - 8) / 2);

                var asciiPanel = ConstruiestePanelAscii(asciiArt, colW, cardLines);
                var statsPanel = ConstruiestePanelStatisticiSiGrafice(sistem, colW, cardLines);

                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(statsPanel, asciiPanel);
                AnsiConsole.Write(grid);

                AnsiConsole.WriteLine();

                // reviews jos (fără nevoie să fie la fix cu ASCII)
                AnsiConsole.Write(ConstruiestePanelTestimoniale(w, Math.Clamp(cardLines, 10, 16)));
            }
            else
            {
                AnsiConsole.Write(ConstruiestePanelStatisticiSiGrafice(sistem, w, Math.Clamp(cardLines, 10, 16)));
                AnsiConsole.WriteLine();
                AnsiConsole.Write(ConstruiestePanelTestimoniale(w, Math.Clamp(cardLines, 10, 16)));
                AnsiConsole.WriteLine();
                AnsiConsole.Write(ConstruiestePanelAscii(asciiArt, w, Math.Clamp(cardLines, 10, 16)));
            }

            // BUTOANE
            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new Panel(new Markup(
                    "[black on green]   1  LOGARE CLIENT           [/]\n" +
                    "[black on green]   2  LOGARE ADMINISTRATOR    [/]\n\n" +
                    "[white on darkred]   3  IESIRE (sau ESC)        [/] \n\n" +
                    "[grey]Hint: poți redimensiona fereastra, UI-ul se reface automat. (R = refresh)[/]"
                ))
                .Header("[bold green]Start[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Expand()
            );
        }


        // =================== STATISTICI (cu BreakdownChart ca înainte) ===================

        static IRenderable ConstruiestePanelStatisticiSiGrafice(SistemMatcha sistem, int colWidth, int cardLines)
        {
            int nrMagazine = sistem.Magazine?.Count ?? 0;
            int nrClienti = sistem.Clienti?.Count ?? 0;
            int nrAdmins = sistem.Administratori?.Count ?? 0;

            int totalProduse = 0, totalRezervari = 0, totalCapacitate = 0;
            if (sistem.Magazine != null)
            {
                foreach (var m in sistem.Magazine)
                {
                    totalProduse += (m.Meniu?.Count ?? 0);
                    totalRezervari += (m.Rezervari?.Count ?? 0);
                    totalCapacitate += m.Capacitate;
                }
            }

            int totalTranzactii = 0;
            if (sistem.Clienti != null)
                foreach (var c in sistem.Clienti)
                    totalTranzactii += (c.Istoric?.Count ?? 0);

            var rows = new List<IRenderable>
            {
                new Markup($"[bold]Magazine:[/] {nrMagazine}   [bold]Clienți:[/] {nrClienti}   [bold]Admini:[/] {nrAdmins}"),
                new Markup($"[bold]Produse:[/] {totalProduse}   [bold]Rezervări:[/] {totalRezervari}   [bold]Tranzacții:[/] {totalTranzactii}"),
                new Rule("[green]Overview[/]")
            };

            // 1) BreakdownChart DOAR pe valori comparabile (fără Capacitate, că domină)
            int chartW = Math.Clamp(colWidth - 8, 22, 60);
            int sumAct = totalProduse + totalRezervari + totalTranzactii;

            if (sumAct <= 0)
            {
                rows.Add(new Markup("[grey]Nu există date pentru activitate.[/]"));
            }
            else
            {
                var breakdownAct = new BreakdownChart()
                    .Width(chartW)
                    .AddItem("Produse", totalProduse, Color.Aqua)      // mai distinct
                    .AddItem("Rezervari", totalRezervari, Color.Yellow)
                    .AddItem("Tranzactii", totalTranzactii, Color.Magenta1);

                rows.Add(breakdownAct);
            }

            // 2) Ocupare: arată clar Rezervări vs Locuri libere (aici Capacitatea are sens)
            rows.Add(new Rule("[green]Ocupare[/]"));

            int cap = Math.Max(1, totalCapacitate);
            int ocupate = Math.Clamp(totalRezervari, 0, cap);
            int libere = Math.Max(0, cap - ocupate);
            int pct = (int)Math.Round(100.0 * ocupate / cap);

            var ocupare = new BreakdownChart()
                .Width(chartW)
                .AddItem("Ocupate", ocupate, Color.Yellow)
                .AddItem("Libere", libere, Color.Green);

            rows.Add(ocupare);
            rows.Add(new Markup($"[grey]Capacitate totală:[/] [white]{cap}[/]   [grey]Ocupare:[/] [white]{pct}%[/]"));

            // Popularitate (ca înainte, simplu)
            rows.Add(new Rule("[green]Popularitate[/]"));
            var top = GetTopMatcheriiByRezervari(sistem, 3);

            if (top.Count == 0)
            {
                rows.Add(new Markup("[grey]N/A[/]"));
            }
            else
            {
                int nameW = Math.Max(10, colWidth - 14);
                foreach (var x in top)
                {
                    string name = CutNoDots(x.nume, nameW);
                    rows.Add(new Markup($"[white]{Markup.Escape(name)}[/]  [green]{x.val}[/]"));
                }
            }

            // padding până la cardLines (dictat de ASCII)
            int used = rows.Count;
            int pad = Math.Max(0, cardLines - used);
            rows.Add(new Text(BlankLines(pad)));

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold green]Statistici retea[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }



        // =================== TESTIMONIALE (fără "...", cu spațiu între ele) ===================

        static IRenderable ConstruiestePanelTestimoniale(int colWidth, int cardLines)
        {
            var reviews = new[]
            {
                ("Alex B.", 5, "Matcha extraordinară, servicii premium și un personal foarte atent la nevoile clientului."),
                ("Mara D.", 4, "Atmosferă super cozy, meniul e variat și matcha latte-ul e top. Aș mai adăuga doar câteva deserturi noi."),
                ("Radu P.", 5, "Rezervarea a mers rapid, iar comanda a venit foarte repede. Calitate constantă, perfect pentru zile aglomerate."),
                ("Ioana S.", 4, "Prețuri ok, locații bune, și îmi place că văd totul într-un singur loc. UI-ul e chiar fun.")
            };

            int innerW = Math.Max(24, colWidth - 6);

            var rows = new List<IRenderable>
            {
                new Markup("[grey]Ultimele review-uri[/]")
            };

            // câte linii mai avem disponibile în panel (în afară de titlul "Ultimele...")
            int remaining = Math.Max(0, cardLines - rows.Count);

            foreach (var r in reviews)
            {
                string stars = Stele(r.Item2);
                string author = r.Item1;
                string text = r.Item3;

                string prefixPlain = $"{stars} {author} - ";
                int prefixLen = prefixPlain.Length;

                // cât loc rămâne pe prima linie pentru text (după prefix)
                int firstTextW = Math.Max(5, innerW - prefixLen);

                // pentru liniile de continuare, indentăm la aceeași coloană ca textul
                string indent = new string(' ', prefixLen);
                int nextTextW = Math.Max(5, innerW - indent.Length);

                // împachetează textul pe linii (fără tăiere cu ...)
                var wrapped = WrapWithFirstWidth(text, firstTextW, nextTextW);

                // câte linii ocupă review-ul? (prima + continuări) + 1 linie goală între reviews
                int needed = wrapped.Count + 1;

                // dacă NU încape integral, ne oprim (nu afișăm review tăiat)
                if (needed > remaining)
                    break;

                // prima linie (cu prefix colorat)
                rows.Add(new Markup(
                    $"[yellow]{Markup.Escape(stars)}[/] " +
                    $"[green]{Markup.Escape(author)}[/] " +
                    $"[grey]-[/] " +
                    $"[white]{Markup.Escape(wrapped[0])}[/]"
                ));

                // continuări (indent + text)
                for (int i = 1; i < wrapped.Count; i++)
                {
                    rows.Add(new Markup($"[white]{Markup.Escape(indent + wrapped[i])}[/]"));
                }

                // spațiu între testimoniale
                rows.Add(new Text(""));

                remaining -= needed;
            }

            // padding până la cardLines (dictat de ASCII)
            int used = rows.Count;
            int pad = Math.Max(0, cardLines - used);
            rows.Add(new Text(BlankLines(pad)));

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold green]Testimoniale[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }



        // =================== ASCII (exact cardLines; el dictează) ===================

        static IRenderable ConstruiestePanelAscii(string asciiArt, int colWidth, int cardLines)
        {
            asciiArt ??= "";
            string normalized = asciiArt.Replace("\r", "").TrimEnd('\n');

            int safeWidth = Math.Max(20, colWidth - 8);
            int maxLine = MaxLineLength(normalized);

            string body;

            if (maxLine > safeWidth)
            {
                body =
                    "ASCII art prea lat pentru coloana curentă.\n" +
                    "Mărește fereastra sau folosește un art mai îngust.";
            }
            else
            {
                body = normalized;
            }

            // clip + pad la EXACT cardLines
            string fixedLines = PadOrClipToLines(body, cardLines);

            return new Panel(Align.Center(new Text(fixedLines)))
                .Header("[bold green]Art[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }


        // =================== HELPERS ===================

        static string BlankLines(int n)
        {
            if (n <= 0) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < n; i++) sb.AppendLine();
            return sb.ToString();
        }

        static int CountLines(string s)
        {
            if (string.IsNullOrEmpty(s)) return 1;
            return s.Replace("\r", "").Split('\n').Length;
        }

        static int MaxLineLength(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            var lines = s.Replace("\r", "").Split('\n');
            int max = 0;
            foreach (var line in lines)
                if (line.Length > max) max = line.Length;
            return max;
        }

        // taie fără "..."
        static string CutNoDots(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || max <= 0) return "";
            if (s.Length <= max) return s;
            return s.Substring(0, max);
        }

        // fix la N linii (ASCII dictează; celelalte se aliniază)
        static string PadOrClipToLines(string s, int linesWanted)
        {
            if (linesWanted <= 0) return "";
            string[] lines = (s ?? "").Replace("\r", "").Split('\n');

            var sb = new StringBuilder();

            int take = Math.Min(linesWanted, lines.Length);
            for (int i = 0; i < take; i++)
            {
                sb.Append(lines[i]);
                if (i < linesWanted - 1) sb.Append('\n');
            }

            // padding dacă nu ajunge
            for (int i = take; i < linesWanted; i++)
            {
                sb.Append('\n');
            }

            return sb.ToString();
        }

        static string Stele(int n)
        {
            n = Math.Clamp(n, 0, 5);
            return new string('★', n) + new string('☆', 5 - n);
        }
        static List<string> WrapWithFirstWidth(string text, int firstWidth, int nextWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                result.Add("");
                return result;
            }

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // construim prima linie cu firstWidth
            int idx = 0;
            string line = "";

            void FlushLine()
            {
                result.Add(line);
                line = "";
            }

            int currentWidth = firstWidth;

            while (idx < words.Length)
            {
                string w = words[idx];

                // dacă un cuvânt e mai mare decât width, îl spargem (rar, dar safe)
                if (w.Length > currentWidth)
                {
                    if (!string.IsNullOrEmpty(line))
                        FlushLine();

                    int start = 0;
                    while (start < w.Length)
                    {
                        int take = Math.Min(currentWidth, w.Length - start);
                        result.Add(w.Substring(start, take));
                        start += take;

                        // după prima linie, trecem pe nextWidth
                        currentWidth = nextWidth;
                    }

                    idx++;
                    continue;
                }

                // încercăm să îl adăugăm pe linia curentă
                if (string.IsNullOrEmpty(line))
                {
                    line = w;
                    idx++;
                }
                else
                {
                    if (line.Length + 1 + w.Length <= currentWidth)
                    {
                        line += " " + w;
                        idx++;
                    }
                    else
                    {
                        FlushLine();
                        currentWidth = nextWidth; // după prima linie
                    }
                }
            }

            if (!string.IsNullOrEmpty(line))
                result.Add(line);

            // asigurăm minim 1 linie
            if (result.Count == 0) result.Add("");
            return result;
        }



            


            static string ClipToMaxLines(string s, int maxLines)
            {
                if (string.IsNullOrEmpty(s)) return "";
                var lines = s.Replace("\r", "").Split('\n');

                if (lines.Length <= maxLines) return s;

                // ia primele maxLines (stabil)
                var sb = new StringBuilder();
                for (int i = 0; i < maxLines; i++)
                {
                    sb.Append(lines[i]);
                    if (i < maxLines - 1) sb.Append('\n');
                }
                return sb.ToString();
            }

            static string Truncate(string s, int max)
            {
                if (string.IsNullOrEmpty(s) || max <= 0) return "";
                if (s.Length <= max) return s;
                if (max <= 3) return s.Substring(0, max);
                return s.Substring(0, max - 3) + "...";
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