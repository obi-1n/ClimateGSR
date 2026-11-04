using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ClimateGSR.Models;

namespace ClimateGSR.Controllers
{
    public class GamesController : Controller
    {
        [HttpGet]
        public ActionResult Leaderboard()
        {
            using (var context = new ClimateGSRDbContext())
            {
                return View(new LeaderboardService(context).GetTopPlayers(User.Identity.GetUserId()));
            }
        }
    }
}
