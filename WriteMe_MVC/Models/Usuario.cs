using System.ComponentModel;

namespace WriteMe_MVC.Models
{
    public class Usuario
    {
        public int UsuId { get; set; }

        [DisplayName("Nombre Completo")]
        public string UsuNombre { get; set; } = null!;

        [DisplayName("Correo")]
        public string UsuCorreo { get; set; } = null!;

        [DisplayName("Contraseña")]
        public string UsuContrasena { get; set; } = null!;

        public string UsuStatus { get; set; } = null!;
    }
}
