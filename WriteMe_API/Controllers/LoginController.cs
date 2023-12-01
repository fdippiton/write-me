using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WriteMe_API.Models;
using WriteMe_API.ViewModels;

namespace WriteMe_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly WriteMeContext _context;
        private readonly IConfiguration _config;

        public LoginController(WriteMeContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        [HttpGet]
        public IActionResult GetNewCurrentUser()
        {
            var currentUser = GetCurrentUser();
            if (currentUser != null)
            {
                return Ok($"Hola {currentUser.UsuNombre}, tu correo es {currentUser.UsuCorreo}");
            }
            else
            {
                return NotFound("Usuario no encontrado");
            }
        }

        // Inicio de sesion de usuario
        [HttpPost]
        public IActionResult Login(LoginUser userLogin)
        {
            try
            {
                var user = Authenticate(userLogin);

                if (user != null)
                {
                    var token = Generate(user);
                    return Ok(token);
                }

                return NotFound("No se pudo loguear. Usuario no encontrado");
            }
            catch (Exception ex)
            {
                // Manejar la excepción según tus necesidades
                Console.WriteLine($"Error en el método Login: {ex}");
                return StatusCode(500, "Error interno del servidor");
                // Se puede devolver un código de estado 500 (Internal Server Error) o manejar la excepción de otra manera
            }
        }

        // Autenticar usuario
        private Usuario Authenticate(LoginUser userLogin)
        {
            try
            {
                var currentUser = _context.Usuarios.FirstOrDefault(user => user.UsuCorreo == userLogin.UsuCorreo
                    && user.UsuContrasena == userLogin.UsuContrasena);

                if (currentUser != null)
                {
                    Console.WriteLine(currentUser.ToJson());
                    return currentUser;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Authenticate: {ex.Message}");
            }

            return null;
        }

        // Generar token
        private string Generate(Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Crear claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UsuId.ToString()),
                new Claim(ClaimTypes.Name, user.UsuNombre),
                new Claim(ClaimTypes.Email, user.UsuCorreo),
                new Claim("UsuStatus", user.UsuStatus),
            };

            // Crear el token
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private Usuario GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {

                var usuIdClaim = identity.FindFirst("UsuId");
                if (usuIdClaim != null && int.TryParse(usuIdClaim.Value, out var usuId))
                {
                    // Cargar el usuario desde el contexto o la base de datos utilizando el ID del usuario
                    var currentUser = _context.Usuarios.FirstOrDefault(u => u.UsuId == usuId);

                    if (currentUser != null)
                    {
                        return new Usuario
                        {
                            UsuNombre = currentUser.UsuNombre,
                            UsuCorreo = currentUser.UsuCorreo,
                            UsuStatus = currentUser.UsuStatus,
                        };
                    }
                }
            }
            return null;
        }

    }
}
