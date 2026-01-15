using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApp5
{
    /// Full Admin flow: login + dashboard + submenus (Matcheries/Reservation Types/Transactions/Monitoring).
    /// No dependency on AdministratorMatcha class (logic is here).
    public static class AdminFlow
    {
        public static void Run(SistemMatcha sistem)
        {
            EnsureCollections(sistem);

            var admin = LoginAdmin(sistem);
            if (admin == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();
                AfiseazaDashboardAdmin(admin, sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold red]ADMIN PANEL[/] - Salut, [white]{Markup.Escape(admin.Nume)}[/]")
                        .AddChoices(new[]
                        {
                            "1) Manage Matcheries (CRUD)",
                            "2) Reservation Types (CRUD)",
                            "3) Transactions (create/modify/assign client)",
                            "4) Activity Monitoring",
                            "5) Create Admin Account",
                            "Logout"
                        }));

                switch (optiune)
                {
                    case "1) Manage Matcheries (CRUD)":
                        SubmeniuMatcheries(sistem);
                        break;

                    case "2) Reservation Types (CRUD)":
                        SubmeniuReservationTypes(sistem);
                        break;

                    case "3) Transactions (create/modify/assign client)":
                        SubmeniuTransactions(sistem);
                        break;

                    case "4) Activity Monitoring":
                        AfiseazaMonitorizare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "5) Create Admin Account":
                        AccountService.CreeazaAdminNou(sistem);
                        break;

                    case "Logout":
                        inapoi = true;
                        break;
                }
            }
        }

        // -------------------- SAFETY --------------------

        private static void EnsureCollections(SistemMatcha sistem)
        {
            sistem.Magazine ??= new List<Matcherie>();
            sistem.Clienti ??= new List<ClientAccount>();
            sistem.Administratori ??= new List<AdminAccount>();
            sistem.TipuriRezervari ??= new List<TipRezervare>();
        }

        // -------------------- LOGIN --------------------

        private static AdminAccount? LoginAdmin(SistemMatcha sistem)
        {
            if (sistem.Administratori == null || sistem.Administratori.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No admins found in the system.[/]");
                CommonUI.Pauza();
                return null;
            }

            for (int incercari = 0; incercari < 3; incercari++)
            {
                Console.Clear();
                AnsiConsole.Write(new Rule("[red]Admin Login[/]").RuleStyle("grey"));

                string id = AnsiConsole.Ask<string>("Admin ID:");
                string parola = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

                var found = sistem.Administratori.FirstOrDefault(a =>
                    a.AdminId == id && a.Parola == parola);

                if (found != null)
                {
                    AnsiConsole.MarkupLine("[green]Login successful![/]");
                    Thread.Sleep(250);
                    return found;
                }

                AnsiConsole.MarkupLine("[red]Invalid credentials. Try again.[/]");
                Thread.Sleep(600);
            }

            AnsiConsole.MarkupLine("[red]Too many attempts. Returning...[/]");
            CommonUI.Pauza();
            return null;
        }

        // -------------------- DASHBOARD --------------------

        private static void AfiseazaDashboardAdmin(AdminAccount admin, SistemMatcha sistem)
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

            var info = new Panel(
                new Rows(
                    new Markup($"[bold]Admin:[/] {Markup.Escape(admin.Nume)} ([grey]{Markup.Escape(admin.AdminId)}[/])"),
                    new Markup($"[bold]Matcheries:[/] {nrMagazine}"),
                    new Markup($"[bold]Clients:[/] {nrClienti}"),
                    new Markup($"[bold]Active reservations:[/] {rezervariActive}"),
                    new Markup($"[bold]Total transactions:[/] {tranzactii}")
                ))
                .Header("[bold red]📌 ADMIN DASHBOARD[/]")
                .BorderColor(Color.Red)
                .Expand();

            // last 7 days transactions count
            var chart = new BarChart()
                .Label("[green]Sales (last 7 days)[/]")
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

        // -------------------- MATCHERIES --------------------

        private static void SubmeniuMatcheries(SistemMatcha sistem)
        {
            EnsureCollections(sistem);

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Manage Matcheries[/]")
                        .AddChoices(new[]
                        {
                            "View matcheries report",
                            "Create matchery",
                            "Edit matchery (schedule/capacity)",
                            "Delete matchery",
                            "Products menu",
                            "Back"
                        }));

                switch (opt)
                {
                    case "View matcheries report":
                        Console.Clear();
                        AfiseazaRaportMatcheries(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Create matchery":
                        CreeazaMatchery(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Edit matchery (schedule/capacity)":
                        ModificaMatchery(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Delete matchery":
                        StergeMatchery(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Products menu (CRUD)":
                        SubmeniuProduse(sistem);
                        break;

                    case "Back":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void AfiseazaRaportMatcheries(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries yet.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Matcheries Report[/]");
            t.AddColumn("Name");
            t.AddColumn("Schedule");
            t.AddColumn(new TableColumn("Capacity").RightAligned());
            t.AddColumn(new TableColumn("Products").RightAligned());
            t.AddColumn(new TableColumn("Reservations").RightAligned());
            t.AddColumn(new TableColumn("Occupancy").RightAligned());

            foreach (var m in sistem.Magazine)
            {
                int prod = m.Meniu?.Count ?? 0;
                int rez = m.Rezervari?.Count ?? 0;
                int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                int pct = (int)Math.Round(100.0 * rez / cap);

                t.AddRow(
                    Markup.Escape(m.Nume),
                    Markup.Escape(m.Program),
                    m.Capacitate.ToString(),
                    prod.ToString(),
                    rez.ToString(),
                    $"{pct}%"
                );
            }

            AnsiConsole.Write(t);
        }

        private static void CreeazaMatchery(SistemMatcha sistem)
        {
            if (sistem.Magazine == null) sistem.Magazine = new List<Matcherie>();

            string nume = AnsiConsole.Ask<string>("Matchery name:");
            string program = AnsiConsole.Ask<string>("Schedule (e.g. 08:00-22:00):");
            int capacitate = AnsiConsole.Ask<int>("Capacity:");

            // unique by name (case-insensitive)
            if (sistem.Magazine.Any(m => string.Equals(m.Nume, nume, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] A matchery named [yellow]{Markup.Escape(nume)}[/] already exists.");
                return;
            }

            sistem.Magazine.Add(new Matcherie(nume, program, capacitate, new List<Matcha>(), new List<Rezervare>()));
            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Matchery created and saved.[/]");
        }

        private static void ModificaMatchery(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries to edit.[/]");
                return;
            }

            var ales = AlegeMatchery(sistem);
            if (ales == null) return;

            var camp = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Edit [green]{Markup.Escape(ales.Nume)}[/]:")
                    .AddChoices(new[] { "Schedule", "Capacity", "Cancel" }));

            if (camp == "Cancel") return;

            if (camp == "Schedule")
            {
                string nou = AnsiConsole.Ask<string>($"New schedule (current: {Markup.Escape(ales.Program)}):");
                ales.SetProgram(nou);
                AnsiConsole.MarkupLine("[green]Schedule updated.[/]");
            }
            else if (camp == "Capacity")
            {
                int noua = AnsiConsole.Ask<int>($"New capacity (current: {ales.Capacitate}):");
                ales.SetCapacitate(noua);
                AnsiConsole.MarkupLine("[green]Capacity updated.[/]");
            }

            CommonUI.SalvareSistem(sistem);
        }

        private static void StergeMatchery(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries to delete.[/]");
                return;
            }

            var ales = AlegeMatchery(sistem);
            if (ales == null) return;

            if (!AnsiConsole.Confirm($"Delete matchery [red]{Markup.Escape(ales.Nume)}[/] permanently?")) return;

            sistem.Magazine.Remove(ales);
            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Matchery deleted.[/]");
        }

        // -------------------- PRODUCTS (CRUD) --------------------

        private static void SubmeniuProduse(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries. Create one first.[/]");
                CommonUI.Pauza();
                return;
            }

            Matcherie? matcherie = AlegeMatchery(sistem);
            if (matcherie == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Products[/] for: [white]{Markup.Escape(matcherie.Nume)}[/]");
                AnsiConsole.WriteLine();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose action:")
                        .AddChoices(new[]
                        {
                            "View menu",
                            "Add product",
                            "Edit product",
                            "Delete product",
                            "Switch matchery",
                            "Back"
                        }));

                switch (opt)
                {
                    case "View menu":
                        Console.Clear();
                        AfiseazaMeniuSafe(matcherie);
                        CommonUI.Pauza();
                        break;

                    case "Add product":
                        AdaugaProdus(matcherie, sistem);
                        CommonUI.Pauza();
                        break;

                    case "Edit product":
                        ModificaProdus(matcherie, sistem);
                        CommonUI.Pauza();
                        break;

                    case "Delete product":
                        StergeProdus(matcherie, sistem);
                        CommonUI.Pauza();
                        break;

                    case "Switch matchery":
                        matcherie = AlegeMatchery(sistem);
                        if (matcherie == null) return;
                        break;

                    case "Back":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void AfiseazaMeniuSafe(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Menu is empty.[/]");
                return;
            }

            matcherie.AfiseazaMeniu();
        }

        private static void AdaugaProdus(Matcherie matcherie, SistemMatcha sistem)
        {
            string nume = AnsiConsole.Ask<string>("Product name:");

            if (matcherie.Meniu.Any(p => string.Equals(p.nume, nume, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]A product with this name already exists.[/]");
                return;
            }

            string descriere = AnsiConsole.Ask<string>("Description:");
            decimal pret = AnsiConsole.Ask<decimal>("Price (RON):");
            int cantitate = AnsiConsole.Ask<int>("Stock quantity:");
            int calorii = AnsiConsole.Ask<int>("Calories:");

            var coord = ServiceLocator.Get<SistemCoordinator>();

            bool ok = coord.TryAdaugaProdus(
                matcherie,
                new Matcha(nume, descriere, pret, cantitate, calorii),
                out string mesaj
            );

            if (!ok)
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(mesaj)}[/]");
                return;
            }

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(mesaj)}[/]");
        }


        private static void ModificaProdus(Matcherie matcherie, SistemMatcha sistem)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No products to edit.[/]");
                return;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose product:")
                    .AddChoices(matcherie.Meniu.Select(p => p.nume)));

            var produs = matcherie.Meniu.FirstOrDefault(p => p.nume == ales);
            if (produs == null) return;

            var camp = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Edit field:")
                    .AddChoices(new[] { "Name", "Description", "Price", "Stock", "Calories", "Cancel" }));

            if (camp == "Cancel") return;

            if (camp == "Name")
            {
                string nou = AnsiConsole.Ask<string>($"New name (current: {Markup.Escape(produs.nume)}):");

                if (matcherie.Meniu.Any(p => p != produs && string.Equals(p.nume, nou, StringComparison.OrdinalIgnoreCase)))
                {
                    AnsiConsole.MarkupLine("[red]Another product already uses this name.[/]");
                    return;
                }

                produs.nume = nou;
            }
            else if (camp == "Description")
            {
                produs.descriere = AnsiConsole.Ask<string>($"New description (current: {Markup.Escape(produs.descriere)}):");
            }
            else if (camp == "Price")
            {
                produs.pret = AnsiConsole.Ask<decimal>($"New price (current: {produs.pret}):");
            }
            else if (camp == "Stock")
            {
                produs.cantitate = AnsiConsole.Ask<int>($"New stock (current: {produs.cantitate}):");
            }
            else if (camp == "Calories")
            {
                produs.calorii = AnsiConsole.Ask<int>($"New calories (current: {produs.calorii}):");
            }

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Product updated.[/]");
        }

        private static void StergeProdus(Matcherie matcherie, SistemMatcha sistem)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No products to delete.[/]");
                return;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose product to delete:")
                    .AddChoices(matcherie.Meniu.Select(p => p.nume)));

            var produs = matcherie.Meniu.FirstOrDefault(p => p.nume == ales);
            if (produs == null) return;

            if (!AnsiConsole.Confirm($"Delete [red]{Markup.Escape(produs.nume)}[/]?")) return;

            matcherie.Meniu.Remove(produs);
            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Product deleted.[/]");
        }

        // -------------------- RESERVATION TYPES --------------------

        private static void SubmeniuReservationTypes(SistemMatcha sistem)
        {
            sistem.TipuriRezervari ??= new List<TipRezervare>();

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Reservation Types (Admin)[/]")
                        .AddChoices(new[] { "List types", "Add type", "Edit type", "Delete type", "Back" }));

                switch (opt)
                {
                    case "List types":
                        AfiseazaTipuriRezervari(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Add type":
                        AdaugaTipRezervare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Edit type":
                        ModificaTipRezervare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Delete type":
                        StergeTipRezervare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Back":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void AfiseazaTipuriRezervari(SistemMatcha sistem)
        {
            Console.Clear();

            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No reservation types defined.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Reservation Types[/]");
            t.AddColumn("Name");
            t.AddColumn(new TableColumn("Price").RightAligned());
            t.AddColumn("Limitations");
            t.AddColumn("Benefits");

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

        private static void AdaugaTipRezervare(SistemMatcha sistem)
        {
            string nume = AnsiConsole.Ask<string>("Type name (e.g. Family, Friends):");
            decimal pret = AnsiConsole.Ask<decimal>("Price:");
            string lim = AnsiConsole.Ask<string>("Limitations:");
            string ben = AnsiConsole.Ask<string>("Benefits:");

            if (sistem.TipuriRezervari.Any(x => string.Equals(x.Nume, nume, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]Type name already exists.[/]");
                return;
            }

            sistem.TipuriRezervari.Add(new TipRezervare(nume, pret, lim, ben));
            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Type added.[/]");
        }

        private static void ModificaTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No types to edit.[/]");
                return;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose type to edit:")
                    .AddChoices(sistem.TipuriRezervari.Select(x => x.Nume)));

            var tip = sistem.TipuriRezervari.FirstOrDefault(x => x.Nume == ales);
            if (tip == null) return;

            tip.Pret = AnsiConsole.Ask<decimal>($"New price (current {tip.Pret}):");
            tip.Limitari = AnsiConsole.Ask<string>($"New limitations (current: {Markup.Escape(tip.Limitari)}):");
            tip.Beneficii = AnsiConsole.Ask<string>($"New benefits (current: {Markup.Escape(tip.Beneficii)}):");

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Type updated.[/]");
        }

        private static void StergeTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No types to delete.[/]");
                return;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose type to delete:")
                    .AddChoices(sistem.TipuriRezervari.Select(x => x.Nume)));

            var tip = sistem.TipuriRezervari.FirstOrDefault(x => x.Nume == ales);
            if (tip == null) return;

            if (!AnsiConsole.Confirm($"Delete type [red]{Markup.Escape(tip.Nume)}[/]?")) return;

            sistem.TipuriRezervari.Remove(tip);
            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Type deleted.[/]");
        }

        // -------------------- TRANSACTIONS --------------------

        private static void SubmeniuTransactions(SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Transactions (Admin)[/]")
                        .AddChoices(new[]
                        {
                            "View all transactions (from clients history)",
                            "Create transaction for a client",
                            "Modify a transaction (replace)",
                            "Back"
                        }));

                switch (opt)
                {
                    case "View all transactions (from clients history)":
                        AfiseazaToateTranzactiile(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Create transaction for a client":
                        CreeazaTranzactiePentruClient(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Modify a transaction (replace)":
                        ModificaTranzactieInlocuire(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Back":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void AfiseazaToateTranzactiile(SistemMatcha sistem)
        {
            Console.Clear();

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]All Transactions[/]");
            table.AddColumn("Client");
            table.AddColumn("Date");
            table.AddColumn("Matchery");
            table.AddColumn(new TableColumn("Amount").RightAligned());

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
                AnsiConsole.MarkupLine("[yellow]No transactions found.[/]");
                return;
            }

            AnsiConsole.Write(table);
        }

        private static void CreeazaTranzactiePentruClient(SistemMatcha sistem)
        {
            EnsureCollections(sistem);

            if (sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No clients in system.[/]");
                return;
            }

            if (sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No matcheries in system.[/]");
                return;
            }

            // client choose
            string clientKey = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select client:")
                    .AddChoices(sistem.Clienti.Select(c => $"{c.Nume} ({c.Email})")));

            var client = sistem.Clienti.FirstOrDefault(c => $"{c.Nume} ({c.Email})" == clientKey);
            if (client == null) return;

            client = AccountService.FixClientIfNeeded(sistem, client);

            // matchery choose
            var magazin = AlegeMatchery(sistem);
            if (magazin == null) return;

            decimal suma = AnsiConsole.Ask<decimal>("Amount (RON):");
            client.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), DateTime.Now, suma, magazin));

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Transaction added and assigned to client.[/]");
        }

        private static void ModificaTranzactieInlocuire(SistemMatcha sistem)
        {
            EnsureCollections(sistem);

            if (sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No clients.[/]");
                return;
            }

            string clientKey = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select client:")
                    .AddChoices(sistem.Clienti.Select(c => $"{c.Nume} ({c.Email})")));

            var client = sistem.Clienti.FirstOrDefault(c => $"{c.Nume} ({c.Email})" == clientKey);
            if (client == null) return;

            client = AccountService.FixClientIfNeeded(sistem, client);

            if (client.Istoric == null || client.Istoric.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Client has no transactions.[/]");
                return;
            }

            var tranzList = client.Istoric
                .Select(t => $"{t.Data:dd/MM HH:mm} - {t.Matcherie?.Nume ?? "N/A"} - {t.suma} RON")
                .ToList();

            string tranzAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose transaction to modify:")
                    .AddChoices(tranzList));

            int index = tranzList.IndexOf(tranzAles);
            if (index < 0) return;

            var tranzSelectata = client.Istoric[index];

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No matcheries.[/]");
                return;
            }

            var magazinNou = AlegeMatchery(sistem);
            if (magazinNou == null) return;

            decimal sumaNoua = AnsiConsole.Ask<decimal>($"New amount (current {tranzSelectata.suma}):");

            // replace (preserve Id & Date)
            client.Istoric[index] = new Tranzactie(tranzSelectata.Id, tranzSelectata.Data, sumaNoua, magazinNou);

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Transaction updated (replaced).[/]");
        }

        // -------------------- MONITORING --------------------

        private static void AfiseazaMonitorizare(SistemMatcha sistem)
        {
            Console.Clear();

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Activity Monitoring[/]");
            t.AddColumn("Matchery");
            t.AddColumn(new TableColumn("Active reservations").RightAligned());
            t.AddColumn(new TableColumn("Capacity").RightAligned());
            t.AddColumn(new TableColumn("Occupancy").RightAligned());

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

        // -------------------- COMMON PICKERS --------------------

        private static Matcherie? AlegeMatchery(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0) return null;

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select matchery:")
                    .PageSize(10)
                    .AddChoices(sistem.Magazine.Select(m => m.Nume)));

            return sistem.Magazine.FirstOrDefault(m => m.Nume == ales);
        }
    }
}
