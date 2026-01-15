using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class AdminAccount
    {
        public string Nume { get; private set; }
        public string AdminId { get; private set; }
        public string Parola { get; private set; }

        [JsonConstructor]
        public AdminAccount(string nume, string adminId, string parola)
        {
            Nume = nume;
            AdminId = adminId;
            Parola = parola;
        }
    }
}