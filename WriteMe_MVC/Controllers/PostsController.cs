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
                Console.WriteLine($"UserId: {parsedUserId}");

                ViewData["UsuarioId"] = userId;

            }

            // Especificar la URL del endpoint de la API

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

        // POST: PostsController/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> Edit(int id, [FromForm] Post post)
        {
            string baseApiUrl = _configuration.GetSection("WriteMeApi").Value!;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Console.WriteLine(post.ToJson());
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
            //return RedirectToAction("PostDetails", "Posts");
        }

        // GET: PostsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PostsController/Delete/5
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
