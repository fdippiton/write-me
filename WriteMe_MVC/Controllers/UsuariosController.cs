using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
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
                    return RedirectToAction("Index", "Home");
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
