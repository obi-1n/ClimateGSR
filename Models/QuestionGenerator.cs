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

    /// <summary>
    /// Service responsible for selecting random countries from the database and generating
    /// guessing-game questions with formatted hints and climate prompts.
    /// </summary>
    public class QuestionGenerator
    {
        /// <summary>
        /// Minimum population threshold used when querying countries to ensure questions are reasonably recognizable and guessable.
        /// Excludes tiny territories. Set to 0 to include all countries.
        /// </summary>
        private const int MinPopulation = 1000000;

        /// <summary>
        /// Pseudo-random number generator instance.
        /// </summary>
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Synchronization lock object to ensure thread-safe operations on the shared Random instance.
        /// </summary>
        private static readonly object RngLock = new object();

        /// <summary>
        /// Performs an in-place Fisher-Yates shuffle on the given list in a thread-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The collection to shuffle.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionGenerator"/> class with the database context.
        /// </summary>
        /// <param name="context">The Entity Framework database context.</param>
        public QuestionGenerator(ClimateGSRDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates a full <see cref="Question"/> by randomly selecting a country, constructing the introductory emissions prompt,
        /// and assembling a randomized set of hints.
        /// </summary>
        /// <returns>A populated <see cref="Question"/> ready for presentation in the game view.</returns>
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

        /// <summary>
        /// Queries the database for a random country meeting the minimum population requirement, eagerly loading its Continent.
        /// </summary>
        /// <returns>A random <see cref="Country"/> entity.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no qualifying countries exist in the database.</exception>
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

        /// <summary>
        /// Generates a randomized list of clues (hints) for a given country based on available demographic, geographic, and climate data.
        /// </summary>
        /// <param name="c">The target country to generate hints for.</param>
        /// <returns>A shuffled list of <see cref="Hint"/> objects.</returns>
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

        /// <summary>
        /// Formats raw emissions tonnage into human-readable text scaled to billions, millions, or thousands of tonnes.
        /// </summary>
        /// <param name="tonnes">The total emissions value in tonnes.</param>
        /// <returns>A formatted descriptive string for the emissions prompt.</returns>
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