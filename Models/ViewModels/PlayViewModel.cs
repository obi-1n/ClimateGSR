using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models.ViewModels
{
    public class PlayViewModel
    {

        public int RoundNumber { get; set; }
        public int TotalRounds { get; set; }
        public int Score { get; set; }
        public string EmissionsPrompt { get; set; }
        public List<Hint> RevealedHints { get; set; }
        public bool CanRevealMore { get; set; }
        public int PointsAvailable { get; set; }
        public string Feedback { get; set; }

    }
}