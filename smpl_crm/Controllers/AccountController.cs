using smpl_crm.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using smpl_crm.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace smpl_crm.Controllers
{
    public class AccountController : Controller
    {
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private Task<Data.MstUser> GetUserFromDatabaseAsync(string username, string password)
        {
            // Run the LINQ-to-SQL query synchronously inside Task.Run 
            // to avoid blocking the ASP.NET request thread
            return Task.Run(() =>
            {
                using (var context = new smpl_crmDataContext())
                {
                    var user = context.MstUsers
                        .FirstOrDefault(u => u.Username == username); // sync query

                    if (user == null)
                        return null; // User not found

                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                    return isPasswordValid ? user : null;
                }
            });
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await GetUserFromDatabaseAsync(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Create claims - NameIdentifier claim is required for antiforgery
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                // add more claims if needed
            };

            var identity = new ClaimsIdentity(claims, "ApplicationCookie"); // Match AuthenticationType from Startup.cs

            AuthenticationManager.SignOut("ApplicationCookie"); // Clear any existing cookies
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut("ApplicationCookie");
            return RedirectToAction("Index", "Home");
        }
    }

    public class LoginModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Username { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Password { get; set; }
    }
}
