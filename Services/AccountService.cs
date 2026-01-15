using Spectre.Console;

namespace ConsoleApp5
{
    public static class AccountService
    {
        public static void CreeazaAdminNou(SistemMatcha sistem)
        {
            sistem.Administratori ??= new List<AdminAccount>();

            Console.Clear();
            AnsiConsole.Write(new Rule("[green]Creare Administrator[/]").RuleStyle("grey"));

            string nume = AnsiConsole.Ask<string>("Nume admin:");
            string idNou = AnsiConsole.Ask<string>("Admin ID nou:");

            foreach (var a in sistem.Administratori)
                if (a.AdminId == idNou)
                {
                    AnsiConsole.MarkupLine("[red]Există deja un administrator cu acest Admin ID.[/]");
                    CommonUI.Pauza();
                    return;
                }

            string parolaNoua = AnsiConsole.Prompt(new TextPrompt<string>("Parolă nouă:").Secret());

            sistem.Administratori.Add(new AdminAccount(nume, idNou, parolaNoua));

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Administrator creat și salvat![/]");
            CommonUI.Pauza();
        }

        public static void CreeazaContClient(SistemMatcha sistem)
        {
            sistem.Clienti ??= new List<ClientAccount>();

            Console.Clear();
            AnsiConsole.Write(new Rule("[green]Creare cont client[/]").RuleStyle("grey"));

            string nume = AnsiConsole.Ask<string>("Nume client:");
            string email = AnsiConsole.Ask<string>("Email:");

            foreach (var c in sistem.Clienti)
                if (string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[red]Există deja un client cu acest email.[/]");
                    CommonUI.Pauza();
                    return;
                }

            sistem.Clienti.Add(new ClientAccount(nume, email, new List<Tranzactie>(), new List<Rezervare>()));

            CommonUI.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Cont client creat și salvat![/]");
            CommonUI.Pauza();
        }

        public static ClientAccount? AlegeClientDinSistem(SistemMatcha sistem)
        {
            if (sistem.Clienti == null || sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există clienți. Creează un cont (tasta 3).[/]");
                CommonUI.Pauza();
                return null;
            }

            var map = new Dictionary<string, ClientAccount>();
            foreach (var c in sistem.Clienti)
            {
                string key = $"{c.Nume} ({c.Email})";
                int k = 2;
                string temp = key;
                while (map.ContainsKey(temp))
                {
                    temp = $"{key} #{k}";
                    k++;
                }
                map[temp] = c;
            }

            Console.Clear();
            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Selectează clientul[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys)
            );

            return map[ales];
        }

        public static ClientAccount FixClientIfNeeded(SistemMatcha sistem, ClientAccount client)
        {
            bool needsFix = (client.Istoric == null) || (client.Rezervari == null);
            if (!needsFix) return client;

            var ist = client.Istoric ?? new List<Tranzactie>();
            var rez = client.Rezervari ?? new List<Rezervare>();

            var fixedClient = new ClientAccount(client.Nume, client.Email, ist, rez);

            int idx = sistem.Clienti.IndexOf(client);
            if (idx >= 0) sistem.Clienti[idx] = fixedClient;

            return fixedClient;
        }
    }
}
