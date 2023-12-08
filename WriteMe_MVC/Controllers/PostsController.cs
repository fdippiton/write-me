using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using WriteMe_MVC.Models;
using WriteMe_MVC.ViewModels;

namespace WriteMe_MVC.Controllers
{
    public class PostsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient httpClient;


        public PostsController(IConfiguration configuration)
        {
            _configuration = configuration;
            httpClient = new HttpClient();
        }

        // GET: PostsController
        public ActionResult Index()
        {
            return View();
        }



        [HttpGet("PublicPostDetalles/{id}")]

        public async Task<ActionResult> PublicPostDetails(int id)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            PostViewModel postInfo = new PostViewModel();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage Res = await client.GetAsync($"{baseApiUrl}/posts/" + id.ToString());

                if (Res.IsSuccessStatusCode)
                {
                    var EquiResponse = Res.Content.ReadAsStringAsync().Result;
                    postInfo = JsonConvert.DeserializeObject<PostViewModel>(EquiResponse)!;


                }

                return View(postInfo);
            }
        }


        // Obtener los detalles de un post especifico
        // GET: PostsController/Details/5
        [HttpGet("PostDetalles/{id}")]
        
        public async Task <ActionResult> PostDetails(int id)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            var token = HttpContext.Request.Cookies["AuthToken"];

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Obtén el identificador del usuario desde el token
            var userId = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var parsedUserId))
            {
                // El userId ahora contiene el identificador del usuario como un entero
                // Puedes usarlo según tus necesidades
                //Console.WriteLine($"UserId: {parsedUserId}");

                ViewData["UsuarioId"] = userId;

            }

            PostViewModel postInfo = new PostViewModel();
            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage Res = await client.GetAsync($"{baseApiUrl}/posts/" + id.ToString());

            
                string favApiUrl = $"{baseApiUrl}/favoritos/ObtenerFavorito/{id}/{ViewData["UsuarioId"]}";
                HttpResponseMessage response = await client.GetAsync(favApiUrl);

                var data = await response.Content.ReadAsStringAsync();
                var favViewModel = JsonConvert.DeserializeObject<FavoritoViewModel>(data);

                if (favViewModel.FavId != 0)
                {
                    ViewData["FavoritoState"] = true;
                }

                    if (Res.IsSuccessStatusCode)
                {
                    var EquiResponse = Res.Content.ReadAsStringAsync().Result;
                    postInfo = JsonConvert.DeserializeObject<PostViewModel>(EquiResponse)!;


                }

                return View(postInfo);
            }
        }

        // Obtener vista de crear nuevo post
        // GET: PostsController/Create
        [HttpGet]
        [Authorize]
        public async Task <ActionResult> Create()
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            List<Categoria> categorias = new List<Categoria>();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage ResFec = await client.GetAsync($"{baseApiUrl}/categorias/");

                if (ResFec.IsSuccessStatusCode )
                {
                    var PosResponse = ResFec.Content.ReadAsStringAsync().Result;
                    categorias = JsonConvert.DeserializeObject<List<Categoria>>(PosResponse)!;

                 
                    ViewBag.DropDownData = new SelectList(categorias, "CatId", "CatNombre");
                }
                return View();
            }
        }

        // Crear nuevo post
        // POST: PostsController/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([FromForm] Post post)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

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


            post.PostFechaPublicacion = DateTime.UtcNow;
            post.PostStatus = "A";

            if (int.TryParse(userId, out int userIdValue))
            {
                post.PostUsuarioId = userIdValue;
            }

            Console.WriteLine(post.ToJson());

            try
            {
                var postTask = httpClient.PostAsJsonAsync<Post>($"{baseApiUrl}/posts", post);
                postTask.Wait();
                var result = postTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("GetPostsForCurrentUser", "Usuarios");
                }
                else
                {
                    Console.WriteLine("Response Content: " + result.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);

                ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el equipo.");
            }
            return View(post);
        }

        // Obtener vista de editar post
        // GET: PostsController/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            // Especificar la URL del endpoint de la API
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            Post PostInfo = new Post();
            List<Categoria> categoria = new List<Categoria>();

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage ResPost = await client.GetAsync($"{baseApiUrl}/posts/" + id.ToString());
                HttpResponseMessage ResCateg = await client.GetAsync($"{baseApiUrl}/categorias/");

                if (ResPost.IsSuccessStatusCode && ResCateg.IsSuccessStatusCode)
                {
                    var PostResponse = ResPost.Content.ReadAsStringAsync().Result;
                    PostInfo = JsonConvert.DeserializeObject<Post>(PostResponse)!;

                    var CategResponse = ResCateg.Content.ReadAsStringAsync().Result;
                    categoria = JsonConvert.DeserializeObject<List<Categoria>>(CategResponse)!;

                    ViewBag.DropDownData = new SelectList(categoria, "CatId", "CatNombre");
                }

                return View(PostInfo);
            }

        }

        // Editar post
        // POST: PostsController/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> Edit(int id, [FromForm] Post post)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;
            Console.WriteLine(post.ToJson());

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(JsonConvert.SerializeObject(post), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PutAsync($"{baseApiUrl}/posts/" + id.ToString(), content);

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
            return View(post);
        }

        // GET: PostsController/Delete/5
        //[HttpGet]
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}


        // POST: PostsController/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseApiUrl);

                try
                {
                    var deleteTask = await client.DeleteAsync($"{baseApiUrl}/posts/{id}");
                    if (deleteTask.IsSuccessStatusCode)
                    {
                        return RedirectToAction("GetPostsForCurrentUser", "Usuarios");
                    }
                    else
                    {
                        // Handle the case where deletion failed, perhaps show an error message
                        ModelState.AddModelError(string.Empty, "Failed to delete the post.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while deleting the post.");
                }
            }

            // If there's an error or the deletion was unsuccessful, stay on the same page or handle as needed.
            return View();
        }



        // POST: FavoritosController/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MarcarPostFavorito(int id)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            var token = HttpContext.Request.Cookies["AuthToken"];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("No se pudo obtener el token desde las cookies.");
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userId = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int userIdValue))
            {
                return Unauthorized("El identificador del usuario en el token no es válido.");
            }

            Favorito favorito = new Favorito
            {
                FavUsuarioId = userIdValue,
                FavPost = id
            };

            string apiUrl = $"{baseApiUrl}/favoritos/ObtenerFavorito/{favorito.FavPost}/{favorito.FavUsuarioId}";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseApiUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                var data = await response.Content.ReadAsStringAsync();
                var favViewModel = JsonConvert.DeserializeObject<FavoritoViewModel>(data);

                Console.WriteLine(favViewModel.ToJson());
                if (favViewModel.FavId == 0)
                {
                   
                    Console.WriteLine("Favorito no existe, entonces lo crearemos");
                    try
                    {
                        var favoritoTask = httpClient.PostAsJsonAsync<Favorito>($"{baseApiUrl}/favoritos", favorito);
                        favoritoTask.Wait();
                        var result = favoritoTask.Result;

                        if (result.IsSuccessStatusCode)
                        {
                            // Post marcado como favorito con éxito
                            ViewData["FavoritoState"] = true;
                            Console.WriteLine("Creado correctamente");
                        }
                        else
                        {
                            Console.WriteLine("Response Content: " + result.Content.ReadAsStringAsync().Result);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        ModelState.AddModelError(string.Empty, "Ocurrió un error al marcar el post como favorito.");
                    }

                } else
                {
                    Console.WriteLine("Favorito existe, entonces lo eliminaremos");

                    try
                    {
                        var favoritoTask = httpClient.DeleteAsync($"{baseApiUrl}/favoritos/delete/{favViewModel.FavId}");
                        favoritoTask.Wait();
                        var result = favoritoTask.Result;

                        if (result.IsSuccessStatusCode)
                        {
                            // Post marcado como favorito con éxito
                            ViewData["FavoritoState"] = false;
                            Console.WriteLine("Eliminado correctamente");
                            return RedirectToAction("PostDetails", new { id });
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        ModelState.AddModelError(string.Empty, "Ocurrió un error al marcar el post como favorito.");
                    }
                }
            }

            // Actualiza el estado y redirige a la página de detalles del post
            return RedirectToAction("PostDetails", new { id });
        }
    }
}

