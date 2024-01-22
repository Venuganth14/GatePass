using GatePass_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net.Http;

namespace GatePass.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NewLogin(Login model)
        {
            if (ModelState.IsValid)
            {
                var claims = new List<Claim>();
                if (model.Username.StartsWith("a", StringComparison.OrdinalIgnoreCase) && model.Password != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "Admin")));
                    TempData["Message"] = "Admin " + model.Username + " logged in successfully!";
                    return RedirectToAction("Index", "Home");
                }
                if (model.Username.StartsWith("u", StringComparison.OrdinalIgnoreCase) && model.Password != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "User")));
                    TempData["Message"] = "User " + model.Username + " logged in successfully!";
                    return RedirectToAction("Index", "Home");
                }
                if (model.Username.StartsWith("e", StringComparison.OrdinalIgnoreCase) && model.Password != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "executive"));
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "executive")));
                    TempData["Message"] = "Executive " + model.Username + " logged in successfully!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                    TempData["Error"] = "Invalid username or password!";
                    return View("Login", model);
                }
            }
            TempData["Error"] = "Please fill in all fields!";
            return View("Login");
        }

        
    }
}
