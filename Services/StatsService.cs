namespace ConsoleApp5
{
    /// Statistici / agregări pentru UI.
    public static class StatsService
    {
        public static List<(string nume, int val)> GetTopMatcheriiByRezervari(SistemMatcha sistem, int max)
        {
            var list = new List<(string nume, int val)>();
            if (sistem.Magazine == null) return list;

            foreach (var m in sistem.Magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                list.Add((m.Nume, rez));
            }

            // sort desc
            list.Sort((a, b) => b.val.CompareTo(a.val));

            if (list.Count > max) list = list.GetRange(0, max);
            return list;
        }
    }
}