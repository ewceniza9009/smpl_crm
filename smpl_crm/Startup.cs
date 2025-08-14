using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System.Linq;
using System.Security.Claims;

[assembly: OwinStartup(typeof(JwtDemoMvc.Startup))]

namespace JwtDemoMvc
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = context =>
                    {
                        var claims = context.Identity.Claims.ToList();

                        // If NameIdentifier claim is missing, add it here
                        if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                        {
                            // You need to fetch user id somehow, e.g. from Name or external source
                            // For example, get username from claim and then load user ID from DB
                            var username = context.Identity.Name;

                            // Fetch user ID from DB here synchronously or async (async more complex)
                            // For demo, let's say userId = "123";
                            var userId = "123";

                            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

                            var newIdentity = new ClaimsIdentity(claims, context.Identity.AuthenticationType);
                            context.ReplaceIdentity(newIdentity);
                        }

                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                }
            });
        }
    }
}
