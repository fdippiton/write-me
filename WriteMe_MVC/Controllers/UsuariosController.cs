﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using WriteMe_MVC.Models;
using WriteMe_MVC.ViewModels;

namespace WriteMe_MVC.Controllers
{
    public class UsuariosController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly HttpClient httpClient;
        private readonly IHttpClientFactory _httpClientFactory;


        public UsuariosController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            httpClient = new HttpClient();
            _httpClientFactory = httpClientFactory;
        }

        // GET: UsuariosController/Details/5
        [HttpGet]
        public ActionResult IniciarSesion()
        {
            return View();
        }


        // POST: UsuariosController
        [HttpPost]
        [AllowAnonymous]
        public async Task <IActionResult> IniciarSesion(LoginUser userLogin)
        {
            try
            {
                var token = await ObtenerTokenDesdeAPI(userLogin);

                if (!string.IsNullOrEmpty(token))
                {
                    // Almacenar el token en Local Storage
                    HttpContext.Response.Cookies.Append("AuthToken", token);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, "UsuNombre"),  // Ajusta esto según la información de tu usuario
                        // Otras claims según la información de usuario que tengas
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // Autenticar al usuario
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                   

                    Console.WriteLine($"User.Identity.Name: {User.Identity.Name}");
                    Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity.IsAuthenticated}");

                    // Redirigir a la página deseada después de iniciar sesión
                    return RedirectToAction("GetPostsForCurrentUser");
                }

                return NotFound("No se pudo loguear. Usuario no encontrado");
            }
            catch (Exception ex)
            {
                // Manejar la excepción según tus necesidades
                Console.WriteLine($"Error en el método IniciarSesion: {ex}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task<string> ObtenerTokenDesdeAPI(LoginUser userLogin)
        {
            // Configurar el cliente HTTP
            var httpClient = _httpClientFactory.CreateClient();

            // Especificar la URL del endpoint de la API
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            // Configurar la solicitud HTTP
            var request = new HttpRequestMessage(HttpMethod.Post, $"{baseApiUrl}/login");
            request.Content = new StringContent(JsonConvert.SerializeObject(userLogin), Encoding.UTF8, "application/json");

            // Enviar la solicitud y obtener la respuesta
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Leer el token de la respuesta
                var token = await response.Content.ReadAsStringAsync();
                return token;
            }

            return null;
        }

        [HttpPost]
        [Authorize]
        public IActionResult CerrarSesion()
        {
            // Implementa el código para cerrar la sesión aquí

            // Por ejemplo, puedes eliminar la cookie de autenticación
            HttpContext.SignOutAsync();

            // Redirige al usuario a la página de inicio u otra página después de cerrar sesión
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PostViewModel>>> GetPostsForCurrentUser()
        {
            try
            {
                // Obtiene el token desde las cookies
                var token = HttpContext.Request.Cookies["AuthToken"];

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("No se pudo obtener el token desde las cookies.");
                }

                // Decodifica el token
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                // Obtiene el identificador del usuario desde el token
                var userId = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                // Obtén el nombre de usuario
                var userName = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
                // Asigna el nombre de usuario a ViewBag
                ViewData["UserName"] = userName;

                Console.WriteLine(userName);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("No se pudo obtener el identificador del usuario desde el token.");
                }

                // Convierte el identificador del usuario a un entero
                if (!int.TryParse(userId, out var intUserId))
                {
                    return Unauthorized("El identificador del usuario en el token no es válido.");
                }

                // Construye la URL de la API utilizando el identificador del usuario
                string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;
                string apiUrl = $"{baseApiUrl}/posts/postsByUser/{intUserId}";

                // Realiza la solicitud a la API
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseApiUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();


                        // Verifica si el JSON es una lista o un objeto individual
                        if (data.StartsWith("[") && data.EndsWith("]"))
                        {
                            var postViewModels = JsonConvert.DeserializeObject<List<PostViewModel>>(data);
                            return View(postViewModels);
                        }
                        else
                        {
                            var postViewModel = JsonConvert.DeserializeObject<PostViewModel>(data);
                            return View(new List<PostViewModel> { postViewModel });
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error al llamar a la Web API: " + response.ReasonPhrase);
                        ModelState.AddModelError(String.Empty, "Error al obtener datos. Código de estado: " + response.StatusCode);
                        return StatusCode((int)response.StatusCode, "Error al obtener datos desde la API.");
                    }
                }
               
            }
            catch (Exception ex)
            {
                // Registra la excepción para obtener más detalles en los registros
                Console.WriteLine($"Error al realizar la operación GET: {ex}");

                // Registra la excepción interna si está presente
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException}");
                }

                // Devuelve un error interno del servidor con un mensaje personalizado
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        // GET: UsuariosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] Usuario usuario)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            try
            {
                var postTask = httpClient.PostAsJsonAsync<Usuario>($"{baseApiUrl}/usuarios", usuario);
                postTask.Wait();
                var result = postTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("IniciarSesion");
                }
                else
                {
                    Console.WriteLine("Response Content: " + result.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);

                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario.");
            }
            return View(usuario);
        }

        // GET: UsuariosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UsuariosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsuariosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}