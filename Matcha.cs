namespace ConsoleApp5;

public class Matcha
{
    public string nume { get; set; }
    public string descriere { get; set; }
    public decimal pret { get; set; }
    public int cantitate { get; set; }
    public int calorii { get; set; }

    public Matcha(string nume, string descriere, decimal pret, int cantitate, int calorii)
    {
        this.nume = nume;
        this.descriere = descriere;
        this.pret = pret;
        this.cantitate = cantitate;
        this.calorii = calorii;
    }
}
    
public class Matcherie
{
    private string Nume { get; set; }
    private string Program { get; set;  } 
    private int Capacitate { get; set; }
    private List<Matcha> Meniu {get; set;}

    public Matcherie(string nume, string program, int Capa, List<Matcha> meniu)
    {
        Nume = nume;
        Program = program;
        this.Capacitate= Capa;
        this.Meniu = meniu;
    }
}