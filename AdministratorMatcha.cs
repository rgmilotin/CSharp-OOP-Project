namespace ConsoleApp5;

public class AdministratorMatcha
{
    private string nume { get; set; }
    private string adminId { get; set; }
    private string parola { get; set; }
    public List<Matcherie>  Matcherii { get; set; }

    public AdministratorMatcha(string nume, string adminId, string parola, List<Matcherie> matcherii)
    {
        this.nume = nume;
        this.adminId = adminId;
        this.parola = parola;
        Matcherii = matcherii;
    }

    public void creazaMatcherie()
    {
        
    }

    public void modificaMatcherie()
    {
        
    }

    public void stergeMatcherie()
    {
        
    }

    public Tranzactie vindeMatcha()//probabil pus ca argument restuarantul la care se face
    {
        
    }

    public Rezervare creazaRezervare() //probabil pus ca argument restaurantul la care se face
    {
        
    }

    public string informatii()// returneaza/afiseaza informatii despre fiecare restaurant al adminului
    {
        
    }
}