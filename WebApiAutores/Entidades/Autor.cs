namespace WebApiAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Libro Libros { get; set; } //Propiedad de navegación
    }
}
