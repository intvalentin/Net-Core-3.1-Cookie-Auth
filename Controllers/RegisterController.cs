using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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


namespace app.Controllers
{
    
    public class RegisterController : Controller
    {
        public readonly youtubeContext _context;
        public readonly IConfiguration configuration;
        [TempData]
        private string Code { get; set; }
       
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
                await CreateCookie(user.Email,user.Username,user.AvatarLocation);

                return RedirectToAction("Index","Home");
            }
            return Json(new { success = false, responseText = "Wrong Code!" });

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


            
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(user.Email, "To " + user.PrimaryName + " " + user.SecondName));
                    message.From = new MailAddress("from@email.com", "MyApp Email Verification");
                    message.Subject = "Email Verification";
                    message.Body = "<strong>Verification code: " + verificationCode + "</strong>";
                    message.IsBodyHtml = true;

                    using (var client = new SmtpClient("smtp.gmail.com"))
                    {
                        client.Port = 587;
                        client.Credentials = new NetworkCredential(configuration.GetConnectionString("Gmail"), configuration.GetConnectionString("GmailPassowrd"));
                        client.EnableSsl = true;
                        await client.SendMailAsync(message);
                        client.ServicePoint.CloseConnectionGroup(client.ServicePoint.ConnectionName);
                    }
                }
           
      
           

           
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