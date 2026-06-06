using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(InterfazHubSpot.Startup))]
namespace InterfazHubSpot
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
        }
    }
}


