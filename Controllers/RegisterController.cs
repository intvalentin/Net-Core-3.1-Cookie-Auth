using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace app.Controllers
{
    public class RegisterController : Controller
    {
        public readonly youtubeContext _context;

        public RegisterController(youtubeContext context)
        {
            _context = context;
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

              
            }
        }

    }
}