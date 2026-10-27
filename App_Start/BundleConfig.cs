using System.Web;
using System.Web.Optimization;

namespace ClimateGSR
{
    /// <summary>
    /// Configures script and stylesheet bundling and minification for client-side assets.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Registers client JavaScript and CSS bundles with the bundle table.
        /// </summary>
        /// <param name="bundles">The bundle collection to register assets into.</param>
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Core jQuery library bundle
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // jQuery validation scripts for client-side form validation
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Modernizr for feature detection in modern web browsers
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Bootstrap JavaScript library bundle
            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            // Site-wide CSS style bundle containing Bootstrap and custom styling
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
