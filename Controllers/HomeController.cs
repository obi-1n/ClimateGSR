using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace ClimateGSR.Controllers
{
    public class CountryStat
    {
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
            string appDataPath = Server.MapPath("~/App_Data");
            var stats = new Dictionary<string, CountryStat>(StringComparer.OrdinalIgnoreCase);

            //co2.csv is formatted different so we need a regex to split on commas that are not inside quotes
            Regex csvSplit = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            //helper to determine whether Country Name is in Column 0 or Column 1 since co2.csv is different format to other csv
            string ExtractCountryName(string[] parts, out int dataStartIndex)
            {
                dataStartIndex = 1;
                string col0 = parts[0].Trim().Trim('"');
                string col1 = parts.Length > 1 ? parts[1].Trim().Trim('"') : "";

                //ff Column 0 is a 3-letter ISO code (e.g. WLD, GBR, AFG) and Col 1 is text (not a number)
                if (col0.Length == 3 && col0.All(char.IsLetter) &&
                    !string.IsNullOrEmpty(col1) && !double.TryParse(col1, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    dataStartIndex = 2;
                    return col1;
                }

                //otherwise, Col 0 is the Country Name
                //if Col 1 is a 3-letter ISO code, data starts at Col 2
                if (col1.Length == 3 && col1.All(char.IsLetter))
                {
                    dataStartIndex = 2;
                }

                return col0;
            }

            //loads country data from countries.txt, ensuring all countries are represented
            string countriesFile = Path.Combine(appDataPath, "countries.txt");
            if (System.IO.File.Exists(countriesFile))
            {
                var lines = System.IO.File.ReadAllLines(countriesFile);
                foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    string cName = line.Split(',')[0].Trim().Trim('"');
                    if (!stats.ContainsKey(cName))
                    {
                        stats[cName] = new CountryStat { Country = cName };
                    }
                }
            }

            //helps to parse CSV files and update the CountryStat object for each country, based on updated values in csv files
            void ParseCsv(string fileName, Action<CountryStat, double> updateStat)
            {
                string filePath = Path.Combine(appDataPath, fileName);
                if (!System.IO.File.Exists(filePath)) return;

                var lines = System.IO.File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = csvSplit.Split(line);
                    if (parts.Length < 2) continue;

                    string col0 = parts[0].Trim().Trim('"');
                    //skip header rows
                    if (col0.StartsWith("(") || col0.StartsWith("DOI") ||
                        col0.Equals("ISO", StringComparison.OrdinalIgnoreCase) ||
                        col0.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                        col0.Equals("Country", StringComparison.OrdinalIgnoreCase) ||
                        col0.Equals("Entity", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string countryName = ExtractCountryName(parts, out int startCol);
                    if (string.IsNullOrEmpty(countryName) ||
                        countryName.Equals("Country", StringComparison.OrdinalIgnoreCase) ||
                        countryName.Equals("Entity", StringComparison.OrdinalIgnoreCase) ||
                        countryName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Scan backwards from the end of the row to get the most recent valid number
                    for (int col = parts.Length - 1; col >= startCol; col--)
                    {
                        string val = parts[col].Trim().Trim('"');
                        if (!string.IsNullOrWhiteSpace(val) &&
                            double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double numericVal))
                        {
                            if (!stats.ContainsKey(countryName))
                            {
                                stats[countryName] = new CountryStat { Country = countryName };
                            }

                            updateStat(stats[countryName], numericVal);
                            break;
                        }
                    }
                }
            }

            //formatting helpers for Climate Awareness
            string FormatCO2(double tonnes)
                    {
                        if (tonnes >= 1_000_000_000)
                            return $"{(tonnes / 1_000_000_000.0):N2} Billion tonnes (Gt/yr)";
                        if (tonnes >= 1_000_000)
                            return $"{(tonnes / 1_000_000.0):N2} Million tonnes (Mt/yr)";
                        if (tonnes >= 1_000)
                            return $"{tonnes:N0} tonnes/yr";
                        return $"{tonnes:N2} Mt/yr"; //in case dataset is already in Megatonnes
                    }

                    string FormatPopulation(double pop)
                    {
                        //some population CSVs store numbers in thousands
                        //standard population counts:
                        if (pop >= 1_000_000_000)
                            return $"{(pop / 1_000_000_000.0):N2} Billion people";
                        if (pop >= 1_000_000)
                            return $"{(pop / 1_000_000.0):N2} Million people";
                        return $"{pop:N0} people";
                    }

                    string FormatTemperature(double temp)
                    {
                        //if temperature is between -5 and +5, possible anomaly; otherwise average temp
                        return $"{temp:N2} °C";
                    }

                    //parse each file
                    ParseCsv("CO2.csv", (stat, val) =>
                    {
                        stat.RawCO2 = val;
                        stat.CO2 = FormatCO2(val);
                    });

                    ParseCsv("population.csv", (stat, val) =>
                    {
                        stat.RawPopulation = val;
                        stat.Population = FormatPopulation(val);
                    });

                    ParseCsv("temperature.csv", (stat, val) =>
                    {
                        stat.Temperature = FormatTemperature(val);
                    });

                    //calculate CO2 Per Capita
                    foreach (var item in stats.Values)
                    {
                        if (item.RawCO2.HasValue && item.RawPopulation.HasValue && item.RawPopulation.Value > 0)
                        {
                            double perCapita = item.RawCO2.Value / item.RawPopulation.Value;
                            item.CO2PerCapita = $"{perCapita:N2} tonnes / person";
                        }
                    }

                    return Json(stats.Values.ToList(), JsonRequestBehavior.AllowGet);
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