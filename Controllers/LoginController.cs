using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using app.Logic;
using app.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace app.Controllers
{
   
    public class LoginController : Controller
    {

        public readonly youtubeContext _context;

        public LoginController(youtubeContext context)
        {
            _context = context;
        }
        [AnonymousOnlyFilter]
        public IActionResult Index()
        {
            return View();
        }


        ////Login
        
        [HttpPost]
        [AnonymousOnlyFilter]
        public async Task<IActionResult> Login([Bind("Email,Password")]Users u)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == u.Email);
                if (user == null) return RedirectToAction("Index", "Login");
                else
                {
                    string[] dataUser = new string[3];
                    dataUser[0] = user.Password;
                    dataUser[1] = user.Salt;
                    dataUser[2] = u.Password;
                    if (hashPassword(dataUser))
                    {
                        CreateCookie(user.Email, user.Username, user.AvatarLocation);
                        return RedirectToAction("Index", "Users");
                    }
                }

                
            }
            return View("~/Views/Register/Index.cshtml");

        }

        private bool hashPassword(string[] dataUser)
        {
            byte[] dbSalt = Convert.FromBase64String(dataUser[1]);
            byte[] dbHash = Convert.FromBase64String(dataUser[0]);
            var pbkdf2 = new Rfc2898DeriveBytes(dataUser[2], dbSalt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            if (hash.SequenceEqual(dbHash))
                return true;
            else
                return false;
        }
        public async Task CreateCookie(string email, string username,string avatar)
        {
            var claims = new List<Claim>
            {

                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.UserData, avatar),
                new Claim(ClaimTypes.Role, "Normal"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120),
                IsPersistent = true,
                IssuedUtc = DateTime.Now,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
                );

        }
        
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}