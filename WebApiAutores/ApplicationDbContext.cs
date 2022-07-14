using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;

namespace WebApiAutores
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AutoresLibro>().HasKey(al => new { al.AutorId, al.LibroId }); //crea un nuevo objeto que representa la llave primaria (compuesta) de la entidad AutoresLibro
        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; } 
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutoresLibro> AutoresLibros { get; set; }
    }
}
