using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string IsoCode { get; set; }
        public string Name { get; set; }
        public virtual Continent Continent { get; set; }
        public int Population { get; set; }
        public double Area { get; set; }
        public double Emissions { get; set; }
        public double EmissionsPerCapita { get; set; }
        public double? AverageTemperature { get; set; }
    }
}