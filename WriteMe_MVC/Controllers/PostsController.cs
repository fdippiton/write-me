using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
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
        [Authorize]
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
        public ActionResult Create()
        {
            return View();
        }

        // POST: PostsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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
        [ValidateAntiForgeryToken]
        public async Task <ActionResult> Edit(int id, IFormCollection collection)
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
