using ClimateGSR.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Hosting;


//CreateDatabaseIfNotExists
//DropCreateDatabaseAlways
public class DatabaseInitialiser : CreateDatabaseIfNotExists<ClimateGSRDbContext>
{
    protected override void Seed(ClimateGSRDbContext context)
    {
        string dir = HostingEnvironment.MapPath("~/App_Data/");
        var inv = CultureInfo.InvariantCulture;
        var countries = new Dictionary<string, Country>();

        // 1. Emissions: column 36 = 2024 (0 = code, 1 = name, 2 = 1990)
        foreach (var line in File.ReadLines(Path.Combine(dir, "CO2.csv")).Skip(1))
        {
            var p = line.Split(',');
            double value;
            if (p.Length < 37 || p[0] == "WLD") continue;
            if (!double.TryParse(p[36], NumberStyles.Float, inv, out value)) continue;

            countries[p[0]] = new Country
            {
                IsoCode = p[0],
                Name = p[1].Trim(),
                Emissions = value
            };
        }

        // 2. Temperature: assumes columns Entity, Code, Year, Value. Check your header.
        foreach (var line in File.ReadLines(Path.Combine(dir, "temperature.csv")).Skip(1))
        {
            var p = line.Split(',');
            Country c;
            double temp;
            if (p.Length < 4 || p[2] != "2025") continue;
            if (!countries.TryGetValue(p[1], out c)) continue; // also drops OWID aggregates
            if (double.TryParse(p[3], NumberStyles.Float, inv, out temp))
                c.AverageTemperature = temp;
        }

        // 3. GeoNames: continent + area (population only as a fallback)
        var continentNames = new Dictionary<string, string>
    {
        { "AF", "Africa" }, { "AN", "Antarctica" }, { "AS", "Asia" },
        { "EU", "Europe" }, { "NA", "North America" }, { "OC", "Oceania" },
        { "SA", "South America" }
    };

        var continents = new Dictionary<string, Continent>();
        foreach (var line in File.ReadLines(Path.Combine(dir, "countries.txt")))
        {
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;

            var p = line.Split('\t');
            Country c;
            string continentName;
            double area;
            int population;

            if (p.Length < 9) continue;
            if (!countries.TryGetValue(p[1], out c)) continue;          // p[1] = ISO3
            if (!continentNames.TryGetValue(p[8], out continentName)) continue;
            if (!double.TryParse(p[6], NumberStyles.Float, inv, out area)) continue;
            int.TryParse(p[7], NumberStyles.Integer, inv, out population);

            Continent cont;
            if (!continents.TryGetValue(continentName, out cont))
            {
                cont = new Continent { Name = continentName };
                continents[continentName] = cont;
            }

            c.Continent = cont;
            c.Area = area;
            c.Population = population;   // fallback, overwritten below where OWID has data
            cont.TotalCountries++;
        }

        // 4. OWID population: Entity, Code, Year, Population (2023 = latest)
        foreach (var line in File.ReadLines(Path.Combine(dir, "population.csv")).Skip(1))
        {
            var p = line.Split(',');
            Country c;
            int pop;
            if (p.Length < 4 || p[2] != "2023" || p[1].Length != 3) continue;
            if (!countries.TryGetValue(p[1], out c)) continue;
            if (int.TryParse(p[3], NumberStyles.Integer, inv, out pop) && pop > 0)
                c.Population = pop;
        }

        // 5. Per capita, once population is final
        foreach (var c in countries.Values)
            c.EmissionsPerCapita = c.Population > 0 ? c.Emissions / c.Population : 0;

        // Only keep countries we could place on a continent; continents are saved via the graph
        var toSave = countries.Values.Where(c => c.Continent != null).ToList();

        context.Set<Country>().AddRange(toSave);
        context.SaveChanges();

        base.Seed(context);
    }
}