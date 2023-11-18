using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WriteMe_API.Models;

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
        public async Task<ActionResult<IEnumerable<Favorito>>> GetFavoritos()
        {
          if (_context.Favoritos == null)
          {
              return NotFound();
          }
            return await _context.Favoritos.ToListAsync();
        }

        // GET: api/Favoritos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Favorito>> GetFavorito(int id)
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

            return favorito;
        }

        // PUT: api/Favoritos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
        [HttpDelete("{id}")]
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
