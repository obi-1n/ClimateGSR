using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClimateGSR
{
    /// <summary>
    /// Configures URL routing rules and maps request paths to controllers and actions.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers route handlers into the global route collection.
        /// </summary>
        /// <param name="routes">The route collection to configure.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Ignore ASP.NET resource handlers (e.g., WebResource.axd)
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Default route pattern: /{controller}/{action}/{id}
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
