namespace ConsoleApp5
{
    /// Model tranzacție: se adaugă în istoricul clientului.
    public class Tranzactie
    {
        public string Id { get; private set; }
        public DateTime Data { get; private set; }
        public decimal suma { get; private set; }
        public Matcherie Matcherie { get; private set; }

        public Tranzactie(string id, DateTime data, decimal suma, Matcherie matcherie)
        {
            Id = id;
            Data = data;
            this.suma = suma;
            Matcherie = matcherie;
        }
    }
}