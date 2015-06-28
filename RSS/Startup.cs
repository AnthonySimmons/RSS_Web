using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RSS.Startup))]
namespace RSS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
