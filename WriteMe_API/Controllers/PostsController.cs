using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WriteMe_API.Models;
using WriteMe_API.ViewModels;

namespace WriteMe_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly WriteMeContext _context;

        public PostsController(WriteMeContext context)
        {
            _context = context;
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
        {
            try
            {
              if (_context.Posts == null)
              {
                  return NotFound();
              }
                var posts = await _context.Posts
                    .Include(x => x.PostCategoriaNavigation)
                    .Include(x => x.PostUsuario)
                    .Select(post => new PostViewModel
                    {
                        PostId = post.PostId,
                        PostTitulo = post.PostTitulo,
                        PostContenido = post.PostContenido,
                        PostFechaPublicacion = post.PostFechaPublicacion,
                        PostUsuarioId = post.PostUsuarioId,
                        PostUsuarioNombre = post.PostUsuario!.UsuNombre,
                        PostCategoria = post.PostCategoria,
                        PostCategoriaNombre = post.PostCategoriaNavigation!.CatNombre,
                        PostStatus = post.PostStatus
                    })
                    .ToListAsync();

                var jsonResult = JsonSerializer.Serialize(posts);

                // Devuelve el resultado serializado
                return Content(jsonResult, "application/json");
            } catch (Exception ex)
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

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PostViewModel>> GetPost(int id)
        {
          if (_context.Posts == null)
          {
              return NotFound();
          }
            var post = await _context.Posts
                    .Include(x => x.PostCategoriaNavigation)
                    .Include(x => x.PostUsuario)
                    .Where(post => post.PostId == id)
                    .Select(post => new PostViewModel
                    {
                        PostId = post.PostId,
                        PostTitulo = post.PostTitulo,
                        PostContenido = post.PostContenido,
                        PostFechaPublicacion = post.PostFechaPublicacion,
                        PostUsuarioId = post.PostUsuarioId,
                        PostUsuarioNombre = post.PostUsuario!.UsuNombre,
                        PostCategoria = post.PostCategoria,
                        PostCategoriaNombre = post.PostCategoriaNavigation!.CatNombre,
                        PostStatus = post.PostStatus
                    })
                    .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            return post;
        }

        // PUT: api/Posts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, Post post)
        {
            if (id != post.PostId)
            {
                return BadRequest();
            }

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Posts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Post>> PostPosts(Post post)
        {
          if (_context.Posts == null)
          {
              return Problem("Entity set 'WriteMeContext.Posts'  is null.");
          }
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPost", new { id = post.PostId }, post);
        }

        // DELETE: api/Posts/5
        [HttpPut("delete/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var article = await _context.Posts.FindAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            // Cambia el estado del usuario a "inactivo" (o cualquier otro valor que indique inactividad)
            article.PostStatus = "I"; // Suponiendo que "I" indica inactivo

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}
