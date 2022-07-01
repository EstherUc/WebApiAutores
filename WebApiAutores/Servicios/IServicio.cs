namespace WebApiAutores.Servicios
{
    public interface IServicio
    {
        Guid ObtenerTransient();
        Guid ObtenerSingleton();
        Guid ObtenerScoped();

    }

    public class ServicioA : IServicio
    {
        private readonly ILogger<ServicioA> logger;
        private readonly ServicioTransient servicioTransient;
        private readonly ServicioSingleton servicioSingleton;
        private readonly ServicioScoped servicioScoped;

        public ServicioA(ILogger<ServicioA> logger, ServicioTransient servicioTransient, ServicioSingleton servicioSingleton,
            ServicioScoped servicioScoped)
        {
            this.logger = logger; 
            this.servicioTransient = servicioTransient; 
            this.servicioSingleton = servicioSingleton;
            this.servicioScoped = servicioScoped;
        }

        public Guid ObtenerTransient() { return servicioTransient.Guid;  }
        public Guid ObtenerSingleton() { return servicioSingleton.Guid; }
        public Guid ObtenerScoped() { return servicioScoped.Guid; }
    }

    public class ServicioB : IServicio
    {
        public Guid ObtenerScoped()
        {
            throw new NotImplementedException();
        }

        public Guid ObtenerSingleton()
        {
            throw new NotImplementedException();
        }

        public Guid ObtenerTransient()
        {
            throw new NotImplementedException();
        }

        public void RealizarTarea()
        {
            throw new NotImplementedException();
        }
    }

    public class ServicioTransient //TRANSIENT siempre te da una nueva instancia de la clase
    {
        public Guid Guid = Guid.NewGuid(); // crea un string aleaotorio
    }

    public class ServicioScoped //SCOPED te da la misma instancia en el mismo contexto HTTP
    {
        public Guid Guid = Guid.NewGuid(); // crea un string aleaotorio
    }

    public class ServicioSingleton //SINGLETON siempre te da la misma instancia, incluso en distintas peticiones HTTP, independientemente que sean usuarios distintos los que estén haciendo esas peticiones.
    {
        public Guid Guid = Guid.NewGuid(); // crea un string aleaotorio
    }
}



