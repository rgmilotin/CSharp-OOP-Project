using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp5
{
    /// <summary>
    /// Flow-ul complet pentru client (selectare client + meniul client).
    /// </summary>
    public static class ClientFlow
    {
        public static void Run(SistemMatcha sistem)
        {
            var client = AccountService.AlegeClientDinSistem(sistem);
            if (client == null) return;

            client = AccountService.FixClientIfNeeded(sistem, client);

            bool inapoi = false;
            while (!inapoi)
            {
                Meniuri.AfiseazaDashboardClient(client, sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Ce dorești să faci?[/]")
                        .AddChoices(new[]
                        {
                            "Comandă Matcha",
                            "Rezervă Masă",
                            "Anulează Rezervare Masă",
                            "Istoric Tranzacții",
                            "Vizualizează Rezervări",
                            "Deconectare"
                        }));

                switch (optiune)
                {
                    case "Comandă Matcha":
                    {
                        if (sistem.Magazine == null || sistem.Magazine.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[red]Nu există matcherii în sistem.[/]");
                            CommonUI.Pauza();
                            break;
                        }

                        var magazin = AlegeMatcherieDinLista(sistem.Magazine);
                        if (magazin == null) break;

                        PlaseazaComanda(client, magazin, sistem);

                        CommonUI.Pauza();
                        break;
                    }

                    case "Rezervă Masă":
                    {
                        if (sistem.Magazine == null || sistem.Magazine.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[red]Nu există matcherii în sistem.[/]");
                            CommonUI.Pauza();
                            break;
                        }

                        var magazin = AlegeMatcherieDinLista(sistem.Magazine);
                        if (magazin == null) break;

                        var rez = CreeazaRezervareDinTip(client, magazin, sistem);
                        if (rez != null)
                            AnsiConsole.MarkupLine("[bold green]Rezervare adăugată cu succes![/]");

                        CommonUI.Pauza();
                        break;
                    }

                    case "Vizualizează Rezervări":
                        AfiseazaRezervari(client);
                        break;

                    case "Anulează Rezervare Masă":
                        AnuleazaRezervare(client);
                        break;

                    case "Istoric Tranzacții":
                        AfiseazaIstoric(client);
                        break;

                    case "Deconectare":
                        inapoi = true;
                        break;
                }
            }
        }

        // -------------------- COMANDA (CLIENTACCOUNT) --------------------

        private static Matcherie? AlegeMatcherieDinLista(List<Matcherie> magazine)
        {
            if (magazine == null || magazine.Count == 0) return null;

            // key unic -> matcherie
            var map = new Dictionary<string, Matcherie>();

            foreach (var m in magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                int libere = Math.Max(0, cap - rez);

                string keyBase = $"{m.Nume} | {m.Program} | {libere}/{cap} libere";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key))
                {
                    key = $"{keyBase} #{k}";
                    k++;
                }

                map[key] = m;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Alege matcheria[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys));

            return map[ales];
        }

        private static void PlaseazaComanda(ClientAccount client, Matcherie magazin, SistemMatcha sistem)
        {
            if (magazin.Meniu == null || magazin.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Momentan meniul este gol la această matcherie.[/]");
                return;
            }

            Console.Clear();
            magazin.AfiseazaMeniu();
            AnsiConsole.WriteLine();

            // map produs
            var map = new Dictionary<string, Matcha>();
            foreach (var p in magazin.Meniu)
            {
                string keyBase = $"{p.nume} - {p.pret} RON (stoc {p.cantitate})";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key))
                {
                    key = $"{keyBase} #{k}";
                    k++;
                }
                map[key] = p;
            }

            string produsKey = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Alege produsul[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys)
            );

            var produs = map[produsKey];

            if (produs.cantitate <= 0)
            {
                AnsiConsole.MarkupLine("[red]Produsul este indisponibil (stoc 0).[/]");
                return;
            }

            int qty;
            while (true)
            {
                qty = AnsiConsole.Ask<int>($"Cantitate [grey](1 - {produs.cantitate})[/]:");
                if (qty >= 1 && qty <= produs.cantitate) break;
                AnsiConsole.MarkupLine("[red]Cantitate invalidă.[/]");
            }

            decimal total = produs.pret * qty;

            var confirm = AnsiConsole.Confirm(
                $"Confirmi comanda: [green]{qty}x {Markup.Escape(produs.nume)}[/] = [yellow]{total} RON[/] ?");

            if (!confirm)
            {
                AnsiConsole.MarkupLine("[grey]Comanda anulată.[/]");
                return;
            }

            // update stoc + istoric
            produs.cantitate -= qty;
            client.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), DateTime.Now, total, magazin));

            CommonUI.SalvareSistem(sistem);

            var receipt = new Panel(new Rows(
                    new Markup($"[bold green]Comandă confirmată![/]"),
                    new Markup($"[grey]Client:[/] {Markup.Escape(client.Nume)}"),
                    new Markup($"[grey]Matcherie:[/] {Markup.Escape(magazin.Nume)}"),
                    new Markup($"[grey]Produs:[/] {Markup.Escape(produs.nume)} x {qty}"),
                    new Markup($"[grey]Total:[/] [yellow]{total} RON[/]"),
                    new Markup($"[grey]Stoc rămas:[/] {produs.cantitate}")
                ))
                .Header("[bold]🧾 Bon[/]")
                .BorderColor(Color.Green)
                .Expand();

            AnsiConsole.Write(receipt);
        }

        // -------------------- REZERVARE DIN TIPURI (SISTEM) --------------------

        private static Rezervare? CreeazaRezervareDinTip(ClientAccount client, Matcherie magazin, SistemMatcha sistem)
        {
            sistem.TipuriRezervari ??= new List<TipRezervare>();

            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri de rezervări definite.[/]");
                AnsiConsole.MarkupLine("[grey]Admin: Panou Admin → Tipuri Rezervări (CRUD) pentru a le adăuga.[/]");
                return null;
            }

            int cap = magazin.Capacitate <= 0 ? 0 : magazin.Capacitate;
            int ocupate = magazin.Rezervari?.Count ?? 0;

            if (cap == 0 || ocupate >= cap)
            {
                AnsiConsole.MarkupLine("[red]Ne pare rău, magazinul este plin![/]");
                return null;
            }

            var tipAles = AnsiConsole.Prompt(
                new SelectionPrompt<TipRezervare>()
                    .Title($"Alege tipul rezervării pentru [green]{Markup.Escape(magazin.Nume)}[/]:")
                    .PageSize(10)
                    .AddChoices(sistem.TipuriRezervari)
                    .UseConverter(t => $"{t.Nume} - {t.Pret} RON")
            );

            var detalii = new Panel(new Rows(
                    new Markup($"[bold]{Markup.Escape(tipAles.Nume)}[/]   [green]{tipAles.Pret} RON[/]"),
                    new Markup($"[grey]Limitări:[/] {Markup.Escape(tipAles.Limitari ?? "")}"),
                    new Markup($"[grey]Beneficii:[/] {Markup.Escape(tipAles.Beneficii ?? "")}"),
                    new Markup($"[grey]Locuri disponibile acum:[/] [white]{(cap - ocupate)}[/] din [white]{cap}[/]")
                ))
                .Header("[bold yellow]Detalii rezervare[/]")
                .BorderColor(Color.Orange1)
                .Expand();

            AnsiConsole.Write(detalii);

            if (!AnsiConsole.Confirm("Confirmi această rezervare?"))
            {
                AnsiConsole.MarkupLine("[grey]Rezervare anulată.[/]");
                return null;
            }

            bool existaDeja = client.Rezervari.Any(r =>
                (r.Matcherie?.Nume ?? "") == magazin.Nume &&
                string.Equals(r.Tip ?? "", tipAles.Nume ?? "", StringComparison.OrdinalIgnoreCase));

            if (existaDeja)
            {
                AnsiConsole.MarkupLine("[yellow]Ai deja o rezervare de acest tip la această matcherie.[/]");
                return null;
            }

            var rezNoua = new Rezervare(
                tipAles.Nume,
                tipAles.Pret,
                tipAles.Limitari,
                tipAles.Beneficii,
                client.Nume,
                magazin
            );

            magazin.Rezervari.Add(rezNoua);
            client.Rezervari.Add(rezNoua);

            CommonUI.SalvareSistem(sistem);
            return rezNoua;
        }

        // -------------------- TABEL REZERVARI --------------------

        private static void AfiseazaRezervari(ClientAccount client)
        {
            Console.Clear();

            if (client.Rezervari == null || client.Rezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă momentan.[/]");
                CommonUI.Pauza();
                return;
            }

            var tabel = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Orange1)
                .Title("[bold orange1]📌 REZERVĂRILE TALE[/]")
                .Caption("[grey]Total rezervări active: " + client.Rezervari.Count + "[/]");

            tabel.AddColumn("[bold]Nr.[/]");
            tabel.AddColumn("[bold]Locație[/]");
            tabel.AddColumn("[bold]Tip Rezervare[/]");
            tabel.AddColumn("[bold]Beneficii[/]");
            tabel.AddColumn(new TableColumn("[bold]Preț[/]").Centered());

            for (int i = 0; i < client.Rezervari.Count; i++)
            {
                var rez = client.Rezervari[i];
                tabel.AddRow(
                    (i + 1).ToString(),
                    $"[cyan]{rez.Matcherie?.Nume ?? "Nespecificat"}[/]",
                    rez.Tip,
                    $"[italic grey]{rez.Beneficii}[/]",
                    $"[green]{rez.Pret} RON[/]"
                );
            }

            AnsiConsole.Write(tabel);
            CommonUI.Pauza();
        }

        // -------------------- ANULARE REZERVARE --------------------

        private static void AnuleazaRezervare(ClientAccount client)
        {
            if (client.Rezervari == null || client.Rezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă de anulat.[/]");
                CommonUI.Pauza();
                return;
            }

            var rezervareDeAnulat = AnsiConsole.Prompt(
                new SelectionPrompt<Rezervare>()
                    .Title("Selectează rezervarea pe care dorești să o [red]anulezi[/]:")
                    .PageSize(10)
                    .AddChoices(client.Rezervari)
                    .UseConverter(r =>
                    {
                        string numeEscapat = Markup.Escape(r.Matcherie?.Nume ?? "Nespecificat");
                        string tipEscapat = Markup.Escape(r.Tip ?? "Rezervare");
                        return $"[[{numeEscapat}]] {tipEscapat} - [green]{r.Pret} RON[/]";
                    }));

            if (AnsiConsole.Confirm($"Sigur dorești să anulezi rezervarea [yellow]{Markup.Escape(rezervareDeAnulat.Tip)}[/]?"))
            {
                rezervareDeAnulat.Matcherie?.Rezervari.Remove(rezervareDeAnulat);
                client.Rezervari.Remove(rezervareDeAnulat);

                AnsiConsole.MarkupLine("[bold green]Rezervarea a fost anulată cu succes![/]");
            }

            CommonUI.Pauza();
        }

        // -------------------- ISTORIC TRANZACTII --------------------

        private static void AfiseazaIstoric(ClientAccount client)
        {
            Console.Clear();

            if (client.Istoric == null || client.Istoric.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu ai nicio tranzacție înregistrată.[/]");
                CommonUI.Pauza();
                return;
            }

            var tabel = new Table()
                .Border(TableBorder.DoubleEdge)
                .Title("[bold magenta]🧾 ISTORIC CUMPĂRĂTURI[/]")
                .BorderColor(Color.Magenta1);

            tabel.AddColumn("Dată");
            tabel.AddColumn("Magazin");
            tabel.AddColumn(new TableColumn("Preț").RightAligned());

            foreach (var t in client.Istoric)
            {
                tabel.AddRow(
                    t.Data.ToString("dd/MM/yyyy HH:mm"),
                    Markup.Escape(t.Matcherie?.Nume ?? "N/A"),
                    $"[green]{t.suma} RON[/]"
                );
            }

            AnsiConsole.Write(tabel);
            CommonUI.Pauza();
        }
    }
}
