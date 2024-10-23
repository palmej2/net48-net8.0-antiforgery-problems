using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(WebNet48.Startup))]
namespace WebNet48
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<CustomSessionMiddleware>();
        }
    }
}