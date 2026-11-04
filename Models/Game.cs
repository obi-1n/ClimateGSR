using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClimateGSR.Models
{
    public class Game
    {

        public Game()
        {
            Questions = new List<Question>();
        }

        public int Score {  get; set; }
        public List<Question> Questions { get; set; }

    }
}