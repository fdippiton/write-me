using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WriteMe_API.Models;

public partial class WriteMeContext : DbContext
{
    public WriteMeContext()
    {
    }

    public WriteMeContext(DbContextOptions<WriteMeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("server=localhost; database=WriteMe; integrated security=true; TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CatId).HasName("PK__Categori__26E351400C3AEBB2");

            entity.Property(e => e.CatId).HasColumnName("Cat_Id");
            entity.Property(e => e.CatNombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Cat_Nombre");
            entity.Property(e => e.CatStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("Cat_Status");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.FavId).HasName("PK__Favorito__32266C2FB21B1163");

            entity.Property(e => e.FavId).HasColumnName("Fav_Id");
            entity.Property(e => e.FavPost).HasColumnName("Fav_Post");
            entity.Property(e => e.FavUsuarioId).HasColumnName("Fav_Usuario_Id");

            entity.HasOne(d => d.FavPostNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.FavPost)
                .HasConstraintName("FK__Favoritos__Fav_P__412EB0B6");

            entity.HasOne(d => d.FavUsuario).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.FavUsuarioId)
                .HasConstraintName("FK__Favoritos__Fav_U__403A8C7D");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Posts__5875F7AD36FB10CA");

            entity.Property(e => e.PostId).HasColumnName("Post_Id");
            entity.Property(e => e.PostCategoria).HasColumnName("Post_Categoria");
            entity.Property(e => e.PostContenido)
                .HasColumnType("text")
                .HasColumnName("Post_Contenido");
            entity.Property(e => e.PostFechaPublicacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Post_FechaPublicacion");
            entity.Property(e => e.PostStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("Post_Status");
            entity.Property(e => e.PostTitulo)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Post_Titulo");
            entity.Property(e => e.PostUsuarioId).HasColumnName("Post_Usuario_Id");

            entity.HasOne(d => d.PostCategoriaNavigation).WithMany(p => p.Posts)
                .HasForeignKey(d => d.PostCategoria)
                .HasConstraintName("FK__Posts__Post_Cate__3D5E1FD2");

            entity.HasOne(d => d.PostUsuario).WithMany(p => p.Posts)
                .HasForeignKey(d => d.PostUsuarioId)
                .HasConstraintName("FK__Posts__Post_Usua__3C69FB99");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuId).HasName("PK__Usuarios__B6173FCB1C414AB7");

            entity.Property(e => e.UsuId).HasColumnName("Usu_Id");
            entity.Property(e => e.UsuContrasena)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Usu_Contrasena");
            entity.Property(e => e.UsuCorreo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Usu_Correo");
            entity.Property(e => e.UsuNombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Usu_Nombre");
            entity.Property(e => e.UsuStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("Usu_Status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
