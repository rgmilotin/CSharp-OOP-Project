using System.Transactions;

namespace ConsoleApp5;

public class Client
{
    private string Nume{get;set;}
    private string Email{get;set;}
    private List<Tranzactie> Istoric { get; set; }

    public Client(string nume, string email, List<Tranzactie> istoric)
    {
        Nume = nume;
        Email = email;
        Istoric = istoric;
    }
    public void VeziIstoric ()
    {
        foreach (var var in Istoric)
        {
            Console.WriteLine(var);            
        }
    }

    public string veziRestaurante()
    {
        
    }

    public Tranzactie cumparaMatcha(Matcha m)
    {
        
    }

    public Rezervare rezervaMasa()//poate pus ca paramentru restuarantul pentru care se rezerva masa
    {
        
    }

    public bool anuleazaRezervare() //poate pus la ce restaurant se anuleaza rezervarea sau o identificam dupa un id al
        //rezervarii, returneaza true dace s-a facut, false daca nu a reusit
    {
        
    }
}