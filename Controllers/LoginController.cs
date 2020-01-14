using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using app.Models;
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
        public IActionResult Index()
        {
            return View();
        }
      

        ////Login
        //public async Task<IActionResult> Register([Bind("Id,Username,PrimaryName,SecondName,Email,Password,Salt,JoinedDate,AvatarLocation")]Users user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var checkEmailExists = await _context.Users.FirstOrDefaultAsync(m => m.Email == user.Email);
        //        var checkIfUserExists = await _context.Users.FirstOrDefaultAsync(m => m.Username == user.Username);

        //    }

        //    return View();
        //}

    }
}