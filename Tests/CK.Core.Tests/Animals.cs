namespace CK.Core.Tests;

public class Animal
{
    public Animal( string name )
    {
        Name = name;
    }

    public string Name { get; set; }

    public override string ToString()
    {
        return $"Animals: {Name} ({GetHashCode()})";
    }
}

public class Mammal : Animal
{
    public Mammal( string name, int gestationPeriod = 12 )
        : base( name )
    {
        Name = name;
        GestationPeriod = gestationPeriod;
    }

    public int GestationPeriod { get; set; }

    public override string ToString()
    {
        return $"Mammals: {Name}, {GestationPeriod} ({GetHashCode()})";
    }

}

public class Canidae : Mammal
{
    public Canidae( string name, int gestationPeriod = 9, bool isRetriever = false )
        : base( name, gestationPeriod )
    {
        Name = name;
        GestationPeriod = gestationPeriod;
        IsRetriever = isRetriever;
    }

    public bool IsRetriever { get; set; }

    public override string ToString()
    {
        return $"Canidae: {Name}, {GestationPeriod}, {IsRetriever} ({GetHashCode()})";
    }
}
