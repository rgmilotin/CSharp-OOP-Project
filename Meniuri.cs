using Spectre.Console;
using System.Text;

namespace ConsoleApp5
{
    public static class Meniuri
    {
        public static void AfiseazaDashboardClient(ClientAccount client, SistemMatcha sistem)
        {
            Console.Clear();

            // Heuristică simplă ca să nu “mănânce” ecranul (și să rămână loc pentru prompt)
            int h = AnsiConsole.Profile.Height;
            int maxMatcherii = h < 32 ? 2 : (h < 40 ? 3 : 4);
            int maxProdusePerMatcherie = h < 32 ? 2 : (h < 40 ? 3 : 4);

            // Layout 2 coloane
            var root = new Layout("Root");
            var left = new Layout("Meniu");
            var right = new Layout("Profil");
            root.SplitColumns(left, right);
            
            left.Ratio = 2;
            right.Ratio = 1;

            // -------------------- STÂNGA: tabel compact cu matcherii + MENIU per matcherie --------------------
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold green]🍵 MATCHERII & MENIURI[/]");

            t.AddColumn("Locație");
            t.AddColumn("Program");
            t.AddColumn(new TableColumn("Locuri libere").RightAligned());
            t.AddColumn("Meniu (preview)");

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                t.AddRow("[red]N/A[/]", "[red]N/A[/]", "-", "[grey]Nu există matcherii[/]");
            }
            else
            {
                
                var list = sistem.Magazine
                    .OrderBy(m => m.Nume)
                    .Take(maxMatcherii)
                    .ToList();

                foreach (var m in list)
                {
                    int rez = m.Rezervari?.Count ?? 0;
                    int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                    int libere = Math.Max(0, cap - rez);

                    string locuriCell = libere > 0
                        ? $"[green]{libere}/{cap}[/]"
                        : $"[red]{libere}/{cap}[/]";

                    string meniuCell = BuildMeniuPreview(m, maxProdusePerMatcherie);

                    t.AddRow(
                        $"[white]{Markup.Escape(m.Nume)}[/]",
                        $"[grey]{Markup.Escape(m.Program)}[/]",
                        locuriCell,
                        meniuCell
                    );
                }

                // Dacă există mai multe matcherii decât afișăm
                if (sistem.Magazine.Count > maxMatcherii)
                {
                    t.AddRow(
                        "[grey]…[/]",
                        "[grey](mai multe locații)[/]",
                        "[grey]…[/]",
                        $"[grey]Afișate {maxMatcherii} din {sistem.Magazine.Count}[/]"
                    );
                }
            }

            var leftPanel = new Panel(t)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Header("[bold green]Rețea[/]")
                .Expand();

            // -------------------- DREAPTA: profil scurt --------------------
            int rezCount = client.Rezervari?.Count ?? 0;
            int ordersCount = client.Istoric?.Count ?? 0;

            var profil = new Rows(
                new Markup($"[bold]Utilizator:[/] {Markup.Escape(client.Nume)}"),
                new Markup($"[bold]Email:[/] [blue]{Markup.Escape(client.Email)}[/]"),
                new Rule("[yellow]Activitate[/]"),
                new Markup($"[bold]Rezervări:[/] [yellow]{rezCount}[/]"),
                new Markup($"[bold]Comenzi:[/] [green]{ordersCount}[/]"),
                new Rule(),
                new Markup("[grey]Opțiunile sunt afișate imediat sub dashboard[/]")
            );

            var rightPanel = new Panel(profil)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Header("[bold cyan]👤 Profil[/]")
                .Expand();

            left.Update(leftPanel);
            right.Update(rightPanel);

            AnsiConsole.Write(root);
            AnsiConsole.WriteLine();
        }

        private static string BuildMeniuPreview(Matcherie m, int maxItems)
        {
            if (m.Meniu == null || m.Meniu.Count == 0)
                return "[grey italic]În curând... (meniu indisponibil)[/]";

            // maxItems produse, restul “… (+X)”
            int take = Math.Min(maxItems, m.Meniu.Count);
            int extra = m.Meniu.Count - take;

            var sb = new StringBuilder();

            for (int i = 0; i < take; i++)
            {
                var p = m.Meniu[i];
                sb.Append($"[green]•[/] {Markup.Escape(p.nume)} [grey]({p.pret} RON)[/]");
                if (i < take - 1) sb.Append('\n');
            }

            if (extra > 0)
            {
                sb.Append('\n');
                sb.Append($"[grey]… (+{extra} produse)[/]");
            }

            return sb.ToString();
        }
    }
}
