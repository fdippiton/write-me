using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

        [HttpGet]
        public ActionResult UserNotFound()
        {
            return View();
        }

        // Perfil de Usuario
        [HttpGet]
        [Authorize]
        public async Task <ActionResult> Edit()
        {
            Usuario usuario = new Usuario();

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
                ViewData["UserId"] = userId;
                ViewBag.UserId = userId;


                //Console.WriteLine(userName);

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
                string apiUrl = $"{baseApiUrl}/usuarios/{intUserId}";
              

                // Realiza la solicitud a la API
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseApiUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        usuario = JsonConvert.DeserializeObject<Usuario>(data);
                    }
                    else
                    {
                        //Console.WriteLine("Error al llamar a la Web API: " + response.ReasonPhrase);
                        ModelState.AddModelError(String.Empty, "Error al obtener datos. Código de estado: " + response.StatusCode);
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

            return View(usuario);
        }

        // POST: UsuariosController/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, [FromForm] Usuario usuario)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(JsonConvert.SerializeObject(usuario), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync($"{baseApiUrl}/usuarios/actualizarPerfil/" + id.ToString(), content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("GetPostsForCurrentUser", "Usuarios");
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Error al actualizar el articulo. Código de estado: " + response.StatusCode);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(String.Empty, "Error de conexión: " + ex.Message);
            }
            catch (JsonException ex)
            {
                ModelState.AddModelError(String.Empty, "Error al deserializar los datos: " + ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(String.Empty, "Error: " + ex.Message);
            }
            return View(usuario);
            //return RedirectToAction("PostDetails", "Posts");
        }


        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(int id, [FromForm] Usuario usuario)
        //{
        //    string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Clear();
        //            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //            string serializedJson = JsonConvert.SerializeObject(usuario);
        //            var content = new StringContent(serializedJson, Encoding.UTF8, "application/json");
        //            Console.WriteLine(content);

        //            HttpResponseMessage response = await client.PutAsync($"{baseApiUrl}/usuarios/actualizarPerfil/" + id.ToString(), content);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                return RedirectToAction("GetPostsForCurrentUser", "Usuarios");
        //            }
        //            else if (response.StatusCode == HttpStatusCode.BadRequest)
        //            {
        //                ModelState.AddModelError(String.Empty, "La solicitud no es válida. Revise los datos proporcionados.");
        //                var errorResponse = await response.Content.ReadAsStringAsync();
        //                Console.WriteLine($"Error en la respuesta del servidor: {errorResponse}");
        //            }
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        ModelState.AddModelError(String.Empty, "Error de conexión: " + ex.Message);
        //    }
        //    catch (JsonException ex)
        //    {
        //        ModelState.AddModelError(String.Empty, "Error al deserializar los datos: " + ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError(String.Empty, "Error: " + ex.Message);
        //    }
        //    //return View(usuario);
        //    return RedirectToAction("GetPostsForCurrentUser", "Usuarios");
        //}


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

                return RedirectToAction("UserNotFound");
                //return NotFound("No se pudo loguear. Usuario no encontrado");
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
            // Elimina la cookie de autenticación
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Devuelve un script JavaScript para eliminar el token del localStorage
            return Content("<script>document.cookie = 'AuthToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'; localStorage.removeItem('AuthToken'); window.location.href='/Home/Index';</script>", "text/html");

            // Por ejemplo, puedes eliminar la cookie de autenticación
            //HttpContext.SignOutAsync();

            // Redirige al usuario a la página de inicio u otra página después de cerrar sesión
            //return Content("<script>localStorage.removeItem('AuthToken'); window.location.href='/Home/Index';</script>", "text/html");
            //return RedirectToAction("Index", "Home");
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
                ViewData["UserId"] = userId;
                ViewBag.UserId = userId;


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
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        // GET: UsuariosController/Delete/5
        [HttpGet]
        public async Task <ActionResult> Delete(int id)
        {
            Usuario usuario = new Usuario();

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
                ViewData["UserId"] = userId;
                ViewBag.UserId = userId;


                //Console.WriteLine(userName);

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
                string apiUrl = $"{baseApiUrl}/usuarios/{intUserId}";


                // Realiza la solicitud a la API
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseApiUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        usuario = JsonConvert.DeserializeObject<Usuario>(data);
                    }
                    else
                    {
                        //Console.WriteLine("Error al llamar a la Web API: " + response.ReasonPhrase);
                        ModelState.AddModelError(String.Empty, "Error al obtener datos. Código de estado: " + response.StatusCode);
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

            return View(usuario);
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> Delete(string id)
        {
             string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

    try
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.PutAsync($"{baseApiUrl}/usuarios/delete/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.SignOutAsync();

                // Redirige al usuario a la página de inicio u otra página después de cerrar sesión
                return Content("<script>localStorage.removeItem('AuthToken'); window.location.href='/Home/Index';</script>", "text/html");

                // Redirigir a la acción Create después de cerrar la sesión
                //return RedirectToAction("Create", "Usuarios");
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Error al inactivar el usuario. Código de estado: " + response.StatusCode);
            }
        }
    }
    catch (HttpRequestException ex)
    {
        ModelState.AddModelError(String.Empty, "Error de conexión: " + ex.Message);
    }
    catch (Exception ex)
    {
        ModelState.AddModelError(String.Empty, "Error: " + ex.Message);
    }

    return RedirectToAction("Index", "Home");
        }
    }
}
