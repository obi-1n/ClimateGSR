using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    public class Question
    {
        public Country Country { get; set; }
        public List<Hint> Hints { get; set; }
        public string Answer { get; set; }
        public string EmissionsPrompt { get; set; }   // shown at the start, not a hint
    }
}