using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ClimateGSR.Models;

namespace ClimateGSR
{
    /// <summary>
    /// Global application lifecycle handler for the ClimateGSR ASP.NET MVC application.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Application startup entry point executed once when the web server process initializes.
        /// Configures areas, global filters, routing rules, asset bundles, and database initialization.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
