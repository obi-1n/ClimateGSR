using System.Web;
using System.Web.Mvc;

namespace ClimateGSR
{
    /// <summary>
    /// Configures global action filters applied across all MVC controllers.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers global action and exception filters.
        /// </summary>
        /// <param name="filters">The global filter collection.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Catches unhandled exceptions and redirects to the Error view
            filters.Add(new HandleErrorAttribute());
        }
    }
}
