using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text;

namespace ConsoleApp5
{
    /// Start Screen complet (ASCII, statistici, reviews) + input non-blocking.
    public static class StartScreen
    {
        public static string AfiseazaEcranStartSiAlegeRol(SistemMatcha sistem)
        {
            string ascii = IncarcaAsciiArtStart();
            int lastW = -1, lastH = -1;

            while (true)
            {
                int w = Console.WindowWidth;
                int h = Console.WindowHeight;

                // Re-randare la resize
                if (w != lastW || h != lastH)
                {
                    lastW = w;
                    lastH = h;
                    RandareEcranStart(sistem, ascii);
                }

                // Input non-blocking (1/2/3/4/ESC)
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1)
                        return "Client";

                    if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2)
                        return "Administrator";

                    if (key.Key == ConsoleKey.D3 || key.Key == ConsoleKey.NumPad3)
                        return "CreareCont";

                    if (key.Key == ConsoleKey.D4 || key.Key == ConsoleKey.NumPad4 || key.Key == ConsoleKey.Escape)
                        return "Ieșire";

                    // R = refresh manual
                    if (key.Key == ConsoleKey.R)
                    {
                        lastW = -1;
                        lastH = -1;
                    }
                }

                Thread.Sleep(80);
            }
        }

        // -------------------- RENDER START --------------------

        private static void RandareEcranStart(SistemMatcha sistem, string asciiArt)
        {
            AnsiConsole.Clear();

            int w = AnsiConsole.Profile.Width;
            int h = AnsiConsole.Profile.Height;

            // Fallback pentru ferestre mici
            if (w < 90 || h < 28)
            {
                AnsiConsole.MarkupLine("[bold green]X Matcha[/]");
                AnsiConsole.MarkupLine("[green]X marchează matcha[/]");
                AnsiConsole.MarkupLine("[grey]1) Logare Client  2) Logare Administrator  3) Creare cont  4) Ieșire (ESC)[/]");
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

            // ASCII dictează înălțimea card-urilor
            int maxBody = Math.Clamp(h - 26, 12, 30);

            string asciiNormalized = (asciiArt ?? "").Replace("\r", "").TrimEnd('\n');
            int asciiNaturalLines = CountLines(asciiNormalized);
            int cardLines = Math.Clamp(asciiNaturalLines, 12, maxBody);

            if (w >= 140)
            {
                int colW = Math.Max(34, (w - 10) / 3);

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
                        "[black on green]   2  LOGARE ADMINISTRATOR    [/]\n" +
                        "[black on green]   3  CREARE CONT CLIENT      [/]\n\n" +
                        "[white on darkred]   4  IESIRE (sau ESC)        [/] \n\n" +
                        "[grey]Hint: poți redimensiona fereastra, UI-ul se reface automat. (R = refresh)[/]"
                ))
                .Header("[bold green]Start[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Expand()
            );
        }

        // -------------------- PANELS --------------------

        private static IRenderable ConstruiestePanelStatisticiSiGrafice(SistemMatcha sistem, int colWidth, int cardLines)
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
                    .AddItem("Produse", totalProduse, Color.Aqua)
                    .AddItem("Rezervari", totalRezervari, Color.Yellow)
                    .AddItem("Tranzactii", totalTranzactii, Color.Magenta1);

                rows.Add(breakdownAct);
            }

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

            rows.Add(new Rule("[green]Popularitate[/]"));
            var top = StatsService.GetTopMatcheriiByRezervari(sistem, 3);

            if (top.Count == 0) rows.Add(new Markup("[grey]N/A[/]"));
            else
            {
                int nameW = Math.Max(10, colWidth - 14);
                foreach (var x in top)
                {
                    string name = CutNoDots(x.nume, nameW);
                    rows.Add(new Markup($"[white]{Markup.Escape(name)}[/]  [green]{x.val}[/]"));
                }
            }

            int used = rows.Count;
            int pad = Math.Max(0, cardLines - used);
            rows.Add(new Text(BlankLines(pad)));

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold green]Statistici rețea[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }

        private static IRenderable ConstruiestePanelTestimoniale(int colWidth, int cardLines)
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

            int remaining = Math.Max(0, cardLines - rows.Count);

            foreach (var r in reviews)
            {
                string stars = Stele(r.Item2);
                string author = r.Item1;
                string text = r.Item3;

                string prefixPlain = $"{stars} {author} - ";
                int prefixLen = prefixPlain.Length;

                int firstTextW = Math.Max(5, innerW - prefixLen);
                string indent = new string(' ', prefixLen);
                int nextTextW = Math.Max(5, innerW - indent.Length);

                var wrapped = WrapWithFirstWidth(text, firstTextW, nextTextW);
                int needed = wrapped.Count + 1;

                if (needed > remaining)
                    break;

                rows.Add(new Markup(
                    $"[yellow]{Markup.Escape(stars)}[/] " +
                    $"[green]{Markup.Escape(author)}[/] " +
                    $"[grey]-[/] " +
                    $"[white]{Markup.Escape(wrapped[0])}[/]"
                ));

                for (int i = 1; i < wrapped.Count; i++)
                    rows.Add(new Markup($"[white]{Markup.Escape(indent + wrapped[i])}[/]"));

                rows.Add(new Text(""));
                remaining -= needed;
            }

            int used = rows.Count;
            int pad = Math.Max(0, cardLines - used);
            rows.Add(new Text(BlankLines(pad)));

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold green]Testimoniale[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }

        private static IRenderable ConstruiestePanelAscii(string asciiArt, int colWidth, int cardLines)
        {
            asciiArt ??= "";
            string normalized = asciiArt.Replace("\r", "").TrimEnd('\n');

            int safeWidth = Math.Max(20, colWidth - 8);
            int maxLine = MaxLineLength(normalized);

            string body = (maxLine > safeWidth)
                ? "ASCII art prea lat pentru coloana curentă.\nMărește fereastra sau folosește un art mai îngust."
                : normalized;

            string fixedLines = PadOrClipToLines(body, cardLines);

            return new Panel(Align.Center(new Text(fixedLines)))
                .Header("[bold green]Art[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }

        // -------------------- ASCII LOADER --------------------

        private static string IncarcaAsciiArtStart()
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
                // fallback
            }

            return fallback;
        }

        // -------------------- HELPERS --------------------

        private static string BlankLines(int n)
        {
            if (n <= 0) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < n; i++) sb.AppendLine();
            return sb.ToString();
        }

        private static int CountLines(string s)
        {
            if (string.IsNullOrEmpty(s)) return 1;
            return s.Replace("\r", "").Split('\n').Length;
        }

        private static int MaxLineLength(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            var lines = s.Replace("\r", "").Split('\n');
            int max = 0;
            foreach (var line in lines)
                if (line.Length > max) max = line.Length;
            return max;
        }

        private static string CutNoDots(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || max <= 0) return "";
            if (s.Length <= max) return s;
            return s.Substring(0, max);
        }

        private static string PadOrClipToLines(string s, int linesWanted)
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

            for (int i = take; i < linesWanted; i++)
                sb.Append('\n');

            return sb.ToString();
        }

        private static string Stele(int n)
        {
            n = Math.Clamp(n, 0, 5);
            return new string('★', n) + new string('☆', 5 - n);
        }

        private static List<string> WrapWithFirstWidth(string text, int firstWidth, int nextWidth)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                result.Add("");
                return result;
            }

            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
                        currentWidth = nextWidth;
                    }

                    idx++;
                    continue;
                }

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
                        currentWidth = nextWidth;
                    }
                }
            }

            if (!string.IsNullOrEmpty(line))
                result.Add(line);

            if (result.Count == 0) result.Add("");
            return result;
        }

        // -------------------- Animatie Steam --------------------

        public static void RulareAnimatieMatchaSteam()
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
