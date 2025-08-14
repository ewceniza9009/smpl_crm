using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using smpl_crm.Models;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using smpl_crm.Data;

namespace smpl_crm.Controllers
{
    public class TokenController : Controller
    {
        [HttpPost]
        [AllowAnonymous]
        [Route("token")]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return new HttpStatusCodeResult(400, "Invalid request");

            using (var db = new smpl_crmDataContext())
            {
                var user = db.MstUsers.FirstOrDefault(u => u.Username == username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return new HttpStatusCodeResult(401, "Invalid username or password");

                var token = GenerateJwtToken(user);
                int expiryMinutes = int.Parse(ConfigurationManager.AppSettings["jwt:TokenExpiryMinutes"] ?? "60");

                return Json(new
                {
                    access_token = token,
                    token_type = "bearer",
                    expires_in = expiryMinutes
                });
            }
        }

        private string GenerateJwtToken(Data.MstUser user)
        {
            string issuer = ConfigurationManager.AppSettings["jwt:Issuer"];
            string audience = ConfigurationManager.AppSettings["jwt:Audience"];
            string secretBase64 = ConfigurationManager.AppSettings["jwt:SecretBase64"];
            var key = Convert.FromBase64String(secretBase64);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // add role claim if you have one: new Claim(ClaimTypes.Role, "Admin")
            };

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["jwt:TokenExpiryMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
