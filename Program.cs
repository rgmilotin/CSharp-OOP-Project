using Spectre.Console;

var option = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[underline blue]Ce dorești să faci astăzi?[/]")
        .PageSize(10)
        .AddChoices(new[] {
            "Adaugă un student", "Vizualizează catalogul", 
            "Calculare medii", "[red]Ieșire[/]"
        }));

AnsiConsole.MarkupLine($"Ai selectat: [bold yellow]{option}[/]");

if (option == "Vizualizează catalogul")
{
    // Aici poți randa un tabel
    var table = new Table().Border(TableBorder.Rounded);
    table.AddColumn("[yellow]Nume[/]");
    table.AddColumn("[green]Nota[/]");
    table.AddRow("Iozef", "10");
    AnsiConsole.Write(table);
}