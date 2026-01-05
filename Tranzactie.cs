namespace ConsoleApp5;

public class Tranzactie
{
    private string Id { get; set; }
    private DateTime Data { get; set; }
    private decimal suma  { get; set; }

    public Tranzactie(string id, DateTime data, decimal suma)
    {
        Id = id;
        Data = data;
        this.suma = suma;
    }
}

public class Rezervare
{
    public string Tip { get; set; }//masa 2 persoane ex
    private string ClidentID { get; set; }
    public decimal Pret { get; set; }
    public string limitari { get; set; }
    public string beneficii { get; set; }

    public Rezervare(string tip, decimal pret, string limitari, string beneficii)
    {
        Tip = tip;
        Pret = pret;
        this.limitari = limitari;
        this.beneficii = beneficii;
        this.ClidentID = this.beneficii;
    }
}