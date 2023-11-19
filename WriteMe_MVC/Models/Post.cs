namespace WriteMe_MVC.Models
{
    public class Post
    {
        public int PostId { get; set; }

        public string PostTitulo { get; set; } = null!;

        public string PostContenido { get; set; } = null!;

        public DateTime PostFechaPublicacion { get; set; }

        public int? PostUsuarioId { get; set; }

        public int? PostCategoria { get; set; }

        public string PostStatus { get; set; } = null!;
    }
}
