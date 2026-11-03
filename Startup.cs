using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClimateGSR.Startup))]
namespace ClimateGSR
{
    /// <summary>
    /// OWIN application startup class for configuring middleware components.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// Configures OWIN pipeline middleware including cookie authentication and identity context.
        /// </summary>
        /// <param name="app">The OWIN application builder interface.</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
