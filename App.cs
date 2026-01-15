using Spectre.Console;

namespace ConsoleApp5
{
    /// <summary>
    /// Orchestrator-ul aplicației (load, seed, start loop, routing).
    /// </summary>
    public static class App
    {
        public static void Run()
        {
            // 1) Inițializare sistem (load JSON)
            SistemMatcha sistem = GestiuneDate.IncarcaTot();
            EnsureCollections(sistem);

            // Seed dacă nu există admini (comportamentul tău existent)
            if (sistem.Administratori.Count == 0)
                TestDataSeeder.IncarcaDateTest(sistem);

            // Titlu inițial
            AnsiConsole.Write(new FigletText("Matcha System").Color(Color.Green));

            // 2) Loop principal
            bool ruleazaProgramul = true;
            while (ruleazaProgramul)
            {
                string rol = StartScreen.AfiseazaEcranStartSiAlegeRol(sistem);

                switch (rol)
                {
                    case "Administrator":
                        AdminFlow.Run(sistem);
                        break;

                    case "Client":
                        ClientFlow.Run(sistem);
                        break;

                    case "CreareCont":
                        AccountService.CreeazaContClient(sistem);
                        break;

                    case "Ieșire":
                        CommonUI.SalvareSistem(sistem);
                        ruleazaProgramul = false;
                        break;
                }
            }
        }

        /// <summary>
        /// Protecție la liste null după deserializare.
        /// </summary>
        private static void EnsureCollections(SistemMatcha sistem)
        {
            sistem.Magazine ??= new List<Matcherie>();
            sistem.Clienti ??= new List<ClientAccount>();
            sistem.Administratori ??= new List<AdminAccount>();
            sistem.TipuriRezervari ??= new List<TipRezervare>();
        }
    }
}