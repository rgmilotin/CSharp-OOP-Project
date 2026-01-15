using Spectre.Console;

namespace ConsoleApp5
{
    /// UI helpers reutilizabile în mai multe meniuri.
    public static class CommonUI
    {
        /// Pauză standard (așteaptă o tastă).
        public static void Pauza()
        {
            AnsiConsole.MarkupLine("\n[grey]Apasă orice tastă pentru a continua...[/]");
            Console.ReadKey(true);
        }
        
        /// Salvează sistemul în JSON cu feedback vizual.
        public static void SalvareSistem(SistemMatcha sistem)
        {
            AnsiConsole.Status().Start("Se salvează datele...", ctx =>
            {
                GestiuneDate.SalveazaTot(sistem);
                Thread.Sleep(800);
            });
        }
    }
}