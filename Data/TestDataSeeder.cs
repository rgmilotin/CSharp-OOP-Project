using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp5
{

    /// Seed de date demo (folosit când sistemul e gol).
    public static class TestDataSeeder
    {
        public static void IncarcaDateTest(SistemMatcha sistem)
        {
            sistem.Magazine ??= new List<Matcherie>();
            sistem.Clienti ??= new List<ClientAccount>();
            sistem.Administratori ??= new List<AdminAccount>();
            sistem.TipuriRezervari ??= new List<TipRezervare>();

            // -------------------- TIPURI REZERVARI --------------------
            AddTipRezervareIfMissing(sistem, new TipRezervare(
                "Familie",
                35m,
                "Minim 3 persoane, maxim 6. Durată standard 2h. Recomandare: rezervare cu min. 12h înainte.",
                "Masă mai mare + zonă liniștită; 10% reducere la un desert matcha pentru grup."
            ));

            AddTipRezervareIfMissing(sistem, new TipRezervare(
                "Prieteni",
                25m,
                "2–8 persoane. Durată standard 2h. Fără schimbare masă după confirmare.",
                "1 apă (still) gratuită pentru grup + acces la boardgames (unde există)."
            ));

            AddTipRezervareIfMissing(sistem, new TipRezervare(
                "Onomastică",
                55m,
                "Minim 6 persoane. Necesită confirmare cu min. 24h înainte. Durată 2h 30m.",
                "Mic decor + mesaj pe card + mini desert matcha pentru sărbătorit (în limita stocului)."
            ));

            // -------------------- 3 MATCHERII PREDEFINITE --------------------
            var m1 = GetOrCreateMatcherie(
                sistem,
                "Matcha Zen",
                "07:30-22:30",
                capacitate: 26
            );

            var m2 = GetOrCreateMatcherie(
                sistem,
                "Matcha Odaia",
                "09:00-01:00",
                capacitate: 42
            );

            var m3 = GetOrCreateMatcherie(
                sistem,
                "Matcha Harbor",
                "10:00-20:00",
                capacitate: 18
            );

            // -------------------- MENIURI (varietate) --------------------
            // Produse base
            var pLatte = new Matcha("Matcha Latte", "Clasic, lapte + matcha", 22.5m, 120, 180);
            var pCoffee = new Matcha("Matcha Coffee", "Espresso + matcha, vibe energic", 24.9m, 90, 160);
            var pVodka = new Matcha("Matcha Vodka", "Matcha + lime + vodka (18+)", 34.0m, 40, 220);

            var pDessertTiramisu = new Matcha("Matcha Tiramisu", "Desert cremos cu note intense de matcha", 19.9m, 35, 330);
            var pDessertCheesecake = new Matcha("Matcha Cheesecake", "Cheesecake cu crustă crocantă și matcha", 21.5m, 28, 410);
            var pDessertMochi = new Matcha("Matcha Mochi", "2 bucăți mochi, soft & chewy", 16.0m, 60, 210);
            var pDessertBrownie = new Matcha("Matcha Brownie", "Brownie dens cu matcha", 18.5m, 45, 360);

            var pStillWater = new Matcha("Matcha Still Water", "Apă plată rece (0 kcal)", 9.5m, 200, 0);
            var pSparkWater = new Matcha("Matcha Sparkling Water", "Apă minerală carbogazoasă (0 kcal)", 10.0m, 180, 0);

            // Matcha Zen: “clasic + cozy”
            AddProductIfMissing(m1, Clone(pLatte));
            AddProductIfMissing(m1, Clone(pCoffee));
            AddProductIfMissing(m1, Clone(pDessertMochi));
            AddProductIfMissing(m1, Clone(pDessertTiramisu));
            AddProductIfMissing(m1, Clone(pStillWater));

            // Matcha Odaia: “night vibe + mai multe opțiuni”
            AddProductIfMissing(m2, Clone(pLatte));
            AddProductIfMissing(m2, Clone(pCoffee));
            AddProductIfMissing(m2, Clone(pVodka));
            AddProductIfMissing(m2, Clone(pDessertCheesecake));
            AddProductIfMissing(m2, Clone(pDessertBrownie));
            AddProductIfMissing(m2, Clone(pSparkWater));

            // Matcha Harbor: “light + refreshing”
            AddProductIfMissing(m3, new Matcha("Matcha Ice Cream", "Înghețată matcha, cremoasă", 17.5m, 55, 250));
            AddProductIfMissing(m3, Clone(pLatte));
            AddProductIfMissing(m3, new Matcha("Matcha Lemonade", "Limonadă cu matcha și mentă", 18.0m, 80, 120));
            AddProductIfMissing(m3, Clone(pDessertMochi));
            AddProductIfMissing(m3, Clone(pStillWater));
            AddProductIfMissing(m3, Clone(pSparkWater));

            // -------------------- CLIENTI PREDEFINITI --------------------
            var c1 = GetOrCreateClient(sistem, "Andrei Popa", "andrei@email.com");
            var c2 = GetOrCreateClient(sistem, "Mara Dima", "mara@email.com");
            var c3 = GetOrCreateClient(sistem, "Radu Popescu", "radu@email.com");
            var c4 = GetOrCreateClient(sistem, "Ioana Stoica", "ioana@email.com");
            var c5 = GetOrCreateClient(sistem, "Vlad Ionescu", "vlad@email.com");

            // -------------------- ADMINI PREDEFINITI --------------------
            AddAdminIfMissing(sistem, new AdminAccount("Admin", "ADM01", "1234"));
            AddAdminIfMissing(sistem, new AdminAccount("Admin2", "ADM02", "1234"));

            // -------------------- REZERVARI PREDEFINITE (ca să varieze “locuri libere”) --------------------
            SeedRezervariIfFew(m1, new[]
            {
                new Rezervare("Familie", 35m, "Min 3 pers, max 6", "Masă mare + 10% desert", c1.Nume, m1),
                new Rezervare("Prieteni", 25m, "2–8 pers", "Apă gratis + boardgames", c2.Nume, m1),
                new Rezervare("Onomastică", 55m, "Min 6 pers, 24h înainte", "Decor + mini desert", c3.Nume, m1),
            }, minCount: 2);

            SeedRezervariIfFew(m2, new[]
            {
                new Rezervare("Prieteni", 25m, "2–8 pers", "Apă gratis + boardgames", c4.Nume, m2),
                new Rezervare("Onomastică", 55m, "Min 6 pers, 24h înainte", "Decor + mini desert", c2.Nume, m2),
                new Rezervare("Prieteni", 25m, "2–8 pers", "Apă gratis + boardgames", c5.Nume, m2),
                new Rezervare("Familie", 35m, "Min 3 pers, max 6", "Masă mare + 10% desert", c1.Nume, m2),
            }, minCount: 3);

            SeedRezervariIfFew(m3, new[]
            {
                new Rezervare("Familie", 35m, "Min 3 pers, max 6", "Masă mare + 10% desert", c3.Nume, m3),
            }, minCount: 1);

            // -------------------- TRANZACTII PREDEFINITE (ca să arate chart-ul din Admin dashboard) --------------------
            SeedTranzactiiIfEmpty(c1, m1, new[] { 22.5m, 19.9m, 9.5m });
            SeedTranzactiiIfEmpty(c2, m2, new[] { 24.9m, 21.5m });
            SeedTranzactiiIfEmpty(c3, m2, new[] { 34.0m, 18.5m });
            SeedTranzactiiIfEmpty(c4, m3, new[] { 18.0m, 10.0m });
            SeedTranzactiiIfEmpty(c5, m1, new[] { 22.5m });

            // Notă: sistemul se salvează când ieși din aplicație (în App.Run -> Ieșire).
        }

        // -------------------- HELPERS --------------------

        private static void AddTipRezervareIfMissing(SistemMatcha sistem, TipRezervare tip)
        {
            if (sistem.TipuriRezervari.Any(x => x.Nume.Equals(tip.Nume, StringComparison.OrdinalIgnoreCase)))
                return;

            sistem.TipuriRezervari.Add(tip);
        }

        private static Matcherie GetOrCreateMatcherie(SistemMatcha sistem, string nume, string program, int capacitate)
        {
            var existing = sistem.Magazine.FirstOrDefault(m => m.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));
            if (existing != null) return existing;

            var created = new Matcherie(nume, program, capacitate, new List<Matcha>(), new List<Rezervare>());
            sistem.Magazine.Add(created);
            return created;
        }

        private static void AddProductIfMissing(Matcherie matcherie, Matcha produs)
        {
            
            if (matcherie.Meniu == null) return; 

            bool exists = matcherie.Meniu.Any(p => p.nume.Equals(produs.nume, StringComparison.OrdinalIgnoreCase));
            if (!exists) matcherie.Meniu.Add(produs);
        }

        private static ClientAccount GetOrCreateClient(SistemMatcha sistem, string nume, string email)
        {
            var existing = sistem.Clienti.FirstOrDefault(c =>
                string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));

            if (existing != null) return existing;

            var created = new ClientAccount(nume, email, new List<Tranzactie>(), new List<Rezervare>());
            sistem.Clienti.Add(created);
            return created;
        }

        private static void AddAdminIfMissing(SistemMatcha sistem, AdminAccount admin)
        {
            bool exists = sistem.Administratori.Any(a => a.AdminId.Equals(admin.AdminId, StringComparison.OrdinalIgnoreCase));
            if (!exists) sistem.Administratori.Add(admin);
        }

        private static void SeedRezervariIfFew(Matcherie m, IEnumerable<Rezervare> rezervari, int minCount)
        {
            

            if (m.Rezervari.Count >= minCount) return;

            foreach (var r in rezervari)
            {
                // evităm duplicate grosiere (Tip + Client + Matcherie)
                bool exists = m.Rezervari.Any(x =>
                    (x.Tip ?? "").Equals(r.Tip ?? "", StringComparison.OrdinalIgnoreCase) &&
                    (x.ClientID ?? "").Equals(r.ClientID ?? "", StringComparison.OrdinalIgnoreCase));

                if (!exists) m.Rezervari.Add(r);

                if (m.Rezervari.Count >= minCount) break;
            }
        }

        private static void SeedTranzactiiIfEmpty(ClientAccount c, Matcherie m, decimal[] sume)
        {
            

            // dacă ai deja tranzacții, nu mai băgăm (ca să nu se dubleze)
            if (c.Istoric.Count > 0) return;

            // împrăștiem pe ultimele 6 zile ca să arate frumos graficul
            // (azi, ieri, -2, etc)
            for (int i = 0; i < sume.Length; i++)
            {
                var dt = DateTime.Now.AddDays(-i).AddMinutes(-10 * i);
                c.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), dt, sume[i], m));
            }
        }

        private static Matcha Clone(Matcha p)
            => new Matcha(p.nume, p.descriere, p.pret, p.cantitate, p.calorii);
    }
}
