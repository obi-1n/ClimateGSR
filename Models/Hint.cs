using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    /// <summary>
    /// Represents an individual clue or hint presented to the player during game trivia.
    /// </summary>
    public class Hint
    {
        /// <summary>
        /// Category or metric type of the hint (e.g., "Temperature", "Population", "Continent").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// User-friendly descriptive text describing the hint value.
        /// </summary>
        public string Value { get; set; }
    }
}