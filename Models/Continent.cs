using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    public class Continent
    {
        public Continent()
        {
            Countries = new HashSet<Country>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalCountries { get; set; }
        public virtual ICollection<Country> Countries { get; set; }
    }
}