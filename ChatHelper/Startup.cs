using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ChatHelper.Startup))]
namespace ChatHelper
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
