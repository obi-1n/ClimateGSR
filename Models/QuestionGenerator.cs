using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;

    public class QuestionGenerator
    {
        //Excludes tiny territories so questions are guessable. Set to 0 to include everything.
        private const int MinPopulation = 1000000;

        private static readonly Random Rng = new Random();
        private static readonly object RngLock = new object(); //Random isn't thread safe
        private static void Shuffle<T>(IList<T> list)
        {
            lock (RngLock)
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = Rng.Next(i + 1);
                    T temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
        }

        private readonly ClimateGSRDbContext _context;

        public QuestionGenerator(ClimateGSRDbContext context)
        {
            _context = context;
        }

        public Question GenerateQuestion()
        {
            var country = GetRandomCountry();

            return new Question
            {
                Country = country,
                Answer = country.Name,
                EmissionsPrompt = FormatEmissions(country.Emissions),
                Hints = GenerateHints(country)
            };
        }

        public Country GetRandomCountry()
        {
            var query = _context.Countries
                .Include(c => c.Continent)
                .Where(c => c.Population >= MinPopulation);

            int count = query.Count();
            if (count == 0)
                throw new InvalidOperationException("No countries available to generate a question.");

            int skip;
            lock (RngLock) { skip = Rng.Next(count); }

            return query.OrderBy(c => c.Id).Skip(skip).First();   // EF6 needs OrderBy before Skip
        }

        public List<Hint> GenerateHints(Country c)
        {
            var inv = CultureInfo.InvariantCulture;
            var hints = new List<Hint>();

            if (c.AverageTemperature.HasValue)
                hints.Add(new Hint
                {
                    Type = "Temperature",
                    Value = "Its average annual temperature is about " +
                            Math.Round(c.AverageTemperature.Value).ToString("0", inv) + "°C."
                });

            hints.Add(new Hint
            {
                Type = "EmissionsPerCapita",
                Value = "Each person emits roughly " +
                        c.EmissionsPerCapita.ToString("0.0", inv) + " tonnes of CO2 per year."
            });

            hints.Add(new Hint
            {
                Type = "Population",
                Value = "Its population is about " +
                        (c.Population / 1000000.0).ToString("0.#", inv) + " million people."
            });

            hints.Add(new Hint
            {
                Type = "Area",
                Value = "Its area is about " + c.Area.ToString("N0", inv) + " km²."
            });

            hints.Add(new Hint
            {
                Type = "Continent",
                Value = "It is located in " + c.Continent.Name + "."
            });

            hints.Add(new Hint
            {
                Type = "FirstLetter",
                Value = "Its name starts with the letter " + c.Name[0] + "."
            });

            Shuffle(hints);
            return hints;
        }

        private static string FormatEmissions(double tonnes)
        {
            var inv = CultureInfo.InvariantCulture;
            string amount;

            if (tonnes >= 1e9) amount = (tonnes / 1e9).ToString("0.##", inv) + " billion";
            else if (tonnes >= 1e6) amount = (tonnes / 1e6).ToString("0.#", inv) + " million";
            else amount = (tonnes / 1e3).ToString("0", inv) + " thousand";

            return "This country emitted about " + amount + " tonnes of CO2 in 2024.";
        }


    }
}