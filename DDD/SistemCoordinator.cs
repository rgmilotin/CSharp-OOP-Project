using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ConsoleApp5
{
    public sealed class SistemCoordinator
    {
        private readonly ILogger<SistemCoordinator> _logger;

        public SistemCoordinator(ILogger<SistemCoordinator> logger)
        {
            _logger = logger;
        }

        // 1) ADD PRODUCT (Admin)
        public bool TryAdaugaProdus(Matcherie matcherie, Matcha produs, out string mesaj)
        {
            mesaj = "";

            if (matcherie == null)
            {
                mesaj = "Matcheria este null.";
                return false;
            }

            if (produs == null || string.IsNullOrWhiteSpace(produs.nume))
            {
                mesaj = "Produs invalid (nume lipsă).";
                return false;
            }

            if (matcherie.Meniu == null)
            {
                mesaj = "Meniul matcheriei nu este inițializat.";
                return false;
            }

            bool exista = matcherie.Meniu.Any(p =>
                string.Equals(p.nume, produs.nume, StringComparison.OrdinalIgnoreCase));

            if (exista)
            {
                mesaj = "Există deja un produs cu acest nume în meniu.";
                return false;
            }

            matcherie.Meniu.Add(produs);

            _logger.LogInformation("Admin: produs adăugat | Matcherie={Matcherie} | Produs={Produs} | Pret={Pret}",
                matcherie.Nume, produs.nume, produs.pret);

            mesaj = "Produs adăugat cu succes.";
            return true;
        }

        // 2) CREATE RESERVATION (Client)
        public bool TryCreeazaRezervare(ClientAccount client, Matcherie matcherie, TipRezervare tip,
            out Rezervare? rezervareNoua, out string mesaj)
        {
            rezervareNoua = null;
            mesaj = "";

            if (client == null) { mesaj = "Client null."; return false; }
            if (matcherie == null) { mesaj = "Matcherie null."; return false; }
            if (tip == null) { mesaj = "Tip rezervare null."; return false; }
            

            int cap = matcherie.Capacitate <= 0 ? 0 : matcherie.Capacitate;
            int ocupate = matcherie.Rezervari.Count;

            if (cap == 0)
            {
                mesaj = "Capacitatea matcheriei este invalidă (0).";
                return false;
            }

            if (ocupate >= cap)
            {
                mesaj = "Ne pare rău, matcheria este plină.";
                return false;
            }

            // evitare duplicate “same type in same matchery”
            bool existaDeja = client.Rezervari.Any(r =>
                (r.Matcherie?.Nume ?? "") == matcherie.Nume &&
                string.Equals(r.Tip ?? "", tip.Nume ?? "", StringComparison.OrdinalIgnoreCase));

            if (existaDeja)
            {
                mesaj = "Ai deja o rezervare de acest tip la această matcherie.";
                return false;
            }

            rezervareNoua = new Rezervare(
                tip.Nume,
                tip.Pret,
                tip.Limitari,
                tip.Beneficii,
                client.Nume,
                matcherie
            );

            matcherie.Rezervari.Add(rezervareNoua);
            client.Rezervari.Add(rezervareNoua);

            _logger.LogInformation("Client: rezervare creată | Client={Client} | Matcherie={Matcherie} | Tip={Tip} | Pret={Pret}",
                client.Email, matcherie.Nume, tip.Nume, tip.Pret);

            mesaj = "Rezervare creată cu succes.";
            return true;
        }

        // 3) CANCEL RESERVATION (Client)
        public bool TryAnuleazaRezervare(ClientAccount client, Rezervare rezervare, out string mesaj)
        {
            mesaj = "";

            if (client == null) { mesaj = "Client null."; return false; }
            if (rezervare == null) { mesaj = "Rezervare null."; return false; }

            

            var matcherie = rezervare.Matcherie;
            if (matcherie == null)
            {
                mesaj = "Rezervarea nu are matcherie asociată.";
                return false;
            }

            

            bool removedFromMatcherie = matcherie.Rezervari.Remove(rezervare);
            bool removedFromClient = client.Rezervari.Remove(rezervare);

            if (!removedFromMatcherie && !removedFromClient)
            {
                mesaj = "Rezervarea nu a fost găsită pentru anulare.";
                return false;
            }

            _logger.LogInformation("Client: rezervare anulată | Client={Client} | Matcherie={Matcherie} | Tip={Tip} | Pret={Pret}",
                client.Email, matcherie.Nume, rezervare.Tip, rezervare.Pret);

            mesaj = "Rezervare anulată cu succes.";
            return true;
        }
    }
}
