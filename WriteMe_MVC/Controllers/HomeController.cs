using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WriteMe_MVC.Models;

namespace WriteMe_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var token = HttpContext.Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                //return RedirectToAction("Index", "Home");
                return View("Index");
                //return Unauthorized("No se pudo obtener el token desde las cookies.");
            }


            try
            {
                // Decodifica el token
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                // Verifica si el token ha expirado
                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    // Si el token ha expirado, redirige al login
                    return RedirectToAction("IniciarSesion", "Usuarios");
                }

                // Obtiene el identificador del usuario desde el token
                var userId = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                // Obtén el nombre de usuario
                var userName = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
                // Asigna el nombre de usuario a ViewBag
                ViewData["UserName"] = userName;

                return View();

            }
            catch (Exception ex)
            {
                return RedirectToAction("IniciarSesion", "Usuarios");
            }

        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
