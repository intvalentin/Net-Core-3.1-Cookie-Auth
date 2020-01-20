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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace app.Controllers
{
    
    public class RegisterController : Controller
    {
        public readonly youtubeContext _context;
        public readonly IConfiguration configuration;
        [TempData]
        private string Code { get; set; }
        [TempData]
        private Users User { get; set; }
        public RegisterController(youtubeContext context, IConfiguration configuration)
        {
            _context = context;
            this.configuration = configuration;
        }
        [AnonymousOnlyFilter]
        public IActionResult Index()
        {
            return View();
        }
        [AnonymousOnlyFilter]
        [HttpPost]

        public async Task<IActionResult> VerifyEmail([Bind("Code")] string code)
        {
            if(code == JsonConvert.DeserializeObject<string>(TempData.Peek("Code").ToString()) && code != null)
            {
                Users user = TempData.Get<Users>("user");
                _context.Add(user);
                await _context.SaveChangesAsync();
                CreateCookie(user.Email,user.Username,user.AvatarLocation);

                return Json(new { success = true, responseText = TempData["Code"]});
            }
            return Json(new { success = false, responseText = code+" sss "+ User.Email });

        }
        [AnonymousOnlyFilter]
        [HttpPost]
        public async Task<IActionResult> Register([Bind("PrimaryName,SecondName,Username,Email,Password")] Users user)
        {
            if (ModelState.IsValid)
            {
                Users checkIfEmailExists = await _context.Users.FirstOrDefaultAsync(m => m.Email == user.Email);
                Users checkIfUserExists = await _context.Users.FirstOrDefaultAsync(m => m.Username == user.Username);

                if (checkIfEmailExists != null && checkIfUserExists == null) return Json(new { success = false, responseText = "Email allready exists!"});
                else if (checkIfEmailExists == null && checkIfUserExists != null) return Json(new { success = false, responseText = "Username allready exists!" });
                else if (checkIfEmailExists != null && checkIfUserExists != null) return Json(new { success = false, responseText = "Username and Email allready exists!" });

                user.AvatarLocation = "/images/avatara.svg";
                user.JoinedDate = DateTime.Now;
                string[] passwordHash = hashPassword(user.Password);
                user.Password = passwordHash[0];
                user.Salt = passwordHash[1];

                // Generating code
                byte[] verificationCodeByte;
                new RNGCryptoServiceProvider().GetBytes(verificationCodeByte = new byte[3]);
                var verificationCode = Convert.ToBase64String(verificationCodeByte);

               
                TempData.Put<Users>("user", user);
                TempData["Code"] = JsonConvert.SerializeObject(verificationCode);
                Execute(user, verificationCode).Wait();
                




                return Json(new { success = true, responseText = "Code sent! Check your inbox!" });
            }
            return Json(new { success = false, responseText = "Invalid data!" });
        }
        private string[] hashPassword(string password)
        {
            string[] pass = new string[2];
            byte[] salt;
            /* generate salt */
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            pass[0] = Convert.ToBase64String(hash);
            pass[1] = Convert.ToBase64String(salt);
            return pass;
        }
        private async Task  Execute(Users user,string verificationCode)
        {
            
            //var apiKey = "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";

            var apiKey = configuration.GetConnectionString("SendGrid");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("myapp@example.com", "youtube");
            var subject = "Email Verification";
            var to = new EmailAddress(user.Email, user.PrimaryName + " " + user.SecondName);
            var plainTextContent = " ";
            var htmlContent = "<strong>Verification code: " + verificationCode + "</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            _ = await client.SendEmailAsync(msg);

            
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

    }
}