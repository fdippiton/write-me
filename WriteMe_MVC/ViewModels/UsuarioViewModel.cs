﻿namespace WriteMe_MVC.ViewModels
{
    public class UsuarioViewModel
    {
        public int UsuId { get; set; }
        public string UsuNombre { get; set; } = null!;
        public string UsuCorreo { get; set; } = null!;
        public string UsuContrasena { get; set; } = null!;
        public string UsuStatus { get; set; } = null!;
    }
}
