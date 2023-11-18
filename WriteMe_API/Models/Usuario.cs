using System;
using System.Collections.Generic;

namespace WriteMe_API.Models;

public partial class Usuario
{
    public int UsuId { get; set; }

    public string UsuNombre { get; set; } = null!;

    public string UsuCorreo { get; set; } = null!;

    public string UsuContrasena { get; set; } = null!;

    public string UsuStatus { get; set; } = null!;

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
