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
                        CreeazaMatcherie(admin);
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
                        StergeMatcherie(admin);
                        Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        static void CreeazaMatcherie(AdministratorMatcha admin)
        {
            string nume = AnsiConsole.Ask<string>("Nume matcherie:");
            string program = AnsiConsole.Ask<string>("Program (ex: 08:00-22:00):");
            int capacitate = AnsiConsole.Ask<int>("Capacitate:");

            var m = new Matcherie(nume, program, capacitate, new List<Matcha>(), new List<Rezervare>());
            admin.creazaMatcherie(m);
        }

        static void StergeMatcherie(AdministratorMatcha admin)
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

            if (target != null) admin.stergeMatcherie(target);
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
                                               ||
                                            ___||___
                                           /________\
                                          |  MATCHA  |
                                          |  LATTE   |
                                           \________/
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
        static void RandareEcranStart(SistemMatcha sistem, string asciiArt)
            {
                AnsiConsole.Clear();

                int w = AnsiConsole.Profile.Width;
                int h = AnsiConsole.Profile.Height;

                // fallback simplu dacă terminalul e mic
                if (w < 90 || h < 28)
                {
                    AnsiConsole.MarkupLine("[bold green]X Matcha[/]");
                    AnsiConsole.MarkupLine("[green]X marchează matcha[/]");
                    AnsiConsole.MarkupLine("[grey]1) Logare Client  2) Logare Administrator  3) Iesire[/]");
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

                // TITLU MARE (Figlet) doar dacă avem înălțime suficientă
                if (h >= 34)
                {
                    var fig = new FigletText("X Matcha").Color(Color.Green);
                    AnsiConsole.Write(Align.Center(fig));
                }
                else
                {
                    AnsiConsole.Write(new Panel(Align.Center(new Markup("[bold green]X Matcha[/]")))
                        .Border(BoxBorder.Double)
                        .BorderColor(Color.Green)
                        .Expand());
                }

                // SUBTITLU + descriere (mai mic decât titlul)
                var header = new Panel(
                        new Rows(
                            Align.Center(new Markup("[green]X marchează matcha[/]")),
                            Align.Center(new Markup("[grey]Prima aplicație care aduce în același loc clienții, managerii și matcheriile din propriul tău oraș![/]"))
                        ))
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green)
                    .Expand();

                AnsiConsole.Write(header);

                // Build panels: stats+charts, reviews, ascii
                var statsPanel = ConstruiestePanelStatisticiSiGrafice(sistem);
                var reviewsPanel = ConstruiestePanelTestimoniale();
                var asciiPanel = ConstruiestePanelAscii(asciiArt);

                // Layout adaptiv (în funcție de lățime)
                if (w >= 140)
                {
                    var grid = new Grid();
                    grid.AddColumn(new GridColumn()); // stânga
                    grid.AddColumn(new GridColumn()); // mijloc
                    grid.AddColumn(new GridColumn()); // dreapta
                    grid.AddRow(statsPanel, reviewsPanel, asciiPanel);
                    AnsiConsole.Write(grid);
                }
                else if (w >= 105)
                {
                    var grid = new Grid();
                    grid.AddColumn(new GridColumn());
                    grid.AddColumn(new GridColumn());
                    grid.AddRow(statsPanel, asciiPanel);
                    AnsiConsole.Write(grid);

                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(reviewsPanel);
                }
                else
                {
                    AnsiConsole.Write(statsPanel);
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(reviewsPanel);
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(asciiPanel);
                }

                // "Butoane" + instrucțiuni
                AnsiConsole.WriteLine();
                AnsiConsole.Write(
                    new Panel(new Markup(
                        "[black on green]   1  LOGARE CLIENT           [/]\n" +
                        "[black on green]   2  LOGARE ADMINISTRATOR    [/]\n\n" +
                        "[white on darkred]   3  IESIRE (sau ESC)        [/] \n\n" +
                        "[grey]Hint: poți redimensiona fereastra, UI-ul se reface automat. (R = refresh)[/]"
                    ))
                    .Header("[bold green]🔐 Start[/]")
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Green)
                    .Expand()
                );
            }

            static IRenderable ConstruiestePanelStatisticiSiGrafice(SistemMatcha sistem)
            {
                int nrMagazine = sistem.Magazine?.Count ?? 0;
                int nrClienti = sistem.Clienti?.Count ?? 0;
                int nrAdmins = sistem.Administratori?.Count ?? 0;

                int totalProduse = 0;
                int totalRezervari = 0;
                int totalCapacitate = 0;
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
                {
                    foreach (var c in sistem.Clienti)
                        totalTranzactii += (c.Istoric?.Count ?? 0);
                }

                // Chart 1 (dif. față de admin): Breakdown pe entități
                var breakdown = new BreakdownChart()
                    .Width(Math.Min(60, Math.Max(30, AnsiConsole.Profile.Width / 2 - 10)))
                    .AddItem("Produse", Math.Max(1, totalProduse), Color.Green)
                    .AddItem("Rezervari", Math.Max(1, totalRezervari), Color.Yellow)
                    .AddItem("Tranzactii", Math.Max(1, totalTranzactii), Color.Aqua)
                    .AddItem("Capacitate", Math.Max(1, totalCapacitate), Color.Olive);

                // Chart 2: Top 5 matcherii după rezervări
                var bar = new BarChart()
                    .Label("[green]Top matcherii (rezervări)[/]")
                    .CenterLabel();

                int barW = Math.Max(24, Math.Min(50, AnsiConsole.Profile.Width / 2 - 10));
                bar.Width(barW);

                var top = GetTopMatcheriiByRezervari(sistem, 5);
                if (top.Count == 0)
                    bar.AddItem("N/A", 0, Color.Grey);
                else
                    foreach (var x in top)
                        bar.AddItem(x.nume, x.val, Color.Green);

                var content = new Rows(
                    new Markup($"[bold]Magazine:[/] {nrMagazine}   [bold]Clienți:[/] {nrClienti}   [bold]Admini:[/] {nrAdmins}"),
                    new Markup($"[bold]Produse:[/] {totalProduse}   [bold]Rezervări:[/] {totalRezervari}   [bold]Tranzacții:[/] {totalTranzactii}"),
                    new Rule("[green]Overview[/]"),
                    breakdown,
                    new Rule("[green]Popularitate[/]"),
                    bar
                );

                return new Panel(content)
                    .Header("[bold green]📊 Statistici rețea[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green)
                    .Expand();
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

            static IRenderable ConstruiestePanelTestimoniale()
            {
                // testimoniale generate (poți schimba oricând)
                var reviews = new[]
                {
                    ("Alex B.", 5, "Matcha extraordinară, servicii premium și un personal foarte atent la nevoile clientului. Cu siguranță voi mai reveni!"),
                    ("Mara D.", 4, "Atmosferă super cozy, meniul e variat și matcha latte-ul e top. Aș mai adăuga doar câteva deserturi noi."),
                    ("Radu P.", 5, "Rezervarea a mers rapid, iar comanda a venit foarte repede. Calitate constantă, perfect pentru zile aglomerate."),
                    ("Ioana S.", 4, "Prețuri ok, locații bune, și îmi place că văd totul într-un singur loc. UI-ul e chiar fun.")
                };

                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn(new TableColumn("[bold]Rating[/]").Centered());
                table.AddColumn("Review");

                foreach (var r in reviews)
                {
                    string stars = Stele(r.Item2);
                    table.AddRow(
                        $"[yellow]{stars}[/]\n[grey]{Markup.Escape(r.Item1)}[/]",
                        $"[white]{Markup.Escape(r.Item3)}[/]"
                    );
                }

                return new Panel(table)
                    .Header("[bold green]⭐ Testimoniale[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green)
                    .Expand();
            }

            static string Stele(int n)
            {
                n = Math.Max(0, Math.Min(5, n));
                return new string('★', n) + new string('☆', 5 - n);
            }

            static IRenderable ConstruiestePanelAscii(string asciiArt)
            {
                // dacă ASCII-ul e prea lat, îl înlocuim cu un mesaj (ca să nu “strice” layout-ul)
                int maxLine = MaxLineLength(asciiArt);
                int w = AnsiConsole.Profile.Width;
                int safe = Math.Max(30, w / 3 - 6);

                IRenderable inner;
                if (maxLine > safe)
                {
                    inner = new Markup("[grey]ASCII art prea lat pentru fereastra curentă.\nMărește consola sau folosește un art mai îngust.[/]");
                }
                else
                {
                    // Text (nu Markup) ca să nu interpreteze [] sau alte simboluri
                    inner = Align.Center(new Text(asciiArt));
                }

                return new Panel(inner)
                    .Header("[bold green]🍵 Art[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green)
                    .Expand();
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