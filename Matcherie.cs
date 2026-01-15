using Spectre.Console;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    /// Model magazin (matcherie): program, capacitate, meniu, rezervări.
    public class Matcherie
    {
        public string Nume { get; private set; }
        public string Program { get; private set; }
        public int Capacitate { get; private set; }
        public List<Matcha> Meniu { get; private set; }
        public List<Rezervare> Rezervari { get; private set; }

        [JsonConstructor]
        public Matcherie(string nume, string program, int capacitate, List<Matcha> meniu, List<Rezervare> rezervari)
        {
            Nume = nume;
            Program = program;
            Capacitate = capacitate;
            Meniu = meniu ?? new List<Matcha>();
            Rezervari = rezervari ?? new List<Rezervare>();
        }
        
        /// Afișează meniul în tabel Spectre.Console.
        public void AfiseazaMeniu()
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title($"[bold white on green] MENIU {Nume.ToUpper()} [/]");

            table.AddColumn("[bold]Produs[/]");
            table.AddColumn("[bold]Descriere[/]");
            table.AddColumn(new TableColumn("[bold]Preț[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Calorii[/]").Centered());

            foreach (var item in Meniu)
            {
                table.AddRow(
                    $"[green]{item.nume}[/]",
                    $"[grey]{item.descriere}[/]",
                    $"[yellow]{item.pret} RON[/]",
                    $"{item.calorii} kcal"
                );
            }

            AnsiConsole.Write(table);
        }
        
        /// Set program (validare minimă).
        public void SetProgram(string noulProgram)
        {
            if (!string.IsNullOrEmpty(noulProgram))
                Program = noulProgram;
        }
        
        /// Set capacitate (pozitivă).
        public void SetCapacitate(int nouaCapacitate)
        {
            if (nouaCapacitate > 0)
                Capacitate = nouaCapacitate;
        }
        
        /// Șterge rezervare din listă (safe).
        public bool StergeRezervare(Rezervare rezervare)
        {
            if (rezervare == null) return false;
            if (!Rezervari.Contains(rezervare)) return false;

            Rezervari.Remove(rezervare);
            return true;
        }
    }
}
