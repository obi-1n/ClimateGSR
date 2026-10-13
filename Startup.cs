using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ClimateGSR.Startup))]
namespace ClimateGSR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
