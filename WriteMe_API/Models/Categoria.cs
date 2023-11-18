using System;
using System.Collections.Generic;

namespace WriteMe_API.Models;

public partial class Categoria
{
    public int CatId { get; set; }

    public string CatNombre { get; set; } = null!;

    public string CatStatus { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
