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
    public string Nume { get; private set; }
    public string Program { get; private set;  } 
    public int Capacitate { get; private set; }
    private List<Matcha> Meniu {get; set;}

    public Matcherie(string nume, string program, int Capa, List<Matcha> meniu)
    {
        Nume = nume;
        Program = program;
        this.Capacitate= Capa;
        this.Meniu = meniu;
    }
    public void SetProgram(string noulProgram)
    {
        if (!string.IsNullOrEmpty(noulProgram))
        {
            this.Program = noulProgram;
        }
    }

    public void SetCapacitate(int nouaCapacitate)
    {
        if (nouaCapacitate > 0) 
        {
            this.Capacitate = nouaCapacitate;
        }
    }
}///dsadasdsadsa