using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace ClimateGSR.Controllers
{
    public class CountryStat
    {
        public string IsoCode { get; set; }
        public string Country { get; set; }
        public string CO2 { get; set; } = "N/A";
        public string Population { get; set; } = "N/A";
        public string Temperature { get; set; } = "N/A";
        public string CO2PerCapita { get; set; } = "N/A";
        public double? RawCO2 { get; set; }
        public double? RawPopulation { get; set; }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetClimateData()
        {
            var directory = Server.MapPath("~/App_Data");
            var stats = new Dictionary<string, CountryStat>(StringComparer.OrdinalIgnoreCase);
            var invariant = CultureInfo.InvariantCulture;

            // GeoNames is tab-separated. Its ISO3 code joins the three datasets reliably.
            foreach (var line in System.IO.File.ReadLines(Path.Combine(directory, "countries.txt")))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split('\t');
                if (parts.Length < 9 || parts[1].Length != 3) continue;
                stats[parts[1]] = new CountryStat { IsoCode = parts[1], Country = parts[4] };
            }

            var csvSplit = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            foreach (var line in System.IO.File.ReadLines(Path.Combine(directory, "CO2.csv")).Skip(1))
            {
                var parts = csvSplit.Split(line).Select(p => p.Trim().Trim('"')).ToArray();
                CountryStat stat;
                if (parts.Length < 3 || !stats.TryGetValue(parts[0], out stat)) continue;
                for (var column = parts.Length - 1; column >= 2; column--)
                {
                    double tonnes;
                    if (!double.TryParse(parts[column], NumberStyles.Float, invariant, out tonnes)) continue;
                    stat.RawCO2 = tonnes;
                    stat.CO2 = tonnes >= 1e9 ? (tonnes / 1e9).ToString("N2") + " Billion tonnes (Gt/yr)"
                        : tonnes >= 1e6 ? (tonnes / 1e6).ToString("N2") + " Million tonnes (Mt/yr)"
                        : tonnes.ToString("N0") + " tonnes/yr";
                    break;
                }
            }

            ReadLatest("population.csv", (stat, value) =>
            {
                stat.RawPopulation = value;
                stat.Population = value >= 1e9 ? (value / 1e9).ToString("N2") + " Billion people"
                    : value >= 1e6 ? (value / 1e6).ToString("N2") + " Million people"
                    : value.ToString("N0") + " people";
            });
            ReadLatest("temperature.csv", (stat, value) => stat.Temperature = value.ToString("N2") + " °C");

            foreach (var stat in stats.Values)
            {
                if (stat.RawCO2.HasValue && stat.RawPopulation > 0)
                    stat.CO2PerCapita = (stat.RawCO2.Value / stat.RawPopulation.Value).ToString("N2") + " tonnes / person";
            }

            return Json(stats.Values.OrderBy(s => s.Country).ToList(), JsonRequestBehavior.AllowGet);

            void ReadLatest(string fileName, Action<CountryStat, double> update)
            {
                var latestYears = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in System.IO.File.ReadLines(Path.Combine(directory, fileName)).Skip(1))
                {
                    var parts = csvSplit.Split(line).Select(p => p.Trim().Trim('"')).ToArray();
                    CountryStat stat;
                    int year;
                    double value;
                    if (parts.Length < 4 || !stats.TryGetValue(parts[1], out stat)
                        || !int.TryParse(parts[2], out year)
                        || !double.TryParse(parts[3], NumberStyles.Float, invariant, out value)) continue;
                    int previousYear;
                    if (latestYears.TryGetValue(parts[1], out previousYear) && year < previousYear) continue;
                    latestYears[parts[1]] = year;
                    update(stat, value);
                }
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}
