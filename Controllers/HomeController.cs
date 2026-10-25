using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClimateGSR.Controllers
{
    /// <summary>
    /// Handles top-level navigation pages including the Home/Game landing page, About page, and Contact page.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Displays the main application landing and game interface.
        /// </summary>
        /// <returns>The primary index view.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays descriptive information regarding the Climate Global Status Report project.
        /// </summary>
        /// <returns>The about view.</returns>
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Displays project and support contact information.
        /// </summary>
        /// <returns>The contact view.</returns>
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}