using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using WriteMe_MVC.Models;
using WriteMe_MVC.ViewModels;

namespace WriteMe_MVC.Controllers
{
    public class FavoritosController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient httpClient;


        public FavoritosController(IConfiguration configuration)
        {
            _configuration = configuration;
            httpClient = new HttpClient();
        }

        // GET: FavoritosController
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<FavoritoViewModel>>> GetFavoritos()
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
                string apiUrl = $"{baseApiUrl}/favoritos/{intUserId}";

                // Realiza la solicitud a la API
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseApiUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var FavViewModels = JsonConvert.DeserializeObject<List<FavoritoViewModel>>(data);

                        return View(FavViewModels);
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

        // GET: FavoritosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: FavoritosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FavoritosController/Create
        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(int id, int idUsuario)
        //{
        //    string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

        //    var token = HttpContext.Request.Cookies["AuthToken"];

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        return Unauthorized("No se pudo obtener el token desde las cookies.");
        //    }

        //    // Decodifica el token
        //    var handler = new JwtSecurityTokenHandler();
        //    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

        //    // Obtiene el identificador del usuario desde el token
        //    var userId = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;


        //    Favorito favorito = new Favorito();

        //    if (int.TryParse(userId, out int userIdValue))
        //    {
        //        favorito.FavUsuarioId = userIdValue;
        //        favorito.FavPost = id;
        //    }


        //    string apiUrl = $"{baseApiUrl}/favoritos/ObtenerFavorito/{favorito.FavPost}/{favorito.FavUsuarioId}";

        //    // Realiza la solicitud a la API
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(baseApiUrl);
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        HttpResponseMessage response = await client.GetAsync(apiUrl);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var data = await response.Content.ReadAsStringAsync();
        //            var FavViewModels = JsonConvert.DeserializeObject<FavoritoViewModel>(data);

        //            if (FavViewModels != null)
        //            {
        //                Console.WriteLine(FavViewModels.ToJson());

        //            } else
        //            {
        //                try
        //                {
        //                    var favoritoTask = httpClient.PostAsJsonAsync<Favorito>($"{baseApiUrl}/favoritos", favorito);
        //                    favoritoTask.Wait();
        //                    var result = favoritoTask.Result;

        //                    if (result.IsSuccessStatusCode)
        //                    {
        //                        var Uri = HttpContext.Request.Headers["Referer"].ToString();

        //                        // Si la URL de referencia es válida y pertenece al mismo dominio, redirige a esa URL
        //                        if (!string.IsNullOrEmpty(Uri) && Uri.StartsWith(Request.Scheme + "://" + Request.Host))
        //                        {
        //                            return Redirect(Uri);
        //                        }

        //                    }
        //                    else
        //                    {
        //                        Console.WriteLine("Response Content: " + result.Content.ReadAsStringAsync().Result);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine("Error: " + ex.Message);

        //                    ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el equipo.");
        //                }
        //                //var Url = $"{Request.Scheme}://{Request.Host}/PostDetalles/{favorito.FavPost}";
        //                var Url = HttpContext.Request.Headers["Referer"].ToString();

        //                return Redirect(Url);
        //            }

        //        }
        //        else
        //        {
        //            Console.WriteLine("Error al llamar a la Web API: " + response.ReasonPhrase);
        //            ModelState.AddModelError(String.Empty, "Error al obtener datos. Código de estado: " + response.StatusCode);
        //            return StatusCode((int)response.StatusCode, "Error al obtener datos desde la API.");
        //        }
        //    }

        //    var refererUrl = HttpContext.Request.Headers["Referer"].ToString();

        //    return Redirect(refererUrl);
        //}

        // GET: FavoritosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: FavoritosController/Edit/5
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

        // GET: FavoritosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: FavoritosController/Delete/5
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
