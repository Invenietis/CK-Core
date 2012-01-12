using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class Animal
    {
        public Animal( string name )
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return String.Format( "Animals: {0} ({1})", Name, GetHashCode() );
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
            return String.Format( "Mammals: {0}, {1} ({2})", Name, GestationPeriod, GetHashCode() );
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
            return String.Format( "Canidae: {0}, {1}, {2} ({3})", Name, GestationPeriod, IsRetriever, GetHashCode() );
        }
    }
}
