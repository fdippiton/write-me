using System;
using System.Collections.Generic;

namespace WriteMe_API.Models;

public partial class Favorito
{
    public int FavId { get; set; }

    public int? FavUsuarioId { get; set; }

    public int? FavPost { get; set; }

    public virtual Post? FavPostNavigation { get; set; }

    public virtual Usuario? FavUsuario { get; set; }
}
