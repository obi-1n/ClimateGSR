using ClimateGSR.Models;
using ClimateGSR.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClimateGSR.Controllers
{
    //Controls the flow of a single game: starting it, showing each round,
    //handling guesses/hints/give-up, and showing the final result.
    //The game's progress is kept in the session between requests.
    public class GameController : Controller
    {
        //Game settings
        private const int Rounds = 5;       //Number of questions per game
        private const int MaxPoints = 100;  //Points for a correct answer with no hints
        private const int HintCost = 25;    //Points lost for each hint revealed

        //Gets the current player's game state from the session.
        //Returns null if no game has been started.
        private GameState State
        {
            get { return Session["GameState"] as GameState; }
        }

        //Creates a new game with a set of random questions, stores it in the
        //session, and sends the player to the first round.
        public ActionResult Start()
        {
            var game = new Game { Score = 0, Questions = new List<Question>() };
            var used = new HashSet<string>();   //Tracks answers already used in this game

            using (var db = new ClimateGSRDbContext())
            {
                var generator = new QuestionGenerator(db);

                //Keep generating until we have enough questions. The attempts limit
                //stops an endless loop if there aren't enough unique countries.
                for (int attempts = 0; game.Questions.Count < Rounds && attempts < 50; attempts++)
                {
                    var q = generator.GenerateQuestion();
                    if (used.Add(q.Answer))          //Add returns false if already present, so no repeated countries
                        game.Questions.Add(q);
                }
            }

            //Save the game so later requests can continue it
            Session["GameState"] = new GameState { Game = game };
            return RedirectToAction("Play");
        }

        //Shows the current round: the emissions prompt, any hints revealed so far,
        //the current score and the points still available.
        public ActionResult Play()
        {
            var s = State;
            if (s == null) return RedirectToAction("Start");           //No game in progress, so start one
            if (s.Finished) return RedirectToAction("Result");         //All rounds done, so show results

            var q = s.Current;

            //Answer is deliberately left out so it doesn't reach the browser.
            var model = new PlayViewModel
            {
                RoundNumber = s.CurrentIndex + 1,
                TotalRounds = s.Game.Questions.Count,
                Score = s.Game.Score,
                EmissionsPrompt = q.EmissionsPrompt,
                RevealedHints = q.Hints.Take(s.HintsRevealed).ToList(), //Only hints the player has unlocked
                CanRevealMore = s.HintsRevealed < q.Hints.Count,        //Hide the button when none are left
                PointsAvailable = PointsFor(s),
                Feedback = TempData["Feedback"] as string               //One-off message from the previous action
            };
            return View(model);
        }

        //Checks the player's guess. A correct guess scores points and moves to the
        //next round. A wrong guess reveals another hint, or ends the round if none remain.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Guess(string guess)
        {
            var s = State;
            if (s == null || s.Finished) return RedirectToAction("Start");

            var q = s.Current;

            //Compare ignoring case and extra spaces
            if (Normalise(guess) == Normalise(q.Answer))
            {
                int points = PointsFor(s);          //Fewer points the more hints were used
                s.Game.Score += points;
                return Advance(s, "Correct! It was " + q.Answer + ". +" + points + " points.");
            }

            //Wrong: reveal another hint, or end the question if none are left
            if (s.HintsRevealed < q.Hints.Count)
            {
                s.HintsRevealed++;
                TempData["Feedback"] = "Not quite. Here's another hint.";
                return RedirectToAction("Play");
            }

            //Wrong and no hints left: no points, move on
            return Advance(s, "Out of hints. It was " + q.Answer + ".");
        }

        //Reveals one more hint for the current question (costs points).
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RevealHint()
        {
            var s = State;
            if (s == null || s.Finished) return RedirectToAction("Start");

            //Only reveal if hints remain
            if (s.HintsRevealed < s.Current.Hints.Count)
                s.HintsRevealed++;

            return RedirectToAction("Play");
        }

        //Skips current question with no points awarded, showing the answer.
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult GiveUp()
        {
            var s = State;
            if (s == null || s.Finished) return RedirectToAction("Start");

            return Advance(s, "The answer was " + s.Current.Answer + ".");
        }

        //Shows final score once every round has been completed.
        public ActionResult Result()
        {
            var s = State;
            if (s == null) return RedirectToAction("Start");
            if (!s.Finished) return RedirectToAction("Play");   //Don't allow skipping ahead to results

            ViewBag.Feedback = TempData["Feedback"];            //Message about the final round
            return View(s.Game);
        }

        //HELPER FUNCTIONS
        //---------------------------------------------------------------------------------------------

        //Moves on to the next question: resets the hint counter, stores a
        //feedback message for the next page, and redirects to Play or Result.
        private ActionResult Advance(GameState s, string feedback)
        {
            s.CurrentIndex++;
            s.HintsRevealed = 0;
            TempData["Feedback"] = feedback;   //TempData survives exactly one redirect
            return RedirectToAction(s.Finished ? "Result" : "Play");
        }

        //Points a correct answer is worth right now: 100 with no hints,
        //dropping by 25 per hint revealed, never below 0.
        private static int PointsFor(GameState s)
        {
            return Math.Max(0, MaxPoints - HintCost * s.HintsRevealed);
        }

        //Prepares text for comparing guesses: trims extra spaces between and
        //around words, and lowercases it. A null guess is treated as empty.
        private static string Normalise(string text)
        {
            return string.Join(" ", (text ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                         .ToLowerInvariant();
        }

    }
}