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
    public class FavoritosController : ControllerBase
    {
        private readonly WriteMeContext _context;

        public FavoritosController(WriteMeContext context)
        {
            _context = context;
        }

        // GET: api/Favoritos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Favorito>>> GetFavoritos(int userId)
        {
            try
            {
                if (_context.Favoritos == null)
                {
                    return NotFound();
                }
                var favoritos = await _context.Favoritos
                    .Include(x => x.FavPostNavigation)
                    .Include(x => x.FavUsuario)
                    .Where(x => x.FavUsuarioId == userId)
                    .Select(favorito => new FavoritoViewModel
                    {
                        FavId = favorito.FavId,
                        FavUsuarioId = favorito.FavUsuarioId,
                        FavUsuarioNombre = favorito.FavUsuario!.UsuNombre,
                        FavPost = favorito.FavPost,
                        FavPostTitulo = favorito.FavPostNavigation!.PostTitulo,
                    })
                    .ToListAsync();

                var jsonResult = JsonSerializer.Serialize(favoritos);

                // Devuelve el resultado serializado
                return Content(jsonResult, "application/json");

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

        // GET: api/Favoritos/5
        [HttpGet("ObtenerFavorito/{id}/{userId}")]
        public async Task<ActionResult<FavoritoViewModel>> GetFavorito(int id, int userId)
        {
          if (_context.Favoritos == null)
          {
              return NotFound();
          }
            var favorito = await _context.Favoritos
                .Include(x => x.FavPostNavigation)
                .Include(x => x.FavUsuario)
                .Where(x => x.FavPost == id)
                .Where(x => x.FavUsuarioId ==  userId)
                .Select(favorito => new FavoritoViewModel
                {
                    FavId = favorito.FavId,
                    FavUsuarioId = favorito.FavUsuarioId,
                    FavUsuarioNombre = favorito.FavUsuario!.UsuNombre,
                    FavPost = favorito.FavPost,
                    FavPostTitulo = favorito.FavPostNavigation!.PostTitulo,
                })
                .FirstOrDefaultAsync();

            if (favorito == null)
            {
                return NotFound();
            }

            return favorito;
        }

        // PUT: api/Favoritos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFavorito(int id, Favorito favorito)
        {
            if (id != favorito.FavId)
            {
                return BadRequest();
            }

            _context.Entry(favorito).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FavoritoExists(id))
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

        // POST: api/Favoritos
        [HttpPost]
        public async Task<ActionResult<Favorito>> PostFavorito(Favorito favorito)
        {
          if (_context.Favoritos == null)
          {
              return Problem("Entity set 'WriteMeContext.Favoritos'  is null.");
          }
            _context.Favoritos.Add(favorito);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFavorito", new { id = favorito.FavId }, favorito);
        }

        // DELETE: api/Favoritos/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFavorito(int id)
        {
            if (_context.Favoritos == null)
            {
                return NotFound();
            }
            var favorito = await _context.Favoritos.FindAsync(id);
            if (favorito == null)
            {
                return NotFound();
            }

            _context.Favoritos.Remove(favorito);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FavoritoExists(int id)
        {
            return (_context.Favoritos?.Any(e => e.FavId == id)).GetValueOrDefault();
        }
    }
}
