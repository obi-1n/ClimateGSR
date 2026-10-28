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

    //Builds quiz questions from the Countries table. Each question has an
    //emissions prompt (shown at the start), the answer (the country's name),
    //and a shuffled list of hints about the country.
    public class QuestionGenerator
    {
        // Countries below this population are never chosen - makes questions guessable
        private const int MinPopulation = 1000000;

        private static readonly Random Rng = new Random();
        private static readonly object RngLock = new object(); //thread safety

        //Randomly reorders a list in place
        private static void Shuffle<T>(IList<T> list)
        {
            lock (RngLock)   //Only one thread at a time may use Rng
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = Rng.Next(i + 1); //Random position

                    //Swap items 
                    T temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }
        }

        //database connection
        private readonly ClimateGSRDbContext _context;

        public QuestionGenerator(ClimateGSRDbContext context)
        {
            _context = context;
        }

        //Creates one complete question: a random country, its answer,
        //the emissions prompt, and a shuffled set of hints.
        public Question GenerateQuestion()
        {
            var country = GetRandomCountry();

            return new Question
            {
                Country = country,
                Answer = country.Name,                                
                EmissionsPrompt = FormatEmissions(country.Emissions), //Shown at the start
                Hints = GenerateHints(country)
            };
        }

        //Picks a random country from the database
        public Country GetRandomCountry()
        {
            //Building the query
            var query = _context.Countries.Include(c => c.Continent).Where(c => c.Population >= MinPopulation);

            //Count how many countries qualify
            int count = query.Count();
            if (count == 0)
                throw new InvalidOperationException("No countries available to generate a question.");

            //Choose a random position
            int skip;
            lock (RngLock) { skip = Rng.Next(count); }

            //Fetch only the country at that position
            return query.OrderBy(c => c.Id).Skip(skip).First();
        }

        //Builds the list of hints for a country, then shuffles 
        public List<Hint> GenerateHints(Country c)
        {
            //Invariant culture keeps number formatting the same on any device
            var inv = CultureInfo.InvariantCulture;
            var hints = new List<Hint>();

            //Temperature is nullable as some small countries have no data
            if (c.AverageTemperature.HasValue)
                hints.Add(new Hint
                {
                    Type = "Temperature",
                    Value = "Its average annual temperature is about " +
                            Math.Round(c.AverageTemperature.Value).ToString("0", inv) + "°C."
                });

            //Tonnes of CO2 per person
            hints.Add(new Hint
            {
                Type = "EmissionsPerCapita",
                Value = "Each person emits roughly " +
                        c.EmissionsPerCapita.ToString("0.0", inv) + " tonnes of CO2 per year."
            });

            //Population shown in millions
            hints.Add(new Hint
            {
                Type = "Population",
                Value = "Its population is about " +
                        (c.Population / 1000000.0).ToString("0.#", inv) + " million people."
            });

            //"N0" adds thousands separators with no decimals (e.g 2,766,890)
            hints.Add(new Hint
            {
                Type = "Area",
                Value = "Its area is about " + c.Area.ToString("N0", inv) + " km²."
            });

            //Continent was loaded by the getrandomcountry query
            hints.Add(new Hint
            {
                Type = "Continent",
                Value = "It is located in " + c.Continent.Name + "."
            });

            //c.Name[0] is the first character of the country's name
            hints.Add(new Hint
            {
                Type = "FirstLetter",
                Value = "Its name starts with the letter " + c.Name[0] + "."
            });

            Shuffle(hints);   //Random hint order each time
            return hints;
        }

        //Turns raw emissions figure into a readable sentence,
        private static string FormatEmissions(double tonnes)
        {
            var inv = CultureInfo.InvariantCulture;
            string amount;

            //1e9 is scientific notation for 1,000,000,000
            if (tonnes >= 1e9) amount = (tonnes / 1e9).ToString("0.##", inv) + " billion";
            else if (tonnes >= 1e6) amount = (tonnes / 1e6).ToString("0.#", inv) + " million";
            else amount = (tonnes / 1e3).ToString("0", inv) + " thousand";

            return "This country emitted about " + amount + " tonnes of CO2 in 2024.";
        }

    }
}