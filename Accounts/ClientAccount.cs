using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class ClientAccount
    {
        public string Nume { get; private set; }
        public string Email { get; private set; }
        public List<Tranzactie> Istoric { get; private set; }
        public List<Rezervare> Rezervari { get; private set; }

        [JsonConstructor]
        public ClientAccount(string nume, string email, List<Tranzactie> istoric, List<Rezervare> rezervari)
        {
            Nume = nume;
            Email = email;
            Istoric = istoric ?? new List<Tranzactie>();
            Rezervari = rezervari ?? new List<Rezervare>();
        }
    }
}