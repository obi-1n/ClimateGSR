using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    /// <summary>
    /// Represents a generated quiz/trivia question containing the target country, clues, and prompt.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// The target country object that the user is trying to guess.
        /// </summary>
        public Country Country { get; set; }

        /// <summary>
        /// Shuffled list of progressive clues/hints available for this question.
        /// </summary>
        public List<Hint> Hints { get; set; }

        /// <summary>
        /// The expected correct answer string (the country's name).
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Initial prompt text displaying annual emissions data for the mysterious country.
        /// </summary>
        public string EmissionsPrompt { get; set; }   // shown at the start, not a hint
    }
}