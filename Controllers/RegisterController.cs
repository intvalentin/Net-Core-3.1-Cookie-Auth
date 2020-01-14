using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using app.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace app.Controllers
{
    public class RegisterController : Controller
    {
        public readonly youtubeContext _context;
        public readonly IConfiguration configuration;
        public RegisterController(youtubeContext context, IConfiguration configuration)
        {
            _context = context;
            this.configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

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
                new RNGCryptoServiceProvider().GetBytes(verificationCodeByte = new byte[2]);
                var verificationCode = Convert.ToBase64String(verificationCodeByte);

                Execute(user, verificationCode).Wait();
                user = user;

                HttpContext.Session.SetString("SessionEmailVerificationcode", verificationCode);
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
    }
}