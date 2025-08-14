using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using smpl_crm.Data;
using smpl_crm.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
                new Claim(ClaimTypes.Name, user.FullName),
                // add more claims if needed
            };

            var identity = new ClaimsIdentity(claims, "ApplicationCookie"); // Match AuthenticationType from Startup.cs

            AuthenticationManager.SignOut("ApplicationCookie"); // Clear any existing cookies
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [Authorize]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterUserModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                using (var context = new smpl_crmDataContext())
                {
                    var existingUser = context.MstUsers.FirstOrDefault(u => u.Username == model.Username);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Username already taken.");
                        return View(model);
                    }

                    var user = new Data.MstUser
                    {
                        Username = model.Username,
                        FullName = model.FullName,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                        OrigPassword = model.Password
                    };

                    context.MstUsers.InsertOnSubmit(user);
                    context.SubmitChanges();

                    return RedirectToAction("MstUserListView", "User");
                }
            }

            return View(model);
        }

        // GET: Account/ChangePassword
        [Authorize]
        public ActionResult ChangePassword()
        {
            var claimsPrincipal = User as ClaimsPrincipal;
            var userId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var model = new ChangePasswordModel
            {
                UserId = Convert.ToInt32(userId),
            };
            return View(model);
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            // Get UserId from JWT claims
            var userId = model.UserId.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Unable to identify the current user.";
                return View(model);
            }

            using (var context = new smpl_crmDataContext())
            {
                var user = context.MstUsers.FirstOrDefault(u => u.Id == Convert.ToInt32(userId));
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return View(model);
                }

                // Verify current password
                if(model.CurrentPassword != null)
                {
                    if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                    {
                        TempData["ErrorMessage"] = "Current Password doesn't match!";
                        return View(model);
                    }
                    else
                    {
                        // Update password
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        user.OrigPassword = model.NewPassword;
                        context.SubmitChanges();

                        TempData["SuccessMessage"] = "Password changed successfully!";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            return View(model);
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
    public class RegisterUserModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Password doesn't match")]

        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
    public class ChangePasswordModel
    {
        public int UserId { get; set; }

        [Display(Name = "Current Password")]
        [Required(ErrorMessage = "Current Password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "New Password doesn't match")]
        public string ConfirmPassword { get; set; }
    }
}
