using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WriteMe_API.Models;

public partial class Post
{
    public int PostId { get; set; }
    [DisplayName("Titulo")]
    public string PostTitulo { get; set; } = null!;
    [DisplayName("Contenido")]
    public string PostContenido { get; set; } = null!;

    public DateTime PostFechaPublicacion { get; set; }

    public int? PostUsuarioId { get; set; }
    [DisplayName("Categoria")]
    public int? PostCategoria { get; set; }

    public string PostStatus { get; set; } = null!;

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual Categoria? PostCategoriaNavigation { get; set; }

    public virtual Usuario? PostUsuario { get; set; }
}
