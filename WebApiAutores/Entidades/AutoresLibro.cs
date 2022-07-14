namespace WebApiAutores.Entidades
{
    public class AutoresLibro
    {
        public int LibroId { get; set; }
        public int AutorId { get; set; }
        public int Orden { get; set; } //variable para determinar el orden de los autores de un libro
        public Libro Libro { get; set; } //propiedad de navegación
        public Autor Autor { get; set; } //propiedad de navegación

    }
}
