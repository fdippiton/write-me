﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Protocol;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using WriteMe_MVC.Models;
using WriteMe_MVC.ViewModels;

namespace WriteMe_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient httpClient;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            httpClient = new HttpClient();
        }

        public async Task <IActionResult> Index()
        {
            var token = HttpContext.Request.Cookies["AuthToken"];

            // Especificar la URL del endpoint de la API
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            List<PostViewModel> postInfo = new List<PostViewModel>();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage Res = await client.GetAsync($"{baseApiUrl}/posts");

                if (Res.IsSuccessStatusCode)
                {
                    var EquiResponse = Res.Content.ReadAsStringAsync().Result;
                    postInfo = JsonConvert.DeserializeObject<List<PostViewModel>>(EquiResponse)!;
                    Console.WriteLine(EquiResponse.ToJson());
                }
            }



            if (string.IsNullOrEmpty(token))
            {
                //return RedirectToAction("Index", "Home");
                return View("Index", postInfo);
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

                return View(postInfo);

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
